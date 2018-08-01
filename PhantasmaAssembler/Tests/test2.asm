LOAD r1, 0
LOAD r2, 10
@test: INC r1
PUSH r1
EXTCALL "Runtime.Log"
LTE r1, r2, r3
JMPIF r3, @test
RET