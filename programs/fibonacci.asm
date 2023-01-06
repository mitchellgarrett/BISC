; Calculates nth Fibonacci number
; Returns Fibonacci number in rv

push 5         ; get nth fibo number
call fibonacci ; call fibonacci function
hlt            ; exit program

fibonacci:
	pop r0 
	dec r0         ; Subtract 1 from r0 as we already have first 2 numbers
	li r7, 0
	jle fibonacci_done, r0, r7 ; if n <= 0, return
	li r1, 0       ; Initialize first fibo number
	li r2, 1       ; Initialize second fibo number
	li rv, 0       ; Initialize return value
fibonacci_loop:
	add rv, r1, r2 ; Calculate next fibo number
	mov r1, r2     ; Shift fibos down
	mov r2, rv
	dec r0         ; Decrement loop counter
	jgt fibonacci_loop, r0, r7 ; Loop if not zero
fibonacci_done:
	ret            ; return from function, result in rv