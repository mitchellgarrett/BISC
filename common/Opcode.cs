﻿namespace FTG.Studios.BISC {

    /// <summary>
    /// Byte-encoded enum of each BISC opcode.
    /// </summary>
    public enum Opcode : byte {
        HLT = 0x00, NOP = 0x01, SYS = 0x02, CALL = 0x03, RET = 0x4,
        LLI = 0x05, LUI = 0x06, MOV = 0x07,
		LDW = 0x08, LDH = 0x09, LDB = 0x0A, STW = 0x0B, STH = 0x0C, STB = 0x0D,
		ADD = 0x0E, SUB = 0x0F, MUL = 0x10, MULH = 0x11, MULHU = 0x12, DIV = 0x13, DIVU = 0x14, MOD = 0x15, MODU = 0x16,
		NOT = 0x17, NEG = 0x18, INV = 0x19, 
		AND = 0x1A, OR = 0x1B, XOR = 0x1C, BSL = 0x1D, BSR = 0x1E,
        JMP = 0x1F, JEZ = 0x20, JNZ = 0x21, JEQ = 0x22, JNE = 0x23, 
		JGT = 0x24, JLT = 0x25, JGE = 0x26, JLE = 0x27,
		JGTU = 0x28, JLTU = 0x29, JGEU = 0x2A, JLEU = 0x2B
    };
}
