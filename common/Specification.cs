using System;

namespace FTG.Studios.BISC {

    public static class Specification {

        public const char COMMENT = ';';
		public const char LABEL_DELIMETER = ':';

        public static readonly string[] REGISTER_NAMES = {
            "pc", "sp", "fp", "ra", "rv", "ri", null, null, 
			"r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7",
			"t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
			//"f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7",
			//"ft0", "ft1", "ft2", "ft3", "ft4", "ft5", "ft6", "ft7",
		};

		public static int NUM_REGISTERS { get { return REGISTER_NAMES.Length; } }
		
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

		public static readonly ArgumentType[][] instruction_format_definitions = {
			new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // I
			new ArgumentType[] { ArgumentType.Register, ArgumentType.None, ArgumentType.None },         // R
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate16 },                     // RI
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // M
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // RD
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // RRD
		};

		public static readonly InstructionFormat[] instruction_formats = {
			InstructionFormat.I,   // NOP
			InstructionFormat.I,   // HLT
			InstructionFormat.R,   // SYS
			InstructionFormat.R,   // CALL
			InstructionFormat.I,   // RET

			InstructionFormat.RI,  // LLI
            InstructionFormat.RI,  // LUI
			InstructionFormat.RD,  // MOV

			InstructionFormat.M,  // LW
			InstructionFormat.M,  // LH
			InstructionFormat.M,  // LB
			InstructionFormat.M,  // SW
			InstructionFormat.M,  // SH
			InstructionFormat.M,  // SB

			InstructionFormat.RRD, // ADD
			InstructionFormat.RRD, // SUB
			InstructionFormat.RRD, // MUL
			InstructionFormat.RRD, // DIV
			InstructionFormat.RRD, // MOD

			InstructionFormat.RD,  // NOT
			InstructionFormat.RD,  // NEG
			InstructionFormat.RD,  // INV

			InstructionFormat.RRD, // AND
			InstructionFormat.RRD, // OR
			InstructionFormat.RRD, // XOR
			InstructionFormat.RRD, // BSL
			InstructionFormat.RRD, // BSR

			InstructionFormat.R,   // JMP
			InstructionFormat.RD,  // JEZ
			InstructionFormat.RD,  // JNZ
			InstructionFormat.RRD, // JEQ
			InstructionFormat.RRD, // JNE
			InstructionFormat.RRD, // JGT
			InstructionFormat.RRD, // JLT
			InstructionFormat.RRD, // JGE
			InstructionFormat.RRD, // JLE
		};
		
		public static readonly string[] pseudo_instruction_names = new string[] {
			"LI", "LA",
			"SYS", "CALL",
			"LW", "LH", "LB", "SW", "SH", "SB",
			"PUSH", "PUSH", "PUSHW", "PUSHW", "PUSHB", "PUSHB",
			"POP", "POPW", "POPB",
			"ADDI", "ADDI", "SUBI", "SUBI", "MULI", "MULI", "DIVI", "DIVI", "MODI",
			"INC", "DEC",
			"JMP", "JEZ", "JNZ", "JEQ", "JNE", "JGT", "JLT", "JGE", "JLE"
		};
		
