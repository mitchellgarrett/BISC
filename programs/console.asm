; this program tests console io
; using memory-mapped operations

; turn off cursor
lli r0, 0
ldi fp, 0x4000
stb r0, fp[0]

ldi r0, 0
ldi r1, 0
ldi r2, 'H'
call write_char

inc r0
ldi r2, 'o'
call write_char

inc r0
ldi r2, 'w'
call write_char
ldi r2, 'd'

inc r0
call write_char

inc r0
ldi r2, 'y'
call write_char
;lli r0, ','
;call write_char
;lli r0, ' '
;call write_char

inc r0
inc r0
ldi r2, 'f'
call write_char

inc r0
ldi r2, 'r'
call write_char

inc r0
ldi r2, 'o'
call write_char

inc r0
ldi r2, 'm'
call write_char
;lli r0, ' '
;call write_char

inc r0
inc r0
ldi r2, 'B'
call write_char

inc r0
ldi r2, 'I'
call write_char

inc r0
ldi r2, 'S'
call write_char

inc r0
ldi r2, 'C'
call write_char
;lli r0, '!'
;call write_char

ldi r0, 0
inc r1
ldi r2, 'L'
call write_char

inc r0
ldi r2, 'e'
call write_char

inc r0
ldi r2, 't'
call write_char
;lli r0, '''
;call write_char

inc r0
ldi r2, 's'
call write_char

inc r0
inc r0
ldi r2, 'g'
call write_char

inc r0
ldi r2, 'o'
call write_char

;inc r0
;lli r0, '!'
;call write_char

; read char
call read_char

; reset cursor
ldi r0, 1
stb r0, fp[0]
hlt

; write char
write_char:
	push r7
	muli r7, r1, 80
	add r7, r7, r0
	addi r7, r7, 0x4010
	stb r2, r7[0]
	pop r7
	ret

; read char
read_char:
	push r7
	ldi r7, 0x4000
	ldb rv, r7[12]
	pop r7
	ret