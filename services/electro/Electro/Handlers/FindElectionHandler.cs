﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Electro.Model;
using Electro.Utils;

namespace Electro.Handlers
{
	class FindElectionHandler : AuthorizedBaseHandler
	{
		private readonly ElectroController electroController;

		public FindElectionHandler(ElectroController electroController, AuthController authController, string prefix) : base(authController, prefix)
		{
			this.electroController = electroController;
		}

		protected override void ProcessAuthorizedRequest(HttpListenerContext context, User user)
		{
			var idString = context.Request.QueryString["id"];
			Guid id;
			if(!Guid.TryParse(idString, out id))
				throw new HttpException(HttpStatusCode.BadRequest, "Invalid request params");

			var election = electroController.FindElectionForUser(id, user);

			WriteData(context, election.ToJson());
		}

	}
}
