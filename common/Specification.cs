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
		
		/// <summary>
		/// Assembles a 16-bit integer from two bytes supplied in little-endian order.
		/// </summary>
		/// <param name="a">Least significant byte.</param>
		/// <param name="b">Most significant byte.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt16 AssembleInteger16(byte a, byte b) {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt16(new byte[] { b, a }, 0);
            return BitConverter.ToUInt16(new byte[] { a, b }, 0);
		}

		/// <summary>
		/// Assembles a 32-bit integer from four bytes supplied in little-endian order.
		/// </summary>
		/// <param name="a">Least significant byte.</param>
		/// <param name="b">Second byte.</param>
		/// <param name="c">Third byte.</param>
		/// <param name="d">Most significant byte.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt32 AssembleInteger32(byte a, byte b, byte c, byte d) {
            if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt32(new byte[] { d, c, b, a }, 0);
            return BitConverter.ToUInt32(new byte[] { a, b, c, d }, 0);
		}
		
		/// <summary>
		/// Disassembles a 16-bit integer into two bytes in little-endian order.
		/// </summary>
		/// <param name="value">16-bit integer.</param>
		/// <returns>A byte array of two bytes in little-endian order.</returns>
		public static byte[] DisassembleInteger16(UInt16 value) {
			byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
		}

		/// <summary>
		/// Disassembles a 32-bit integer into four bytes in little-endian order.
		/// </summary>
		/// <param name="value">16-bit integer.</param>
		/// <returns>A byte array of four bytes in little-endian order.</returns>
		public static byte[] DisassembleInteger32(UInt32 value) {
			byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
		}

		public static readonly ArgumentType[][] instruction_format_definitions = {
			new ArgumentType[] { },                                                                     // I
			new ArgumentType[] { ArgumentType.Register },                                               // R
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate16 },                     // RI
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // M
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register },                        // RD
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // RRD
		};

		public static readonly InstructionFormat[] instruction_formats = {
			InstructionFormat.N, // NOP
			InstructionFormat.N, // HLT
			InstructionFormat.R, // SYS
			InstructionFormat.R, // CALL
			InstructionFormat.N, // RET

			InstructionFormat.I, // LLI
            InstructionFormat.I, // LUI
			InstructionFormat.D, // MOV

			InstructionFormat.M, // LDW
			InstructionFormat.M, // LDH
			InstructionFormat.M, // LDB
			InstructionFormat.M, // STW
			InstructionFormat.M, // STH
			InstructionFormat.M, // STB

			InstructionFormat.T, // ADD
			InstructionFormat.T, // SUB
			InstructionFormat.T, // MUL
			InstructionFormat.T, // DIV
			InstructionFormat.T, // MOD

			InstructionFormat.D, // NOT
			InstructionFormat.D, // NEG
			InstructionFormat.D, // INV

			InstructionFormat.T, // AND
			InstructionFormat.T, // OR
			InstructionFormat.T, // XOR
			InstructionFormat.T, // BSL
			InstructionFormat.T, // BSR

			InstructionFormat.R, // JMP
			InstructionFormat.D, // JEZ
			InstructionFormat.D, // JNZ
			InstructionFormat.T, // JEQ
			InstructionFormat.T, // JNE
			InstructionFormat.T, // JGT
			InstructionFormat.T, // JLT
			InstructionFormat.T, // JGE
			InstructionFormat.T, // JLE
		};
		
		public static readonly string[] pseudo_instruction_names = new string[] {
			"LDI", "LRA",
			"SYS", "CALL",
			"LW", "LH", "LB", "SW", "SH", "SB",
			"PUSH", "PUSH", "PUSHW", "PUSHW", "PUSHB", "PUSHB",
			"POP", "POPW", "POPB",
			"ADDI", "ADDI", "SUBI", "SUBI", "MULI", "MULI", "DIVI", "DIVI", "MODI",
			"INC", "DEC",
			"JMP", "JEZ", "JNZ", "JEQ", "JNE", "JGT", "JLT", "JGE", "JLE"
		};
		
		public static readonly ArgumentType[][] pseudo_instruction_arguments = new ArgumentType[][] {
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LDI {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LRA {imm}
			
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // SYS {imm}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // CALL {imm}
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LDW {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LDH {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LDB {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // STW {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // STH {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // STB {reg}, {imm}

			new ArgumentType[] { ArgumentType.Register },                                                  // PUSH {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSH {imm}
			new ArgumentType[] { ArgumentType.Register },                                                  // PUSHH {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSHH {imm}
			new ArgumentType[] { ArgumentType.Register },                                                  // PUSHB {reg}
			new ArgumentType[] { ArgumentType.Immediate32 },                                               // PUSHB {imm}
			
			new ArgumentType[] { ArgumentType.Register },                                                  // POP {reg}
			new ArgumentType[] { ArgumentType.Register },                                                  // POPH {reg}
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
			//new string[] { "LLI {0}, %lo({1})", "LUI {0}, %hi({1})" }, // LDI {imm}
			new string[] { "LLI {0}, {1}", "LUI {0}, {1}" },
			new string[] { "LDI {0}, {1}" },                            // LRA {imm}
			
			new string[] { "LDI ri, {0}", "SYS ri" },                   // SYS {imm}
			new string[] { "LDI ri, {0}", "CALL ri" },                  // CALL {imm}
			
			new string[] { "LDI ri, {1}", "LDW {0}, ri[0]" },           // LDW {reg}, {imm}
			new string[] { "LDI ri, {1}", "LDH {0}, ri[0]" },           // LDH {reg}, {imm}
			new string[] { "LDI ri, {1}", "LDB {0}, ri[0]" },           // LDB {reg}, {imm}
			new string[] { "LDI ri, {1}", "STW {0}, ri[0]" },           // STW {reg}, {imm}
			new string[] { "LDI ri, {1}", "STH {0}, ri[0]" },           // STH {reg}, {imm}
			new string[] { "LDI ri, {1}", "STB {0}, ri[0]" },           // STB {reg}, {imm}

			new string[] { "STW {0}, sp[-4]", "SUBI sp, sp, 4" },       // PUSH {reg}
			new string[] { "LDI ri, {0}", "PUSH ri" },                  // PUSH {imm}
			new string[] { "STH {0}, sp[-2]", "SUBI sp, sp, 4" },       // PUSHH {reg}
			new string[] { "LDI ri, {0}", "PUSHH ri" },                 // PUSHH {imm}
			new string[] { "STB {0}, sp[-1]", "DEC sp" },               // PUSHB {reg}
			new string[] { "LDI ri, {0}", "PUSHB ri" },                 // PUSHB {imm}
			
			new string[] { "ADDI sp, sp, 4", "LDW {0}, sp[-4]" },       // POP {reg}
			new string[] { "ADDI sp, sp, 2", "LDH {0}, sp[-2]" },       // POPH {reg}
			new string[] { "INC sp", "LDB {0}, sp[-1]" },               // POPB {reg}
			
			new string[] { "LDI ri, {2}", "ADD {0}, {1}, ri" },         // ADDI {reg}, {reg}, {imm}
			new string[] { "LDI ri, {1}", "ADD {0}, ri, {2}" },         // ADDI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ri, {2}", "SUB {0}, {1}, ri" },         // SUBI {reg}, {reg}, {imm}
			new string[] { "LDI ri, {1}", "SUB {0}, ri, {2}" },         // SUBI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ri, {2}", "MUL {0}, {1}, ri" },         // MULI {reg}, {reg}, {imm}
			new string[] { "LDI ri, {1}", "MUL {0}, ri, {2}" },         // MULI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ri, {2}", "DIV {0}, {1}, ri" },         // DIVI {reg}, {reg}, {imm}
			new string[] { "LDI ri, {1}", "DIV {0}, ri, {2}" },         // DIVI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ri, {2}", "MOD {0}, {1}, ri" },         // MODI {reg}, {reg}, {imm}
			
			new string[] { "ADDI {0}, {0}, 1" },                        // INC {reg}
 			new string[] { "SUBI {0}, {0}, 1" },                        // DEC {reg}
			
			new string[] { "LDI ri, {0}", "JMP ri" },                   // JMP {imm}
			new string[] { "LDI ri, {0}", "JEZ ri, {1}" },              // JEZ {imm}, {reg}
			new string[] { "LDI ri, {0}", "JNZ ri, {1}" },              // JNZ {imm}, {reg}
			
			new string[] { "LDI ri, {0}", "JEQ ri, {1}, {2}" },         // JEQ {imm}, {reg}, {reg}
			new string[] { "LDI ri, {0}", "JNQ ri, {1}, {2}" },         // JNQ {imm}, {reg}, {reg}
			
			new string[] { "LDI ri, {0}", "JGT ri, {1}, {2}" },         // JGT {imm}, {reg}, {reg}
			new string[] { "LDI ri, {0}", "JLT ri, {1}, {2}" },         // JLT {imm}, {reg}, {reg}
			
			new string[] { "LDI ri, {0}", "JGE ri, {1}, {2}" },         // JGE {imm}, {reg}, {reg}
			new string[] { "LDI ri, {0}", "JLE ri, {1}, {2}" },         // JLE {imm}, {reg}, {reg}
		};
    }
}