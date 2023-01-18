li sp, 0
addi r1, r0, 0xffff
call 0x1111
hlt

function:
jez 0x1111, r0
ret