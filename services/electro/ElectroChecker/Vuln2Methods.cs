﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Electro.Crypto;
using Electro.Utils;

namespace ElectroChecker
{
	static class Vuln2Methods
	{
		const int nominateTimeInSec = 3;
		const int voteTimeInSec = 10;

		public static void ProcessPut(string host, string id, string flag)
		{
			var cs = flag.Select(c => UsersManager.GenUser(null, c.ToString())).ToArray();
			var regTasks = cs.Select(candidate => new { candidate, task = ElectroClient.RegUserAsync(host, Program.PORT, candidate.Login, candidate.Pass, candidate.PublicMessage, candidate.PrivateMessage) }).ToArray();
			try
			{
				Task.WaitAll(regTasks.Select(arg => (Task)arg.task).ToArray());
			}
			catch(AggregateException e)
			{
				throw new ServiceException(ExitCode.DOWN, e.Flatten().ToString());
			}
			cs = regTasks.Select(arg => { arg.candidate.Cookies = arg.task.Result; return arg.candidate; }).ToArray();

			var election = ElectroClient.StartElection(host, Program.PORT, cs[0].Cookies, Utils.GenRandomElectionName(), false, nominateTimeInSec, voteTimeInSec);
			var electionId = election.Id;

			var now = DateTime.UtcNow;
			var nominateTasks = cs.Skip(1).Select(candidate => { return new {candidate, task = ElectroClient.NominateAsync(host, Program.PORT, candidate.Cookies, election.Id)}; });
			try
			{
				Task.WaitAll(regTasks.Select(arg => (Task)arg.task).ToArray());
			}
			catch(AggregateException e)
			{
				throw new ServiceException(ExitCode.DOWN, e.Flatten().ToString());
			}
			var elections = nominateTasks.Select(arg => arg.task.Result).ToArray();

			election = elections.FirstOrDefault(election1 => election1.Candidates.Count >= cs.Length);
			if(election == null)
				throw new ServiceException(ExitCode.MUMBLE, string.Format("Nominated '{0}' candidates for election '{1}', but got less in result", cs.Length, electionId));

			var tts = nominateTimeInSec*1000 - DateTime.UtcNow.Subtract(now).TotalMilliseconds;
			if(tts > 0)
			{
				Console.Error.WriteLine("Sleeping for {0}ms before voting (nomination duration is {1}s)", tts, nominateTimeInSec);
				Thread.Sleep((int) tts);
			}

			var candidates = election.Candidates.ToArray();
			
			var votes = flag.Select(c =>
			{
				var votePos = candidates.IndexOf(info => info.PublicMessage == c.ToString());
				if(votePos < 0)
					throw new ServiceException(ExitCode.MUMBLE, "Nominated candidates for all flag characters but in resulting election have not for all");
				return Utils.GenVoteVector(cs.Length, votePos);
			}).ToArray();

			var voters = new List<User>();
			foreach(var voteVector in votes)
			{
				var voter = UsersManager.GenRandomUser();
				voter.Cookies = ElectroClient.RegUser(host, Program.PORT, voter.Login, voter.Pass);
				voters.Add(voter);

				ElectroClient.Vote(host, Program.PORT, voter.Cookies, election.Id, HomoCrypto.EncryptVector(voteVector, election.PublicKey));
			}

			var state = new Vuln2State
			{
				ElectionId = election.Id.ToString(),
				Voter = voters[0]
			};

			Program.ExitWithMessage(ExitCode.OK, null, Convert.ToBase64String(Encoding.UTF8.GetBytes(state.ToJsonString())));
		}

		public static void ProcessGet(string host, string id, string flag)
		{
			var state = JsonHelper.ParseJson<Vuln2State>(Convert.FromBase64String(id));

			var election = ElectroClient.FindElection(host, Program.PORT, state.Voter.Cookies, state.ElectionId);
			if(election == null || election.Candidates == null)
				throw new ServiceException(ExitCode.MUMBLE, string.Format("Can't find election '{0}' or it has no candidates", id));
			var gotFlag = string.Join("", election.Candidates.WhereNotNull().Select(info => info.PublicMessage ?? ""));
			if(flag != gotFlag)
				throw new ServiceException(ExitCode.CORRUPT, string.Format("Can't find flag. Got '{0}' instead of expected", gotFlag));

			Program.ExitWithMessage(ExitCode.OK, "Flag found! OK");
		}
	}
}