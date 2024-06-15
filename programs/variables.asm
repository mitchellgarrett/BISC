;%section .text

ldi r0, THIS_IS_A_BYTE
ldi r1, THIS_IS_A_HALF
ldi r2, THIS_IS_A_WORD
hlt

;%section .data

THIS_IS_A_BYTE: .byte 0x69
THIS_IS_A_HALF: .half 0xdead
THIS_IS_A_WORD: .word 0xbabecafe
THIS_IS_A_STRING: .string "AAAAA\0"