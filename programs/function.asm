; This program tests calling and returning from a function

ldi sp, 0x0FFF
push 10
push 5
call function
hlt

function:
	pop r1
	pop r0
	add rv, r0, r1
	ret