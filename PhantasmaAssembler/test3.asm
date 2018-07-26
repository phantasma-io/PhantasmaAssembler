LOAD r0, 2
LOAD r1, 3
PUSH r0
PUSH r1
CALL @sum
EXTCALL "Runtime.Log"
RET
@sum: POP r1
POP r0
ADD r0, r1, r3
PUSH r3
RET