; this program tests the random number generation (rng) module

ldi sp, 0x1000
ldi gp, 0x1000
ldi fp, 0x3000

ldi r0, 'V'
call print_char
ldi r0, 'a'
call print_char
ldi r0, 'l'
call print_char
ldi r0, 'u'
call print_char
ldi r0, 'e'
call print_char
ldi r0, '='
call print_char

ldb r0, fp[4]
call print_integer

hlt

print_char:
	stb r0, gp[0]
	inc gp
	ret

; recursively print an integer in the correct order
print_integer:
	push ra
	push r0 ; r0 is pushed twice because its value is manipulated twice
	push r0
	
	jlt print_integer_end, r0, 10 ; if r0 < 10, do not loop
		divi r0, r0, 10 ; divide r0 by 10 to get previous digit 
		call print_integer ; recursively call this function
	print_integer_end:
		pop r0
		modi r0, r0, 10 ; mod r0 by 10 and add 48 to get ascii character
		addi r0, r0, 48
		call print_char ; print digit
	
	pop r0
	pop ra
	ret