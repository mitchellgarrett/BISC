; this program tests a basic loop
ldi r0, 0
ldi t0, 10
loop:
	add r0, r0, t0
	dec t0
	jge loop, t0, 0
hlt