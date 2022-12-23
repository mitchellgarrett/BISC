namespace FTG.Studios.BISC {

    /// <summary>
    /// Byte-encoded enum of each BISC opcode.
    /// </summary>
    public enum Opcode : byte {
        NOP = 0x00, HLT = 0x01, SYS = 0x02, CALL = 0x03, RET = 0x04,
        LLI = 0x05, LUI = 0x06, MOV = 0x07, 
		ADD = 0x08, SUB = 0x09, MUL = 0x0A, DIV = 0x0B, MOD = 0x0C, 
		NEG = 0x0D, NOT = 0x0E, INV = 0x0F, AND = 0x10, OR = 0x11, XOR = 0x12, BSL = 0x13, BSR = 0x14,
        JMP = 0x15, JZ = 0x16, JNZ = 0x17, JE = 0x18, JNE = 0x19, JGT = 0x1A, JLT = 0x2B, JGE = 0x2C, JLE = 0x2D
    };
	
	public enum PseudoOpcode : byte {
		LI, LA, ADDI, SUBI, MULI, DIVI, MODI, INC, DEC
	}
}
