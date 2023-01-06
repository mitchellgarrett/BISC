section .text

global main
main:
	la rt, var_dword
	ld rv, rt[0]
	ret

section .data
	var_dword: .dword 45634566534
	var_word: .word 6535
	var_byte: 255
	var_string: .string "howdy world\n\0"