		public static readonly ArgumentType[][] pseudo_instruction_arguments = new ArgumentType[][] {
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LI {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LA {imm}
			
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // SYS {imm}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // CALL {imm}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LW {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LH {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LB {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // SW {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // SH {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // SB {reg}, {imm}

			new ArgumentType[] { ArgumentType.Register },                                                  // PUSH {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSH {imm}
			new ArgumentType[] { ArgumentType.Register },                                                  // PUSHW {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSHW {imm}
			new ArgumentType[] { ArgumentType.Register },                                                  // PUSHB {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSHB {imm}
			
			new ArgumentType[] { ArgumentType.Register },                                                  // POP {reg}
			new ArgumentType[] { ArgumentType.Register },                                                  // POPW {reg}
			new ArgumentType[] { ArgumentType.Register },                                                  // POPB {reg}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // ADDI {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32, ArgumentType.Register }, // ADDI {reg}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // SUBI {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32, ArgumentType.Register }, // SUBI {reg}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // MULI {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32, ArgumentType.Register }, // MULI {reg}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // DIVI {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32, ArgumentType.Register }, // DIVI {reg}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // MODI {reg}, {reg}, {imm}
			
			new ArgumentType[] { ArgumentType.Register },                                                  // INC {reg}
			new ArgumentType[] { ArgumentType.Register },                                                  // DEC {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // JMP {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register },                        // JEZ {imm}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register },                        // JNZ {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JEQ {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JNQ {imm}, {reg}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JGT {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JLT {imm}, {reg}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JGE {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register }, // JLE {imm}, {reg}, {reg}
		};
		
		public static readonly string[][] pseudo_instruction_definitions = new string[][] {
			new string[] { "LLI {0}, {1}(0:1)", "LUI {0}, {1}(2:3)" }, // LI {imm} change to %hi(val), %lo(val) syntax
			new string[] { "LI {0}, {1}" },                            // LA {imm}
			
			new string[] { "LI ri, {0}", "SYS ri" },                   // SYS {imm}
			new string[] { "LA ri, {0}", "CALL ri" },                  // CALL {imm}
			
			new string[] { "LA ri, {1}", "LW {0}, ri[0]" },            // LW {reg}, {imm}
			new string[] { "LA ri, {1}", "LH {0}, ri[0]" },            // LH {reg}, {imm}
			new string[] { "LA ri, {1}", "LB {0}, ri[0]" },            // LB {reg}, {imm}
			new string[] { "LA ri, {1}", "SW {0}, ri[0]" },            // SW {reg}, {imm}
			new string[] { "LA ri, {1}", "SH {0}, ri[0]" },            // SH {reg}, {imm}
			new string[] { "LA ri, {1}", "SB {0}, ri[0]" },            // SB {reg}, {imm}

			new string[] { "SW {0}, sp[-4]", "SUBI sp, sp, 4" },       // PUSH {reg}
			new string[] { "LI ri, {0}", "PUSH ri" },                  // PUSH {imm}
			new string[] { "SH {0}, sp[-2]", "SUBI sp, sp, 4" },       // PUSHW {reg}
			new string[] { "LI ri, {0}", "PUSHW ri" },                 // PUSHW {imm}
			new string[] { "SB {0}, sp[-1]", "DEC sp" },               // PUSHB {reg}
			new string[] { "LI ri, {0}", "PUSHB ri" },                 // PUSHB {imm}
			
			new string[] { "ADDI sp, sp, 4", "LW {0}, sp[-4]" },       // POP {reg}
			new string[] { "ADDI sp, sp, 2", "LH {0}, sp[-2]" },       // POPW {reg}
			new string[] { "INC sp", "LB {0}, sp[-1]" },               // POPB {reg}
			
			new string[] { "LI ri, {2}", "ADD {0}, {1}, ri" },         // ADDI {reg}, {reg}, {imm}
			new string[] { "LI ri, {1}", "ADD {0}, ri, {2}" },         // ADDI {reg}, {imm}, {reg}
			
 			new string[] { "LI ri, {2}", "SUB {0}, {1}, ri" },         // SUBI {reg}, {reg}, {imm}
			new string[] { "LI ri, {1}", "SUB {0}, ri, {2}" },         // SUBI {reg}, {imm}, {reg}
			
 			new string[] { "LI ri, {2}", "MUL {0}, {1}, ri" },         // MULI {reg}, {reg}, {imm}
			new string[] { "LI ri, {1}", "MUL {0}, ri, {2}" },         // MULI {reg}, {imm}, {reg}
			
 			new string[] { "LI ri, {2}", "DIV {0}, {1}, ri" },         // DIVI {reg}, {reg}, {imm}
			new string[] { "LI ri, {1}", "DIV {0}, ri, {2}" },         // DIVI {reg}, {imm}, {reg}
			
 			new string[] { "LI ri, {2}", "MOD {0}, {1}, ri" },         // MODI {reg}, {reg}, {imm}
			
			new string[] { "ADDI {0}, {-}, 1" },                       // INC {reg}
 			new string[] { "SUBI {0}, {0}, 1" },                       // DEC {reg}
			
			new string[] { "LA ri, {0}", "JMP ri" },                   // JMP {imm}
			new string[] { "LA ri, {0}", "JEZ ri, {1}" },              // JEZ {imm}, {reg}
			new string[] { "LA ri, {0}", "JNZ ri, {1}" },              // JNZ {imm}, {reg}
			
			new string[] { "LA ri, {0}", "JEQ ri, {1}, {2}" },         // JEQ {imm}, {reg}, {reg}
			new string[] { "LA ri, {0}", "JNQ ri, {1}, {2}" },         // JNQ {imm}, {reg}, {reg}
			
			new string[] { "LA ri, {0}", "JGT ri, {1}, {2}" },         // JGT {imm}, {reg}, {reg}
			new string[] { "LA ri, {0}", "JLT ri, {1}, {2}" },         // JLT {imm}, {reg}, {reg}
			
			new string[] { "LA ri, {0}", "JGE ri, {1}, {2}" },         // JGE {imm}, {reg}, {reg}
			new string[] { "LA ri, {0}", "JLE ri, {1}, {2}" },         // JLE {imm}, {reg}, {reg}
		};
    }
}