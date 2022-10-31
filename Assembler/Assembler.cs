using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTG.Studios.BISC {

    /// <summary>
    /// BISC Assembler.
    /// </summary>
    public static class Assembler {

        enum ArgumentType { None, Register, Address, IntegerImmediate, DecimalImmediate };

        const char COMMENT = ';';

        /// <summary>
        /// Assembles BISC source code into binary opcodes.
        /// </summary>
        /// <param name="source">Source code.</param>
        /// <returns>Instructions in binary form.</returns>
        public static UInt32[] Assemble(string source) {
            List<UInt32> instructions = new List<UInt32>();
            using (StringReader reader = new StringReader(source)) {
                string line;
                while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
                    instructions.Add(ParseInstruction(line));
                }
            }
            return instructions.ToArray();
        }

        /// <summary>
        /// Writes binary instructions to specified file.
        /// </summary>
        /// <param name="path">File to write to.</param>
        /// <param name="instructions">Instructions to write.</param>
        public static void WriteInstructions(string path, UInt32[] instructions) {
            byte[] bytes = new byte[instructions.Length * 4];
            Buffer.BlockCopy(instructions, 0, bytes, 0, bytes.Length);
            if (BitConverter.IsLittleEndian) {
                byte b0, b1, b2, b3;
                for (int i = 0; i < bytes.Length; i += 4) {
                    b0 = bytes[i + 0];
                    b1 = bytes[i + 1];
                    b2 = bytes[i + 2];
                    b3 = bytes[i + 3];
                    bytes[i + 0] = b3;
                    bytes[i + 1] = b2;
                    bytes[i + 2] = b1;
                    bytes[i + 3] = b0;
                }
            }
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Reads instructions from a binary file.
        /// </summary>
        /// <param name="path">File to read from.</param>
        /// <returns>Instructions in binary form.</returns>
        public static UInt32[] ReadInstructions(string path) {
            byte[] bytes = File.ReadAllBytes(path);
            if (BitConverter.IsLittleEndian) {
                byte b0, b1, b2, b3;
                for (int i = 0; i < bytes.Length; i += 4) {
                    b0 = bytes[i + 0];
                    b1 = bytes[i + 1];
                    b2 = bytes[i + 2];
                    b3 = bytes[i + 3];
                    bytes[i + 0] = b3;
                    bytes[i + 1] = b2;
                    bytes[i + 2] = b1;
                    bytes[i + 3] = b0;
                }
            }

            UInt32[] instructions = new UInt32[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, instructions, 0, bytes.Length);
            return instructions;
        }

        /// <summary>
        /// Parses a line of BISC code into a single binary instruction.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>A single BISC instruction in binary form.</returns>
        static UInt32 ParseInstruction(string line) {

            string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            foreach (var s in parameters) {
                Console.WriteLine(s);
            }

            UInt32 instruction = 0x00000000;

            byte opcode = GetOpcode(parameters[0]);
            instruction |= (UInt32) opcode << 24;

            ArgumentType[] arg_types = argument_types[opcode];
            for (int i = 0; i < arg_types.Length; i++) {
                switch (arg_types[i]) {
                    case ArgumentType.Register:
                        instruction |= (UInt32) GetRegister(parameters[i + 1]) << (2 - i) * 8;
                        break;
                    case ArgumentType.Address:
                        break;
                    case ArgumentType.IntegerImmediate:
                        instruction |= (UInt32) GetIntegerImmediate(parameters[i + 1]) << (1 - i) * 8;
                        break;
                    default:
                        break;
                }
            }

            return instruction;
        }

        /// <summary>
        /// Assembles a 4-byte array into a 32-bit unsigned integer.
        /// </summary>
        /// <param name="bytes">Byte array to convert into intger. Must be 4 bytes.</param>
        /// <returns>A single 32-bit unsigned integer.</returns>
        static UInt32 AssembleInstruction(byte[] bytes) {
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        static Dictionary<string, byte> opcodes = new Dictionary<string, byte>() {
            { "NOP", 0x00 },
            { "HLT", 0x01 },
            { "LLI", 0x05 },
            { "LUI", 0x06 },
            { "ADD", 0x07 },
        };

        static ArgumentType[][] argument_types = {
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },
            new ArgumentType[] {ArgumentType.None, ArgumentType.None, ArgumentType.None },
            null, null, null,
            new ArgumentType[] {ArgumentType.Register, ArgumentType.IntegerImmediate },
            new ArgumentType[] {ArgumentType.Register, ArgumentType.IntegerImmediate },
            new ArgumentType[] {ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }
        };

        /// <summary>
        /// Parses a string instruction into a binary opcode.
        /// </summary>
        /// <param name="mneumonic">String to parse.</param>
        /// <returns>Opcode in binary form.</returns>
        static byte GetOpcode(string mneumonic) {
            if (opcodes.TryGetValue(mneumonic.ToUpper(), out byte opcode)) return opcode;
            InvalidInstruction(mneumonic);
            return 0xFF;
        }

        static string[] registers = {
            "pc", "sp", "ra", "rv", "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7"
        };

        /// <summary>
        /// Parses a register name into its index into the register file.
        /// </summary>
        /// <param name="mneumonic">Register name.</param>
        /// <returns>Register index.</returns>
        static byte GetRegister(string mneumonic) {
            for (byte i = 0; i < registers.Length; i++) {
                if (mneumonic == registers[i]) return i;
            }
            return 0xFF;
        }

        static byte GetAddress(string mneumonic) {
            return 0x00;
        }

        static UInt16 GetIntegerImmediate(string mneumonic) {
            if (mneumonic[0] == '\'') {
                if (mneumonic.Length != 3 || mneumonic[2] != '\'') {
                    InvalidValue(mneumonic);
                    return 0xFFF;
                }
                return mneumonic[1];
            }

            bool isNegative = false;
            if (mneumonic[0] == '-') {
                isNegative = true;
                mneumonic = mneumonic.Substring(1);
            }
            if (UInt16.TryParse(mneumonic, out UInt16 imm)) {
                if (isNegative) {
                    imm--;
                    imm |= 0xFFFF;
                }
                return imm;
            }

            InvalidValue(mneumonic);
            return 0xFFFF;
        }

        static byte GetSignedImmediate(string mneumonic) {
            return 0xFF;
        }

        static byte GetDecimalImmediate(string mneumonic) {
            return 0xFF;
        }

        static void InvalidValue(string mneumonic) {
            Console.Error.WriteLine($"Invalid value: {mneumonic}");
        }

        static void InvalidInstruction(string mneumonic) {
            Console.Error.WriteLine($"Invalid instruction: {mneumonic}");
        }
    }
}
