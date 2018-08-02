POP r1
LOAD r2, "symbol"
EQUAL r1, r2, r3
JMPNOT r3, @next_1
LOAD r0, "SOUL"
RET r0
@next_1:
THROW