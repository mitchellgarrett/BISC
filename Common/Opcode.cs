namespace FTG.Studios.BISC {

    /// <summary>
    /// Byte-encoded enum of each BISC opcode.
    /// </summary>
    public enum Opcode : byte {
        NOP = 0x00, HLT = 0x01, SYS = 0x02,
        LLI = 0x03, LUI = 0x04, MOV = 0x05,
		LD = 0x06, LH = 0x07, LB = 0x08, ST = 0x09, SH = 0x0A, SB = 0x0B,
		ADD = 0x0C, SUB = 0x0D, MUL = 0x0E, DIV = 0x0F, MOD = 0x10, 
		NEG = 0x11, NOT = 0x12, INV = 0x13, 
		AND = 0x14, OR = 0x15, XOR = 0x16, BSL = 0x17, BSR = 0x18,
        JMP = 0x19, JEZ = 0x1A, JNZ = 0x1B, JEQ = 0x1C, JNE = 0x1D, JLT = 0x1E, JLE = 0x1F
    };
	
	public enum PseudoOpcode : byte {
		LI, LA, ADDI, SUBI, MULI, DIVI, MODI, INC, DEC, PUSH, POP
	}
}
