; Calculates nth Fibonacci number, set in r0
; Returns Fibonacci number in rv

fibonacci:
	li r0, 7       ; Get 7th fibo number   
	subi r0, r0, 2 ; Subtract 2 from r0 as we already have first 2 numbers
	li r1, 0       ; Initialize first fibo number
	li r2, 1       ; Initialize second fibo number
	li rv, 0       ; Initialize return value
	li r7, 48      ; Address to loop to
fibonacci_loop:
	add rv, r1, r2 ; Calculate next fibo number
	mov r1, r2     ; Shift fibos down
	mov r2, rv
	dec r0         ; Decrement loop counter
	jnz r7, r0     ; Loop if not zero
	hlt