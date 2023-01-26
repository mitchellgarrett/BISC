; this program tests console io
; using memory-mapped operations

; turn off cursor
lli fp, 0x0780
lli r0, 0
sb r0, fp[0]

lli sp, 0
lli r0, 'H'
sb r0, sp[0]
lli r0, 'o'
sb r0, sp[1]
lli r0, 'w'
sb r0, sp[2]
lli r0, 'd'
sb r0, sp[3]
lli r0, 'y'
sb r0, sp[4]
;lli r0, ','
;sb r0, sp[5]
;lli r0, ' '
;sb r0, sp[6]
lli r0, 'f'
sb r0, sp[6]
lli r0, 'r'
sb r0, sp[7]
lli r0, 'o'
sb r0, sp[8]
lli r0, 'm'
sb r0, sp[9]
lli r0, 'B'
sb r0, sp[11]
lli r0, 'I'
sb r0, sp[12]
lli r0, 'S'
sb r0, sp[13]
lli r0, 'C'
sb r0, sp[14]
lli r0, '!'
sb r0, sp[15]

lli sp, 120
lli r0, 'L'
sb r0, sp[0]
lli r0, 'e'
sb r0, sp[1]
lli r0, 't'
sb r0, sp[2]
lli r0, '''
sb r0, sp[3]
lli r0, 's'
sb r0, sp[4]
lli r0, 'g'
sb r0, sp[6]
lli r0, 'o'
sb r0, sp[7]
lli r0, '!'
sb r0, sp[8]

; read char
lb r0, fp[3]
sb r0, sp[40]
hlt