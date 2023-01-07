; calculates nth Fibonacci number
; returns Fibonacci number in rv

li r0, 5       ; get nth fibo number
call fibonacci ; call fibonacci function
hlt            ; exit program

fibonacci:
	subi sp, 16  ; allocate space of stack for stored registers
	sw r0, sp[0] ; save registers modified by this function
	sw r1, sp[4]
	sw r2, sp[8]
	sw r7, sp[12]

	dec r0         ; subtract 1 from r0 as we already have first 2 numbers
	li r7, 0
	jle fibonacci_done, r0, r7 ; if n <= 0, return
	li r1, 0       ; initialize first fibo number
	li r2, 1       ; initialize second fibo number
	li rv, 0       ; initialize return value

fibonacci_loop:
	add rv, r1, r2 ; calculate next fibo number
	mov r1, r2     ; shift fibos down
	mov r2, rv
	dec r0         ; decrement loop counter
	jgt fibonacci_loop, r0, r7 ; loop if not zero

fibonacci_done:
	ld r0, sp[0] ; restore modified registers to original values
	ld r1, sp[0]
	ld r2, sp[0]
	ld r7, sp[0]
	addi sp, 16 ; deallocate stack space
	ret         ; return from function, result in rv