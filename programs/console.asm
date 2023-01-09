; this program tests console io
; using memory-mapped operations

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
hlt