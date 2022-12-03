; Calculates nth Fibonacci number, set in r0
; Returns Fibonacci number in rv

lli r0, 7      ; Get 7th fibo number   
subi r0, r0, 2 ; Subtract 2 from r0 as we already have first 2 numbers
lli r1, 0      ; Initialize first fibo number
lli r2, 1      ; Initialize second fibo number
lli rv, 0      ; Initialize return value
lli ra, 24     ; Address to loop to
add rv, r1, r2 ; Calculate next fibo number
mov r1, r2     ; Shift fibos down
mov r2, rv
subi r0, r0, 1 ; Decrement loop counter
jnz ra, r0     ; Loop if not zero
hlt