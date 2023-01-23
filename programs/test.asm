li r0, 5
call function
hlt

function:
	dec r0
	jnz function, r0
	ret