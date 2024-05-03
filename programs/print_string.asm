;%section .text

; initialize stack pointer
ldi sp, 0x0fff

; set x, y = 0, 0
ldi r0, 0
ldi r1, 0

; load pointer to string
ldi r2, THIS_IS_A_STRING

; print string
call print_string

; exit program
hlt

; print_string - prints a null-terminated string
; r0 = x
; r1 = y
; r2 = address of string
print_string:
	; push modified registers to stack
	push ra
	push r0
	push r1
	push r2
	push r4
	push r7
	
	mov r7, r2

	print_string_loop:
		ldb r2, r7[0]
		inc r7
		
		; check for null character
		jez print_string_end, r2
		
		; check for backspace
		jeq print_string_handle_backspace, r2, 8
		
		; check for tab
		jeq print_string_handle_tab, r2, 9
		
		; check for line feed
		jeq print_string_handle_line_feed, r2, 10
			
		; check for carriage return
		jeq print_string_handle_carriage_return, r2, 13
		
		print_string_write_char:
			call print_char
			inc r0
			jmp print_string_loop
		
		print_string_handle_backspace:
			dec r0
			jmp print_string_loop
		
		print_string_handle_tab:
			addi r0, r0, 4
			jmp print_string_loop
		
		print_string_handle_line_feed:
			inc r1
			jmp print_string_loop
		
		print_string_handle_carriage_return:
			ldi r0, 0
			jmp print_string_loop

	; restore original register values
	print_string_end:
		pop r7
		pop r4
		pop r2
		pop r1
		pop r0
		pop ra
	
	; return from routine
	ret

; print_char
; r0 = x
; r1 = y
; r2 = char
; address = 0x4010
; width = 80
; height = 24
print_char:
	push r7
	
	muli r7, r1, 80
	add r7, r7, r0
	addi r7, r7, 0x4010
	stb r2, r7[0]
	
	pop r7
	ret

;%section .data

THIS_IS_A_STRING: .string "howdy world\0"
