; this program plays the classic minesweeper game
; uses the console and random number generation modules

; memory map
; stack 0x0000 - 0x1000
; terminal 0x1004 - 0x2780
; rng 0x3000
; board 0x0000

ldi sp, 0x1000
ldi fp, 0x1780

call play_game
;call print_board

;ldi r0, 0
;stb r0, fp[4]
;ldi r0, 23
;stb r0, fp[8]
ldi r0, 1
stb r0, fp[0]
;ldb r0, fp[16]
hlt

; play_game
play_game:
	push ra
	
	; initialize board
	ldi t0, 0
	ldi t1, 81
	play_game_init_board_loop:
		ldi ta, 0x3000
		stb t3, ta[4]
		jlt play_game_if0_false, t3, 128
			ldi t3, '*'
			jmp play_game_if0_end
		play_game_if0_false:
			ldi t3, '.'
		play_game_if0_end:
		;ldi t3, 'x'
		stb t3, t0[0]
		inc t0
		dec t1
		jge play_game_init_board_loop, t1, 0
	
	call print_board
	; number of mines
	ldi r0, 5
	
	pop ra
	ret

; print_board
print_board:
	push ra
	push r0
	push r1
	push r2
	
	ldi t0, 0
	ldi r1, 9
	print_board_y_loop:
		ldi r0, 9
		print_board_x_loop:
			ldb r2, t0[0]
			inc t0
			call print_char
			dec r0
			jge print_board_x_loop, r0, 0
		dec r1
		jge print_board_y_loop, r1, 0
		
	pop ra
	pop r0
	pop r1
	pop r2
	ret

; write char
; r0 = x
; r1 = y
; r2 = char
; address = 0x1000
; width = 80
; height = 24
print_char:
	muli t0, r1, 80
	add t0, t0, r0
	addi t0, t0, 0x1000
	stb r2, t0[0]
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