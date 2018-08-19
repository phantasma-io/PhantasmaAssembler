//context call test
CTX r1, "token""
LOAD r2, "test"
PUSH r2
SWITCH r1
EXTCALL "Runtime.Log"
RET