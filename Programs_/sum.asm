; returns the sum of all integers
; from 1 to n

lli r0, 20000000
lli rv, 0
lli t0, 1

sum_loop:
	add rv, rv, r0
	sub r0, r0, t0
	jnz sum_loop, r0

hlt
