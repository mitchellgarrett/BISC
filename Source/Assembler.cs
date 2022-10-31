using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTG.Studios.BISC {

    public static class Assembler {

        enum ArgumentType { None, Register, Address, UnsignedImmediate, SignedImmediate, DecimalImmediate };

        const char COMMENT = ';';

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

        static UInt32 ParseInstruction(string line) {
            byte[] bytes = new byte[4];

            string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            foreach (var s in parameters) {
                Console.WriteLine(s);
            }

            bytes[0] = GetOpcode(parameters[0]);
            ArgumentType[] arg_types = argument_types[bytes[0]];
            for (int i = 0; i < 3; i++) {
                switch (arg_types[i]) {
                    case ArgumentType.Register:
                        bytes[i + 1] = GetRegister(parameters[i + 1]);
                        break;
                    case ArgumentType.Address:
                        break;
                    case ArgumentType.UnsignedImmediate:
                        bytes[i + 1] = GetUnsignedImmediate(parameters[i + 1]);
                        break;
                    default:
                        break;
                }
            }

            return AssembleInstruction(bytes);
        }

        static UInt32 AssembleInstruction(byte[] bytes) {
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        static Dictionary<string, byte> opcodes = new Dictionary<string, byte>() {
            { "NOP", 0x00 },
            { "HLT", 0x01 },
            { "LLI", 0x02 },
            { "LUI", 0x03 },
            { "ADD", 0x04 },
        };

        static ArgumentType[][] argument_types = {
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },
            new ArgumentType[] {ArgumentType.None, ArgumentType.None, ArgumentType.None },
            new ArgumentType[] {ArgumentType.Register, ArgumentType.UnsignedImmediate, ArgumentType.None },
            new ArgumentType[] {ArgumentType.Register, ArgumentType.UnsignedImmediate, ArgumentType.None },
            new ArgumentType[] {ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }
        };

        static byte GetOpcode(string mneumonic) {
            if (opcodes.TryGetValue(mneumonic.ToUpper(), out byte opcode)) return opcode;
            Console.Error.WriteLine($"Invalid instruction: {mneumonic}");
            return 0xFF;
        }

        static string[] registers = {
            "pc", "sp", "ra", "rv", "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7"
        };

        static byte GetRegister(string mneumonic) {
            for (byte i = 0; i < registers.Length; i++) {
                if (mneumonic == registers[i]) return i;
            }
            return 0xFF;
        }

        static byte GetAddress(string mneumonic) {
            return 0x00;
        }

        static byte GetUnsignedImmediate(string mneumonic) {
            if (byte.TryParse(mneumonic, out byte imm)) return imm;
            return 0xFF;
        }

        static byte GetSignedImmediate(string mneumonic) {
            return 0xFF;
        }

        static byte GetDecimalImmediate(string mneumonic) {
            return 0xFF;
        }
    }
}
