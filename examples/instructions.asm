nop
lli r0, 20
jmp r0
lli rv, 0b10010011
lui rv, 0xFffF
add r0, r1, rv
lli r0, 10
lli r1, 2
mul r2, r0, r1
div r3, r0, r1
lli r1, 3
mod r4, r0, r1
lli r1, 0xFFFF
lui r1, 0xFFFF
inv r1, r1
lli r0, 0xF0F0
lli r1, 0xF00F
and r5, r0, r1
or r6, r0, r1
xor r7, r0, r1
hlt