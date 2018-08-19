//context call test
CTX r1, "token"
LOAD r2, "BalanceOf"
PUSH r2
LOAD r3, 123
PUSH r3
SWITCH r1
EXTCALL "Runtime.Log"
RET