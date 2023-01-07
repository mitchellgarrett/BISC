using System;
using System.Collections.Generic;

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
		
        public static readonly ArgumentType[][] argument_types = {
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // NOP
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // HLT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.None, ArgumentType.None },         // SYS
            new ArgumentType[] { ArgumentType.Register, ArgumentType.None, ArgumentType.None },         // CALL
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // RET

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate16 },                     // LLI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Immediate16 },                     // LUI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // MOV
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LW
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LH
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LB
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // SW
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
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JEZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JNZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JNE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLE
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
			
			new string[] { "LI rt, {0}", "SYS rt" },                   // SYS {imm}
			new string[] { "LA rt, {0}", "CALL rt" },                  // CALL {imm}
			
			new string[] { "LA rt, {1}", "LW {0}, rt[0]" },            // LW {reg}, {imm}
			new string[] { "LA rt, {1}", "LH {0}, rt[0]" },            // LH {reg}, {imm}
			new string[] { "LA rt, {1}", "LB {0}, rt[0]" },            // LB {reg}, {imm}
			new string[] { "LA rt, {1}", "SW {0}, rt[0]" },            // SW {reg}, {imm}
			new string[] { "LA rt, {1}", "SH {0}, rt[0]" },            // SH {reg}, {imm}
			new string[] { "LA rt, {1}", "SB {0}, rt[0]" },            // SB {reg}, {imm}

			new string[] { "SW {0}, sp[-4]", "SUBI sp, sp, 4" },       // PUSH {reg}
			new string[] { "LI rt, {0}", "PUSH rt" },                  // PUSH {imm}
			new string[] { "SH {0}, sp[-2]", "SUBI sp, sp, 4" },       // PUSHW {reg}
			new string[] { "LI rt, {0}", "PUSHW rt" },                 // PUSHW {imm}
			new string[] { "SB {0}, sp[-1]", "DEC sp" },               // PUSHB {reg}
			new string[] { "LI rt, {0}", "PUSHB rt" },                 // PUSHB {imm}
			
			new string[] { "ADDI sp, sp, 4", "LW {0}, sp[-4]" },       // POP {reg}
			new string[] { "ADDI sp, sp, 2", "LH {0}, sp[-2]" },       // POPW {reg}
			new string[] { "INC sp", "LB {0}, sp[-1]" },               // POPB {reg}
			
			new string[] { "LI rt, {2}", "ADD {0}, {1}, rt" },         // ADDI {reg}, {reg}, {imm}
			new string[] { "LI rt, {1}", "ADD {0}, rt, {2}" },         // ADDI {reg}, {imm}, {reg}
			
 			new string[] { "LI rt, {2}", "SUB {0}, {1}, rt" },         // SUBI {reg}, {reg}, {imm}
			new string[] { "LI rt, {1}", "SUB {0}, rt, {2}" },         // SUBI {reg}, {imm}, {reg}
			
 			new string[] { "LI rt, {2}", "MUL {0}, {1}, rt" },         // MULI {reg}, {reg}, {imm}
			new string[] { "LI rt, {1}", "MUL {0}, rt, {2}" },         // MULI {reg}, {imm}, {reg}
			
 			new string[] { "LI rt, {2}", "DIV {0}, {1}, rt" },         // DIVI {reg}, {reg}, {imm}
			new string[] { "LI rt, {1}", "DIV {0}, rt, {2}" },         // DIVI {reg}, {imm}, {reg}
			
 			new string[] { "LI rt, {2}", "MOD {0}, {1}, rt" },         // MODI {reg}, {reg}, {imm}
			
			new string[] { "ADDI {0}, {-}, 1" },                       // INC {reg}
 			new string[] { "SUBI {0}, {0}, 1" },                       // DEC {reg}
			
			new string[] { "LA rt, {0}", "JMP rt" },                   // JMP {imm}
			new string[] { "LA rt, {0}", "JEZ rt, {1}" },              // JEZ {imm}, {reg}
			new string[] { "LA rt, {0}", "JNZ rt, {1}" },              // JNZ {imm}, {reg}
			
			new string[] { "LA rt, {0}", "JEQ rt, {1}, {2}" },         // JEQ {imm}, {reg}, {reg}
			new string[] { "LA rt, {0}", "JNQ rt, {1}, {2}" },         // JNQ {imm}, {reg}, {reg}
			
			new string[] { "LA rt, {0}", "JGT rt, {1}, {2}" },         // JGT {imm}, {reg}, {reg}
			new string[] { "LA rt, {0}", "JLT rt, {1}, {2}" },         // JLT {imm}, {reg}, {reg}
			
			new string[] { "LA rt, {0}", "JGE rt, {1}, {2}" },         // JGE {imm}, {reg}, {reg}
			new string[] { "LA rt, {0}", "JLE rt, {1}, {2}" },         // JLE {imm}, {reg}, {reg}
		};
    }
}