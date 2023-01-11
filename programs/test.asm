li sp, 0
lui r0, 55 ; this is a comment
lui r1, 0xff
lui r2, 'A'
add r2, r0, r1
sw r2, sp[0b1111]
function:
ret