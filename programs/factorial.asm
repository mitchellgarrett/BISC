; returns n!
; the purpose of this program
; is to display recursion

ldi sp, 0x0FFF ; initialize stack
ldi r0, 5 ; factorial to calculate
call factorial
hlt

factorial:
	subi sp, sp, 12  ; allocate space on stack for 3 registers
	stw r0, sp[0] ; store modified registers on stack
	stw r1, sp[4]
	stw ra, sp[8] ; store ra on stack since we make a function call
	stw r0, sp[0] ; store modified registers on stack
	stw r1, sp[4]
	stw ra, sp[8] ; store ra on stack since we make a function call
	
	ldi rv, 1 ; if r0 = 0, return with value 1 (0! = 1)
	jez factorial_done, r0

	mov r1, r0     ; save r0
	dec r0         ; calculate (n-1)!
	call factorial
	mul rv, rv, r1 ; return n * (n-1)

factorial_done:
	ldw r0, sp[0] ; restore modified registers
	ldw r1, sp[4]
	ldw ra, sp[8] ; restore ra
	ldw r0, sp[0] ; restore modified registers
	ldw r1, sp[4]
	ldw ra, sp[8] ; restore ra
	addi sp, sp, 12  ; deallocate stack space
	ret
