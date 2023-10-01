; this program plays the classic tic tac toe game
; uses the console and random number generation modules

; memory map
; stack 0x0000 - 0x1000
; terminal 0x1000 - 0x2780
; rng 0x3000
; board 0x0000

ldi sp, 0x1000 ; initialize the stack to the end of ram
ldi gp, 0x3000 ; set the global pointer to point to the rng module
ldi fp, 0x0000 ; set the frame pointer to the beginning of ram

call print_board
call play_game
hlt

play_game:
	push ra
	push r6
	push r7
	
	; use r7 as turn flag
	; 0 for Xs, 1 for Os
	ldi r7, 0
	
	; use r6 as game-over flag
	ldi r6, 1
	
	ldi t0, 0x1000
	ldi t1, 1
	stw t1, t0[0]
	ldi t1, 0
	stw t1, t0[4]
	stw t1, t0[8]
	
	play_game_loop:
		play_game_get_input:
			call get_char
			jne play_game_get_input, rv, 32
		ldi r0, 0
		ldi r1, 0
		ldi r2, 'X'
		call print_char
		
		jez play_game_loop, r6
	
	pop r7
	pop r6
	pop ra
	ret

print_board:
	push ra
	
	ldi r0, 1
	ldi r1, 0
	ldi r2, '|'
	call print_char
	
	ldi r0, 3
	ldi r1, 0
	ldi r2, '|'
	call print_char
	
	ldi r0, 0
	ldi r1, 1
	ldi r2, '-'
	call print_char
	
	ldi r0, 1
	ldi r1, 1
	ldi r2, '+'
	call print_char
	
	ldi r0, 2
	ldi r1, 1
	ldi r2, '-'
	call print_char
	
	ldi r0, 3
	ldi r1, 1
	ldi r2, '+'
	call print_char
	
	ldi r0, 4
	ldi r1, 1
	ldi r2, '-'
	call print_char
	
	ldi r0, 1
	ldi r1, 2
	ldi r2, '|'
	call print_char
	
	ldi r0, 3
	ldi r1, 2
	ldi r2, '|'
	call print_char
	
	ldi r0, 0
	ldi r1, 3
	ldi r2, '-'
	call print_char
	
	ldi r0, 1
	ldi r1, 3
	ldi r2, '+'
	call print_char
	
	ldi r0, 2
	ldi r1, 3
	ldi r2, '-'
	call print_char
	
	ldi r0, 3
	ldi r1, 3
	ldi r2, '+'
	call print_char
	
	ldi r0, 4
	ldi r1, 3
	ldi r2, '-'
	call print_char
	
	ldi r0, 1
	ldi r1, 4
	ldi r2, '|'
	call print_char
	
	ldi r0, 3
	ldi r1, 4
	ldi r2, '|'
	call print_char
	
	pop ra
	ret

; print_char
; r0 = x
; r1 = y
; r2 = char
; address = 0x1000
; width = 80
; height = 24
print_char:
	muli t0, r1, 80
	add t0, t0, r0
	addi t0, t0, 0x1010
	stb r2, t0[0]
	ret

; print_string
; r0 = x
; r1 = y
; r2 = memory address
; r3 = length
print_string:
	push ra
	push r0
	push r3
	print_string_loop:
		inc r0
		dec r3
	jgt print_string_loop, 0, r3
	
	pop r3
	pop r0
	pop ra
	ret

; get_char
get_char:
	ldi t0, 0x1000
	ldb rv, t0[12]
	ret