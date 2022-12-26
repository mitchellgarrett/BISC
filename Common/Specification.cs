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
			
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LD
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LH
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // LB
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // ST
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // SH
			new ArgumentType[] { ArgumentType.Register, ArgumentType.Memory },                          // SB
        };
		
		public readonly static string[][] pseudo_instructions = {
			new string[] { "LLI {1}, {2}[0]", "LUI {1}, {2}[1]" },   // LI  <- get better syntax
			new string[] { "LI {1}, {2}" },                          // LA
			new string[] { "LI rt, {3}", "ADD {1}, {2}, rt" },       // ADDI
			new string[] { "LI rt, {3}", "SUB {1}, {2}, rt " },      // SUBI
			new string[] { "LI rt, {3}", "MUL {1}, {1}, rt" },       // MULI
			new string[] { "LI rt, {3}", "DIV {1}, {2}, rt" },       // DIVI
			new string[] { "LI rt, {3}", "MOD {1}, {2}, rt" },       // MODI
			new string[] { "ADDI {1}, {1}, 1" },                     // INC
			new string[] { "SUBI {1}, {1}, 1" },                     // DEC
			new string[] { "SUBI sp, sp, 4", "ST {1}, sp[0]" },      // PUSH
			new string[] { "LD {1}, sp[0]", "ADDI sp, sp, 4" },      // POP
			new string[] { "ADDI rt, pc, 4", "PUSH rt", "JMP {1}" }, // CALL
			new string[] { "POP rt", "JMP rt" }                      // RET
		};
    }
}