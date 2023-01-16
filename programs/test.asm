li sp, 0
push sp
lui r0, 55 ; this is a comment
lui r1, 0xff
lui r2, 'A'
add r2, r0, r1
addi r2, 0xffff, r3
sw r2, sp[0b1111]
function:
pop sp
ret