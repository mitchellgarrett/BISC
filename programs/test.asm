%section .text

;%define THIS_IS_A_CONSTANT 65

ldi r0, THIS_IS_A_STRING
hlt ; this is the end of the program

%section .data

THIS_IS_A_STRING: .string "aa aa\0"
