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
	muli t0, r1, 80
	add t0, t0, r0
	addi t0, t0, 0x4010
	stb r2, t0[0]
	ret

; read char
read_char:
	ldi t0, 0x4000
	ldb rv, t0[12]
	ret