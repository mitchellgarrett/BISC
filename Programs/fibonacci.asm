; Calculates nth Fibonacci number, set in r0
; Returns Fibonacci number in rv

lli r0, 7      ; Get 7th fibo number
lli r3, 2      ; Subtract 2 from r0 as we already have first 2 numbers
sub r0, r0, r3
lli r1, 0      ; Initialize first fibo number
lli r2, 1      ; Initialize second fibo number
lli r3, 1      ; Use this for decrements
lli rv, 0      ; Initialize return value
lli ra, 32     ; Address to loop to
add rv, r1, r2 ; Calculate next fibo number
mov r1, r2     ; Shift fibos down
mov r2, rv
sub r0, r0, r3 ; Decrement loop counter
jnz ra, r0     ; Loop if not zero
hlt