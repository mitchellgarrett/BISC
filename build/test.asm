; testing out other register configurations

function:
	push ap
	
	ldi ap, 0x1000
	
	stw r0, ap[4]
	
	