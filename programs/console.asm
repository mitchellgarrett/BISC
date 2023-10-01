; this program tests console io
; using memory-mapped operations

; set up pointers
lli fp, 0x0780
lli sp, 0

; turn off cursor
lli r0, 0
stb r0, fp[0]


lli r0, 'H'
call write_char

lli r0, 'o'
call write_char
lli r0, 'w'
call write_char
lli r0, 'd'
call write_char
lli r0, 'y'
call write_char
;lli r0, ','
;call write_char
;lli r0, ' '
;call write_char
inc sp
lli r0, 'f'
call write_char
lli r0, 'r'
call write_char
lli r0, 'o'
call write_char
lli r0, 'm'
call write_char
;lli r0, ' '
;call write_char
inc sp
lli r0, 'B'
call write_char
lli r0, 'I'
call write_char
lli r0, 'S'
call write_char
lli r0, 'C'
call write_char
;lli r0, '!'
;call write_char

lli sp, 120
lli r0, 'L'
call write_char
lli r0, 'e'
call write_char
lli r0, 't'
call write_char
;lli r0, '''
;call write_char
lli r0, 's'
call write_char
lli r0, 'g'
call write_char
lli r0, 'o'
call write_char
;lli r0, '!'
;call write_char

; read char
call read_char
stb rv, sp[40]
hlt

; write char
write_char:
	stb r0, sp[0]
	inc sp
	ret

; read char
read_char:
	ldb rv, fp[3]
	ret