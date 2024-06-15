; this program tests a basic loop
ldi r0, 0
ldi r1, 10
loop:
	add r0, r0, r1
	dec r1
	jge loop, r1, 0
hlt