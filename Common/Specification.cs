using System;

namespace FTG.Studios.BISC {

    public static class Specification {

        public const char COMMENT = ';';
		public const char LABEL_DELIMETER = ':';

        public static readonly string[] REGISTER_NAMES = {
            "pc", "sp", "rv", "rt", "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7"
        };

		public static int NUM_REGISTERS { get {return REGISTER_NAMES.Length; } }
		
		public static UInt16 AssembleInteger16(byte a, byte b) {
            if (BitConverter.IsLittleEndian) return BitConverter.ToUInt16(new byte[] { b, a }, 0);
            return BitConverter.ToUInt16(new byte[] { a, b }, 0);
		}
		
		public static UInt32 AssembleInteger32(byte a, byte b, byte c, byte d) {
            if (BitConverter.IsLittleEndian) return BitConverter.ToUInt32(new byte[] { d, c, b, a }, 0);
            return BitConverter.ToUInt32(new byte[] { a, b, c, d }, 0);
		}
		
		public static byte[] DisassembleInteger16(UInt16 value) {
			byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
		}
		
		public static byte[] DisassembleInteger32(UInt32 value) {
			byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
		}
		
        public readonly static ArgumentType[][] argument_types = {
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // NOP
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // HLT
            new ArgumentType[] { ArgumentType.None, ArgumentType.IntegerImmediate },                    // SYS
            new ArgumentType[] { ArgumentType.Register, ArgumentType.None },                            // CALL
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // RET

            new ArgumentType[] { ArgumentType.Register, ArgumentType.IntegerImmediate },                // LLI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.IntegerImmediate },                // LUI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // MOV
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LD
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LH
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LB
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // ST
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // SH
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // SB
			
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // ADD
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // SUB
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // MUL
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // DIV
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // MOD

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // NOT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // NEG
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // INV

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // AND
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // OR
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // XOR
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // BSL
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // BSR

            new ArgumentType[] { ArgumentType.Register, ArgumentType.None, ArgumentType.None },         // JMP
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JNZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JNE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLE
        };
		
		public readonly static string[][] pseudo_instructions = {
			
			// CALL {imm}
			new string[] {
				"CALL {imm}",
				"LA rt", "CALL rt"
			},
			
			// PUSH {reg}
			new string[] { 
				"PUSH {reg}",
				"ST {1}, sp[-4]", "SUBI sp, sp, 4"
			},
			
			// PUSHH {reg}
			new string[] { 
				"PUSHH {reg}",
				"SH {1}, sp[-2]", "SUBI sp, sp, 2"
			},
			
			// PUSHB {reg}
			new string[] {
				"PUSHB {reg}",
				"SB {1}, sp[-1]", "DEC sp"
			},
			
			// POP {reg}
			new string[] { 
				"POP {reg}",
				"ADDI sp, sp, 4", "LD {1}, sp[-4]"
			},
			
			// POPH {reg}
			new string[] {
				"POPH {reg}",
				"ADDI sp, sp, 2", "LD {1}, sp[-2]"
			},
			
			// POPB {reg}
			new string[] { 
				"OPB {reg}",
				"INC sp", "LB {1}, sp[-1]"
			},
			
			// LI {imm}
			new string[] { 
				"LI {imm}",
				"LLI {1}, {2}(0:15)", "LUI {1}, {2}(16:31)"
			},
			
			// LA {imm}
			new string[] { 
				"LA {imm}",
				"LI {1}, {2}"
			},
			
			// ADDI {reg}, {reg}, {imm}
			new string[] { 
				"ADDI {reg}, {reg}, {imm}",
				"LI rt, {3}", "ADD {1}, {2}, rt"
			},
			
			// ADDI {reg}, {imm}, {reg}
			new string[] { 
				"ADDI {reg}, {imm}, {reg}",
				"LI rt, {2}", "ADD {1}, rt, {3}"
			},
			
			// SUBI {reg}, {reg}, {imm}
			new string[] { 
				"SUBI {reg}, {reg}, {imm}",
				"LI rt, {3}", "SUB {1}, {2}, rt"
			},
			
			// SUBI {reg}, {imm}, {reg}
			new string[] { 
				"SUBI {reg}, {imm}, {reg}",
				"LI rt, {2}", "SUB {1}, rt, {3}"
			},
			
			// MULI {reg}, {reg}, {imm}
			new string[] { 
				"MULI {reg}, {reg}, {imm}",
				"LI rt, {3}", "MUL {1}, {2}, rt"
			},
			
			// MULI {reg}, {imm}, {reg}
			new string[] { 
				"MULI {reg}, {imm}, {reg}",
				"LI rt, {2}", "MUL {1}, rt, {3}"
			},
			
			// DIVI {reg}, {reg}, {imm}
			new string[] { 
				"DIVI {reg}, {reg}, {imm}",
				"LI rt, {3}", "DIV {1}, {2}, rt"
			},
			
			// DIVI {reg}, {imm}, {reg}
			new string[] { 
				"DIVI {reg}, {imm}, {reg}",
				"LI rt, {2}", "DIV {1}, rt, {3}"
			},
			
			// MODI {reg}, {reg}, {imm}
			new string[] { 
				"MODI {reg}, {reg}, {imm}",
				"LI rt, {3}", "MOD {1}, {2}, rt"
			},
			
			// MODI {reg}, {imm}, {reg}
			new string[] { 
				"MODI {reg}, {imm}, {reg}",
				"LI rt, {2}", "MOD {1}, rt, {3}"
			},
			
			// INC {reg}
			new string[] { 
				"INC {reg}",
				"ADDI {1}, {1}, 1"
			},
			
			// DEC {reg}
			new string[] { 
				"DEC {reg}",
				"SUBI {1}, {1}, 1"
			},
			
			// JMP {imm}
			new string[] { 
				"JMP {imm}",
				"LA rt, {1}", "JMP rt"
			},
			
			// JEZ {imm}, {reg}
			new string[] { 
				"JEZ {imm}, {reg}",
				"LA rt, {1}", "JEZ rt, {2}, {3}"
			},
			
			// JEZ {imm}, {reg}
			new string[] { 
				"JEZ {imm}, {reg}",
				"LA rt, {1}", "JNZ rt, {2}, {3]"
			},
			
			// JEQ {imm}, {reg}, {reg}
			new string[] { 
				"JEQ {imm}, {reg}, {reg}",
				"LA rt, {1}", "JEQ rt, {2}, {3]"
			},
			
			// JNE {imm}, {reg}, {reg}
			new string[] { 
				"JNE {imm}, {reg}, {reg}",
				"LA rt, {1}", "JNE rt, {2}, {3]"
			},
			
			// JLT {imm}, {reg}, {reg}
			new string[] { 
				"JLT {imm}, {reg}, {reg}",
				"LA rt, {1}", "JLT rt, {2}, {3]"
			},
			
			// JGT {imm}, {reg}, {reg}
			new string[] { 
				"JGT {imm}, {reg}, {reg}",
				"LA rt, {1}", "JGT rt, {2}, {3]"
			},
			
			// JLE {imm}, {reg}, {reg}
			new string[] { 
				"JLE {imm}, {reg}, {reg}",
				"LA rt, {1}", "JLE rt, {2}, {3]"
			},
			
			// JGE {imm}, {reg}, {reg}
			new string[] { 
				"JGE {imm}, {reg}, {reg}",
				"LA rt, {1}", "JGE rt, {2}, {3]"
			}
		};
    }
}