%section .text

;%define THIS_IS_A_CONSTANT 65
mov r0, r9
ldi r0, THIS_IS_A_STRING
hlt

%section .data

THIS_IS_A_STRING: .string "aa aa\0"
