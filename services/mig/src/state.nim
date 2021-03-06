import os, times, marshal
import utils/utils

type
    State* = ref StateObj
    StateObj* = object
        start*: Time
        join*: Time
        user*: UserDataObj
        page*: int
        offer*: bool
        private*: bool
        login*: string

    UserDataObj* = object
        name*: string
        sname*: string
        bdate*: string
        bplace*: string
        occup*: string
        empl*: string
        thought*: string
        phone*: string

    JoinInfo* = object
        login*: string
        join*: Time
        private*: bool
        name*: string
        sname*: string
        occup*: string
        thought*: string

proc newState*(login: string): State =
    State(start: getTime(), page: 1, login: login)

proc tryParseState*(json: string): State =
    if isNil(json): return nil
    try:
        let obj = to[StateObj](json)
        result = new State
        result[] = obj
    except:
        return nil

proc `$`*(state: State): string =
    $$(state[])

proc newJoinInfo*(state: State): JoinInfo =
    if isNil(state): JoinInfo()
    else: JoinInfo(
        login: state.login,
        join: state.join,
        name: state.user.name,
        sname: state.user.sname,
        occup: state.user.occup,
        thought: state.user.thought,
        private: state.private)

proc tryParseJoinInfo*(json: string): JoinInfo =
    if isNil(json): return JoinInfo()
    try: return to[JoinInfo](json) except: return JoinInfo()

proc `$`*(info: JoinInfo): string =
    $$info

when isMainModule:
    assert tryParseState("{") == nil
    assert tryParseState("}") == nil
    assert tryParseState("\"") == nil

    assert tryParseState("{}") != nil

    import times

    const Iterations = 10000
    var state = $newState("test")

    var start = cpuTime()
    for i in 0..Iterations:
        assert tryParseState(state) != nil
    echo "[state] ", Iterations, " iterations CPU time [s] ", cpuTime() - start

    echo "[state] OK: compiled ", CompileDate, " ", CompileTime
