//context call test
CTX r1, "token"
LOAD r3, 123
PUSH r3
LOAD r0, "Name"
PUSH r0
SWITCH r1
EXTCALL "Runtime.Log"
RET