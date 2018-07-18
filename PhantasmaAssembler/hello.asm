load r1, "Hello"
load r2, "World"
cat r1, r2, r3
push r3
extcall "Runtime.Log"
ret