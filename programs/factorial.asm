; returns n!
; the purpose of this program
; is to display recursion

li r0, 5 ; factorial to calculate
call factorial
hlt

factorial:
	subi sp, sp, 12  ; allocate space on stack for 3 registers
	sw r0, sp[0] ; store modified registers on stack
	sw r1, sp[4]
	sw ra, sp[8] ; store ra on stack since we make a function call
	
	li rv, 1 ; if r0 = 0, return with value 1 (0! = 1)
	jez factorial_done, r0

	mov r1, r0     ; save r0
	dec r0         ; calculate (n-1)!
	call factorial
	mul rv, rv, r1 ; return n * (n-1)

factorial_done:
	lw r0, sp[0] ; restore modified registers
	lw r1, sp[4]
	lw ra, sp[8] ; restore ra
	addi sp, sp, 12  ; deallocate stack space
	ret