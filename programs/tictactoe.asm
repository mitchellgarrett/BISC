; this program plays the classic tic tac toe game
; uses the console and random number generation modules

; memory map
; stack 0x0000 - 0x1000
; terminal 0x4000 - 0x4780
; rng 0x5000
; board 0x0000

ldi sp, 0x1000 ; initialize the stack to the end of ram
ldi gp, 0x3000 ; set the global pointer to point to the rng module
ldi fp, 0x0000 ; set the frame pointer to the beginning of ram

call print_board
call init_board
call play_game
hlt

play_game:
	push ra
	push r0
	push r1
	push r2
	push r3
	push r4
	push r5
	push r6
	push r7
	
	; use r7 as turn flag
	; 1 for Xs, 2 for Os
	ldi r7, 1
	
	; use r6 to track remaining tiles
	ldi r6, 9
	
	; use r5 as cursor position index
	ldi r5, 0
	
	play_game_loop:
	
		; loop to get player input
		play_game_get_input:
			; convert board position index into screen position
			; x = (r5 % 3) * 2
			modi r0, r5, 3
			muli r0, r0, 2
			; y = (r5 / 3) * 2
			divi r1, r5, 3
			muli r1, r1, 2
			
			; set cursor position to selected square
			call set_cursor_pos
			
			; get player input
			call get_char
			
			; if player pressed'q', quit game
			jeq play_game_end, rv, 'q'
			
			; if player pressed enter, accept input and move on
			jeq play_game_check_input, rv, 10
			
			; if player pressed space, move to next square
			jne play_game_get_input, rv, 32
			
			; increment r5 to move to next board index
			inc r5
			; mod r5 by 9 to wrap around to beginning
			modi r5, r5, 9
			
			jmp play_game_get_input
		
			; check that the selected tile is empty
			; if not, input is invalid
			play_game_check_input:
				; load board address
				ldi r2, 0x1000
				
				; get value of tile at board index
				add r2, r2, r5
				ldb r3, r2[0]
				
				; if tile is not 0, input is invalid
				jnz play_game_get_input, r3
		
				; write player value to board index
				stb r7, r2[0]
		
		; default by printing X
		ldi r2, 'X'
		; if r7 == 2, then print an O
		jeq play_game_print_turn, r7, 1
		ldi r2, 'O'
		
		; print player's selection on the board
		play_game_print_turn:
			call print_char
		
		; check if there is a winner
		; if return value is non-zero, game is over
		call check_board
		jnz play_game_end, rv
		
		; select next player
		jeq play_game_select_o, r7, 1
		ldi r7, 1
		jmp play_game_loop_end
		play_game_select_o:
			ldi r7, 2
		
		play_game_loop_end:
		
		; decrement number of remaining tiles
		dec r6
		
		; if r6 != 0, start next player's turn
		; otherwise, no tiles are left and game is over
		jnz play_game_loop, r6
	
	play_game_end:
		pop r7
		pop r6
		pop r5
		pop r4
		pop r3
		pop r2
		pop r1
		pop r0
		pop ra
		ret

; check board to see if there is a winner
; returns 1 if X wins, 2 is O, 0 if none
check_board:
	push r0
	push r1
	push r2
	
	; initialize return value to 0
	ldi rv, 0
	
	; board address
	ldi r0, 0x1000
	
	; player to check
	ldi r1, 1
	
	check_board_begin_checks:
	
		; check horizontals
		check_board_first_horizontal:
			ldb r2, r0[0]
			jne check_board_second_horizontal, r1, r2
			ldb r2, r0[1]
			jne check_board_second_horizontal, r1, r2
			ldb r2, r0[2]
			jne check_board_second_horizontal, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		check_board_second_horizontal:
			ldb r2, r0[3]
			jne check_board_third_horizontal, r1, r2
			ldb r2, r0[4]
			jne check_board_third_horizontal, r1, r2
			ldb r2, r0[5]
			jne check_board_third_horizontal, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		check_board_third_horizontal:
			ldb r2, r0[3]
			jne check_board_first_vertical, r1, r2
			ldb r2, r0[4]
			jne check_board_first_vertical, r1, r2
			ldb r2, r0[5]
			jne check_board_first_vertical, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		; check verticals
		check_board_first_vertical:
			ldb r2, r0[0]
			jne check_board_second_vertical, r1, r2
			ldb r2, r0[3]
			jne check_board_second_vertical, r1, r2
			ldb r2, r0[6]
			jne check_board_second_vertical, r1, r2
			
			mov rv, r1
			jmp check_board_end
			
		check_board_second_vertical:
			ldb r2, r0[1]
			jne check_board_third_vertical, r1, r2
			ldb r2, r0[4]
			jne check_board_third_vertical, r1, r2
			ldb r2, r0[7]
			jne check_board_third_vertical, r1, r2
			
			mov rv, r1
			jmp check_board_end

		check_board_third_vertical:
			ldb r2, r0[2]
			jne check_board_first_diagonal, r1, r2
			ldb r2, r0[5]
			jne check_board_first_diagonal, r1, r2
			ldb r2, r0[8]
			jne check_board_first_diagonal, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		; check diagonals
		check_board_first_diagonal:
			ldb r2, r0[0]
			jne check_board_second_diagonal, r1, r2
			ldb r2, r0[4]
			jne check_board_second_diagonal, r1, r2
			ldb r2, r0[8]
			jne check_board_second_diagonal, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		check_board_second_diagonal:
			ldb r2, r0[2]
			jne check_board_end_checks, r1, r2
			ldb r2, r0[4]
			jne check_board_end_checks, r1, r2
			ldb r2, r0[6]
			jne check_board_end_checks, r1, r2
			
			mov rv, r1
			jmp check_board_end
		
		check_board_end_checks:
			inc r1
			jle check_board_begin_checks, r1, 2
		
	check_board_end:
		pop r2
		pop r1
		pop r0
		ret

; initialize 9 bytes in memory to 0 to represent game board
init_board:
	push r0
	push r1
	
	ldi r0, 0x1000
	ldi r1, 0
	
	; initialize 8 bytes to store board values
	; 0 = none, 1 = X, 2 = O
	stb r1, r0[0]
	stb r1, r0[1]
	stb r1, r0[2]
	stb r1, r0[3]
	stb r1, r0[4]
	stb r1, r0[5]
	stb r1, r0[6]
	stb r1, r0[7]
	stb r1, r0[8]
	
	pop r1
	pop r0
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
	addi t0, t0, 0x4010
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
	ldi t0, 0x4000
	ldb rv, t0[12]
	ret
	
; set_cursor_pos
; r0 = x
; r1 = y
set_cursor_pos:
	ldi t0, 0x4000
	stw r0, t0[4]
	stw r1, t0[8]
	ret
