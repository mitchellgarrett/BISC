using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTG.Studios.BISC {

    /// <summary>
    /// BISC Assembler.
    /// </summary>
    public static class Assembler {

        static Dictionary<string, byte> opcodes;
        static Dictionary<string, UInt32> symbols;
		static Dictionary<UInt32, string> unresolved_symbols;
		
		static int lineno;
		
        /// <summary>
        /// Assembles BISC source code into binary opcodes.
        /// </summary>
        /// <param name="source">Source code.</param>
        /// <returns>An executable program.</returns>
        public static Program Assemble(string source) {
            if (opcodes == null) {
                opcodes = new Dictionary<string, byte>();
                foreach (Opcode opcode in Enum.GetValues(typeof(Opcode))) {
                    opcodes[opcode.ToString()] = (byte)opcode;
                }
            }
			
			if (symbols == null) {
				symbols = new Dictionary<string, UInt32>();
			}
			
            List<UInt32> instructions = new List<UInt32>();
			List<string> lines = source.Split('\n').ToList<string>();
			Preprocessor.ResolvePseudoInstructions(lines);
			Preprocessor.Preprocess(lines);
			
			for (lineno = 0; lineno < lines.Count; lineno++) {
				string line = lines[lineno];
				int comment_index = line.IndexOf(Specification.COMMENT);
				if (comment_index >= 0) line = line.Substring(0, comment_index);
                if (string.IsNullOrEmpty(line)) continue;
				
				if (line.IndexOf(Specification.LABEL_DELIMETER) >= 0) {
					string label = line.Substring(0, line.IndexOf(Specification.LABEL_DELIMETER));
					symbols[label] = (UInt32) (instructions.Count * 4 + 4);
				}
				
				UInt32? instruction = ParseInstruction(line);
				if (instruction.HasValue) instructions.Add(instruction.Value);
			}

            Program program = new Program(instructions.ToArray());
            return program;
        }

        /// <summary>
        /// Parses a line of BISC code into a single binary instruction.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>A single BISC instruction in binary form.</returns>
        static UInt32? ParseInstruction(string line) {
            Console.WriteLine(line);
            string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            UInt32 instruction = 0x00000000;

            byte? opcode = ParseOpcode(parameters[0]);
			if (!opcode.HasValue) return null;
            instruction |= (UInt32) opcode.Value << 24;
			
            ArgumentType[] arg_types = Specification.argument_types[opcode.Value];
            for (int i = 0; i < arg_types.Length; i++) {
                switch (arg_types[i]) {
                    case ArgumentType.Register:
						byte? reg = ParseRegister(parameters[i + 1]);
						if (!reg.HasValue) {
							InvalidValue(parameters[i + 1]);
							return null;
						}
                        instruction |= (UInt32) reg.Value << (2 - i) * 8;
                        break;
                    case ArgumentType.Memory:
						UInt16? mem = ParseMemory(parameters[i + 1]);
						if (!mem.HasValue) {
							InvalidValue(parameters[i + 1]);
							return null;
						}
                        instruction |= (UInt32) mem.Value << (1 - i) * 8;
                        break;
                    case ArgumentType.IntegerImmediate:
						UInt16? imm = ParseInteger16(parameters[i + 1]);
						if (!imm.HasValue) {
							InvalidValue(parameters[i + 1]);
							return null;
						}
                        instruction |= (UInt32) imm.Value << (1 - i) * 8;
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

        /// <summary>
        /// Parses a string instruction into a binary opcode.
        /// </summary>
        /// <param name="mneumonic">String to parse.</param>
        /// <returns>Opcode in binary form.</returns>
        static byte? ParseOpcode(string mneumonic) {
            if (opcodes.TryGetValue(mneumonic.ToUpper(), out byte opcode)) return opcode;
            InvalidInstruction(mneumonic);
            return null;
        }

        /// <summary>
        /// Parses a register name into its index into the register file.
        /// </summary>
        /// <param name="mneumonic">Register name.</param>
        /// <returns>Register index.</returns>
        static byte? ParseRegister(string mneumonic) {
            for (byte i = 0; i < Specification.NUM_REGISTERS; i++) {
                if (mneumonic == Specification.REGISTER_NAMES[i]) return i;
            }
            return null;
        }
		
		static UInt32? ParseLabel(string mneumonic) {
			if (symbols.TryGetValue(mneumonic, out UInt32 value)) return value;
			//unresolved_symbols[address] = mneumonic;
			return null;
		}
		
        static UInt16? ParseMemory(string mneumonic) {
			byte? reg = ParseRegister(mneumonic.Substring(0, 2));
			if (!reg.HasValue) return null;
			
			string val = mneumonic.Substring(mneumonic.IndexOf('[') + 1, mneumonic.IndexOf(']') - mneumonic.IndexOf('[') - 1);
			
			// Check for negative value
            bool isNegative = false;
            if (val[0] == '-') {
                isNegative = true;
                val = val.Substring(1);
            }
            if (byte.TryParse(val, out byte imm)) {
                if (isNegative) {
                    imm--;
                    imm |= 0xFF;
                }
            }
			
            return Specification.AssembleInteger16(reg.Value, imm);
        }

        /// <summary>
        /// Parses a string into an unsigned 16-bit integer.
        /// </summary>
        /// <param name="mneumonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
        /// <returns>Unsigned 16-bit integer.</returns>
        static UInt16? ParseInteger16(string mneumonic) {
			
			// Check for label or symbol
			if (symbols.ContainsKey(mneumonic)) {
				return (UInt16) symbols[mneumonic];
			}
			
            // Check for ASCII character
            if (mneumonic[0] == '\'') {
                if (mneumonic.Length != 3 || mneumonic[2] != '\'') {
                    //InvalidValue(mneumonic);
                    return null;
                }
                return mneumonic[1];
            }

            // Check if value has prefix
            if (mneumonic.Length >= 3 && mneumonic[0] == '0') {
                // Check for hexadecimal value
                if (mneumonic[1] == 'x' || mneumonic[1] == 'X') return Convert.ToUInt16(mneumonic.Substring(2), 16);
                // Check for binary value
                if (mneumonic[1] == 'b' || mneumonic[1] == 'B') return Convert.ToUInt16(mneumonic.Substring(2), 2);
            }

            // Check for negative value
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

            //InvalidValue(mneumonic);
            return null;
        }

        static byte? GetSignedImmediate(string mneumonic) {
            return null;
        }

        static byte? GetDecimalImmediate(string mneumonic) {
            return null;
        }

        static void InvalidValue(string mneumonic) {
            Console.Error.WriteLine($"Invalid value at line {lineno}: {mneumonic}");
        }

        static void InvalidInstruction(string mneumonic) {
            Console.Error.WriteLine($"Invalid instruction at line {lineno}: {mneumonic}");
        }
    }
}
