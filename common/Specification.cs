namespace FTG.Studios.BISC {

	public static class Specification {

		public static readonly string[] REGISTER_NAMES = {
			"pc", "sp", "gp", "fp", "ra", "rv", "ti", "ta",
			"r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7",
			//"f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7",
		};

		public static int NUM_REGISTERS { get { return REGISTER_NAMES.Length; } }

		public static readonly ArgumentType[][] instruction_format_definitions = {
			new ArgumentType[] { },                                                                     // N
			new ArgumentType[] { ArgumentType.Register },                                               // R
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate16 },                     // I
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // M
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register },                        // D
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // T
		};

		public static readonly InstructionFormat[] instruction_formats = {
			InstructionFormat.N, // HLT
			InstructionFormat.N, // NOP
			InstructionFormat.N, // SYS
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
			InstructionFormat.T, // MULH
			InstructionFormat.T, // MULHU
			InstructionFormat.T, // DIV
			InstructionFormat.T, // DIVU
			InstructionFormat.T, // MOD
			InstructionFormat.T, // MODU

			InstructionFormat.D, // NOT
			InstructionFormat.D, // NEG
			InstructionFormat.D, // INV

			InstructionFormat.T, // AND
			InstructionFormat.T, // OR
			InstructionFormat.T, // XOR
			InstructionFormat.T, // BSL
			InstructionFormat.T, // BSR

// TODO: maybe change jump instructions to reg reg label

			InstructionFormat.R, // JMP
			InstructionFormat.D, // JEZ
			InstructionFormat.D, // JNZ
			
			InstructionFormat.T, // JEQ
			InstructionFormat.T, // JNE
			
			InstructionFormat.T, // JGT
			InstructionFormat.T, // JLT
			InstructionFormat.T, // JGE
			InstructionFormat.T, // JLE
			
			InstructionFormat.T, // JGTU
			InstructionFormat.T, // JLTU
			InstructionFormat.T, // JGEU
			InstructionFormat.T, // JLEU
		};

		public static readonly string[] pseudo_instruction_names = new string[] {
			"LDI", "LRA",
			"CALL",
			"LDW", "LDH", "LDB", "STW", "STH", "STB",
			"PUSH", "PUSH", "PUSHW", "PUSHW", "PUSHB", "PUSHB",
			"POP", "POPW", "POPB",
			"ADDI", "ADDI", "SUBI", "SUBI", "MULI", "MULI", "DIVI", "DIVI", "MODI",
			"ANDI", "ORI", "XORI",
			"INC", "DEC",
			"JMP", "JEZ", "JNZ",
			"JEQ", "JEQ", "JEQ", "JNE", "JNE", "JNE",
			"JGT", "JGT", "JGT", "JLT", "JLT", "JLT",
			"JGE", "JGE", "JGE", "JLE", "JLE", "JLE",
			"JGTU", "JGTU", "JGTU", "JLTU", "JLTU", "JLTU",
			"JGEU", "JGEU", "JGEU", "JLEU", "JLEU", "JLEU"
		};

		public static readonly ArgumentType[][] pseudo_instruction_arguments = new ArgumentType[][] {
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LDI {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate32 },                        // LRA {imm}
			
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
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // ANDI {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // ORI  {reg}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Immediate32 }, // XORI {reg}, {reg}, {imm}
			
			new ArgumentType[] { ArgumentType.Register },                                                     // INC {reg}
			new ArgumentType[] { ArgumentType.Register },                                                     // DEC {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32 },                                                  // JMP {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register },                           // JEZ {imm}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register },                           // JNZ {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JEQ {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JEQ {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JEQ {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JNQ {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JNQ {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JNQ {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JGT {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JGT {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JGT {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JLT {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JLT {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JLT {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JGE {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JGE {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JGE {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JLE {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JLE {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JLE {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JGTU {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JGTU {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JGTU {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JLTU {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JLTU {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JLTU {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JGEU {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JGEU {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JGEU {imm}, {imm}, {reg}
			
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Register },    // JLEU {imm}, {reg}, {reg}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Register, ArgumentType.Immediate32 }, // JLEU {imm}, {reg}, {imm}
			new ArgumentType[] { ArgumentType.Immediate32, ArgumentType.Immediate32, ArgumentType.Register }, // JLEU {imm}, {imm}, {reg}
		};

		public static readonly string[][] pseudo_instruction_definitions = new string[][] {
			new string[] { "LLI {0}, %lo({1})", "LUI {0}, %hi({1})" },  // LDI {reg}, {imm}
			new string[] { "LDI {0}, {1}" },                            // LRA {reg}, {imm}
			
			new string[] { "LDI ta, {0}", "CALL ta" },                  // CALL {imm}
			
			new string[] { "LDI ti, {1}", "LDW {0}, ti[0]" },           // LDW {reg}, {imm}
			new string[] { "LDI ti, {1}", "LDH {0}, ti[0]" },           // LDH {reg}, {imm}
			new string[] { "LDI ti, {1}", "LDB {0}, ti[0]" },           // LDB {reg}, {imm}
			new string[] { "LDI ti, {1}", "STW {0}, ti[0]" },           // STW {reg}, {imm}
			new string[] { "LDI ti, {1}", "STH {0}, ti[0]" },           // STH {reg}, {imm}
			new string[] { "LDI ti, {1}", "STB {0}, ti[0]" },           // STB {reg}, {imm}

			new string[] { "STW {0}, sp[-4]", "SUBI sp, sp, 4" },       // PUSH {reg}
			new string[] { "LDI ti, {0}", "PUSH ti" },                  // PUSH {imm}
			new string[] { "STH {0}, sp[-2]", "SUBI sp, sp, 4" },       // PUSHH {reg}
			new string[] { "LDI ti, {0}", "PUSHH ti" },                 // PUSHH {imm}
			new string[] { "STB {0}, sp[-1]", "DEC sp" },               // PUSHB {reg}
			new string[] { "LDI ti, {0}", "PUSHB ti" },                 // PUSHB {imm}
			
			new string[] { "ADDI sp, sp, 4", "LDW {0}, sp[-4]" },       // POP {reg}
			new string[] { "ADDI sp, sp, 2", "LDH {0}, sp[-2]" },       // POPH {reg}
			new string[] { "INC sp", "LDB {0}, sp[-1]" },               // POPB {reg}
			
			new string[] { "LDI ti, {2}", "ADD {0}, {1}, ti" },         // ADDI {reg}, {reg}, {imm}
			new string[] { "LDI ti, {1}", "ADD {0}, ti, {2}" },         // ADDI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ti, {2}", "SUB {0}, {1}, ti" },         // SUBI {reg}, {reg}, {imm}
			new string[] { "LDI ti, {1}", "SUB {0}, ti, {2}" },         // SUBI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ti, {2}", "MUL {0}, {1}, ti" },         // MULI {reg}, {reg}, {imm}
			new string[] { "LDI ti, {1}", "MUL {0}, ti, {2}" },         // MULI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ti, {2}", "DIV {0}, {1}, ti" },         // DIVI {reg}, {reg}, {imm}
			new string[] { "LDI ti, {1}", "DIV {0}, ti, {2}" },         // DIVI {reg}, {imm}, {reg}
			
 			new string[] { "LDI ti, {2}", "MOD {0}, {1}, ti" },         // MODI {reg}, {reg}, {imm}
			
			new string[] { "LDI ti, {2}", "AND {0}, {1}, ti" },         // ANDI {reg}, {reg}, {imm}
			new string[] { "LDI ti, {2}", "OR {0}, {1}, ti" },          // ORI  {reg}, {reg}, {imm}
			new string[] { "LDI ti, {2}", "XOR {0}, {1}, ti" },         // XORI {reg}, {reg}, {imm}
			
			new string[] { "ADDI {0}, {0}, 1" },                        // INC {reg}
 			new string[] { "SUBI {0}, {0}, 1" },                        // DEC {reg}
			
			new string[] { "LDI ta, {0}", "JMP ta" },                   // JMP {imm}
			new string[] { "LDI ta, {0}", "JEZ ta, {1}" },              // JEZ {imm}, {reg}
			new string[] { "LDI ta, {0}", "JNZ ta, {1}" },              // JNZ {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JEQ ta, {1}, {2}" },               // JEQ {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JEQ ta, {1}, ti" }, // JEQ {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JEQ ta, ti, {2}" }, // JEQ {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JNE ta, {1}, {2}" },               // JNE {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JNE ta, {1}, ti" }, // JNE {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JNE ta, ti, {2}" }, // JNE {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JGT ta, {1}, {2}" },               // JGT {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JGT ta, {1}, ti" }, // JGT {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JGT ta, ti, {2}" }, // JGT {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JLT ta, {1}, {2}" },               // JLT {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JLT ta, {1}, ti" }, // JLT {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JLT ta, ti, {2}" }, // JLT {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JGE ta, {1}, {2}" },               // JGE {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JGE ta, {1}, ti" }, // JGE {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JGE ta, ti, {2}" }, // JGE {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JLE ta, {1}, {2}" },               // JLE {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JLE ta, {1}, ti" }, // JLE {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JLE ta, ti, {2}" }, // JLE {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JGTU ta, {1}, {2}" },               // JGTU {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JGTU ta, {1}, ti" }, // JGTU {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JGTU ta, ti, {2}" }, // JGTU {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JLTU ta, {1}, {2}" },               // JLTU {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JLTU ta, {1}, ti" }, // JLTU {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JLTU ta, ti, {2}" }, // JLTU {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JGEU ta, {1}, {2}" },               // JGEU {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JGEU ta, {1}, ti" }, // JGEU {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JGEU ta, ti, {2}" }, // JGEU {imm}, {imm}, {reg}
			
			new string[] { "LDI ta, {0}", "JLEU ta, {1}, {2}" },               // JLEU {imm}, {reg}, {reg}
			new string[] { "LDI ta, {0}", "LDI ti, {2}", "JLEU ta, {1}, ti" }, // JLEU {imm}, {reg}, {imm}
			new string[] { "LDI ta, {0}", "LDI ti, {1}", "JLEU ta, ti, {2}" }, // JLEU {imm}, {imm}, {reg}
		};
	}
}
