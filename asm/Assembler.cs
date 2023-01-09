﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FTG.Studios.BISC {

    /// <summary>
    /// BISC Assembler.
    /// </summary>
    public static class Assembler {

        static Dictionary<string, byte> opcodes;
        static Dictionary<string, UInt32> symbols;
		
		static int lineno;
		
        /// <summary>
        /// Assembles BISC source code into binary opcodes.
        /// </summary>
        /// <param name="source">Source code.</param>
        /// <returns>An executable program.</returns>
        public static Program Assemble(string source) {
			
			// Initialize opcodes dictionary to convert string mneumonic to byte value
            if (opcodes == null) {
                opcodes = new Dictionary<string, byte>();
                foreach (Opcode opcode in Enum.GetValues(typeof(Opcode))) {
                    opcodes[opcode.ToString()] = (byte)opcode;
                }
            }

			// Initialize dictionaries for known symbol values and instructions with unresolved symbols
			symbols = new Dictionary<string, UInt32>();
			List<Instruction> unresolved_symbols = new List<Instruction>();
			
			// Split source on all possible newlines
			List<Instruction> instructions = new List<Instruction>();
			List<string> lines = source.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).ToList<string>();

			// Loop over all lines
			for (lineno = 0; lineno < lines.Count; lineno++) {
				// Create an Instruction object from line which the string into its instruction and arguments
				Instruction inst = new Instruction(lines[lineno]);
				
				// Only add the instruction if it has a valid instruction mneumonic
				if (string.IsNullOrEmpty(inst.Mneumonic)) continue;

				inst.Line = lineno;
				instructions.Add(inst);
			}
			
			// Loop over all instructions and resolve pseudo-instructions if possible
			for (int i = 0; i < instructions.Count; i++) {
				Instruction[] pseudo = ResolvePseudoInstruction(instructions[i]);
				// If the instruction was a pseudo, remove it from the list and add its replacements
				if (pseudo != null) {
					instructions.RemoveAt(i);
					instructions.InsertRange(i--, pseudo);
				}
			}
			
			// First-pass optimizations
			Optimizer.Optimize(instructions);
			
			// Loop over instructions to assign addresses to instructions/labels
			UInt32 address = 0;
			for (int i = 0; i < instructions.Count; i++) {
				Instruction inst = instructions[i];
				inst.Address = address;

				// Check if instruction is actuall a symbol, if so remove it since it won't be compiled into machine code
				// and add it to the symbols dictionary instead
				if (inst.Mneumonic == "SYMBOL" && inst.Parameters[0].Type == ArgumentType.Symbol) {
					symbols[inst.Parameters[0].Mneumonic] = inst.Address;
					instructions.RemoveAt(i--);
				} else {
					// Check if instruction references a symbol and add to the list to be resolved later
					for (int p = 0; p < inst.Parameters.Length; p++) {
						if (inst.Parameters[p].Type == ArgumentType.Symbol) {
							unresolved_symbols.Add(inst);
						}
					}
					address += 4;
				}
			}
			
			// Loop over unresolved symbols and get their values
			foreach (Instruction inst in unresolved_symbols) {
				for (int p = 0; p < inst.Parameters.Length; p++) {
						if (inst.Parameters[p].Type == ArgumentType.Symbol) {
							inst.Parameters[p].Value = ResolveSymbol(inst.Parameters[p].Mneumonic);
							inst.Parameters[p].Type = ArgumentType.Immediate32;
						}
					}
			}
			
			// Convert instructions into binary machine code
			List<UInt32> machine_code = new List<UInt32>();
			foreach (Instruction inst in instructions) {
				Console.WriteLine(inst);
				machine_code.Add(inst.Assemble());
			}

			// Create program object from machine code
            Program program = new Program(machine_code.ToArray());
            return program;
        }
		
		/// <summary>
		/// Parses a BISC pseudo-instruction into its opcode replacements.
		/// </summary>
		/// <param name="inst">Pseudo-instruction to resolve.</param>
		/// <returns>Array of valid replacement instructions. Returns null if inst is not a pseudo-instruction.</returns>
		static Instruction[] ResolvePseudoInstruction(Instruction inst) {
			// Loop over all pseudo-instruction names to find match
			for (int i = 0; i < Specification.pseudo_instruction_names.Length; i++) {
				if (inst.Mneumonic == Specification.pseudo_instruction_names[i]) {
					
					// Once match is found, confirm that the parameter types match the expected values
					bool valid = true;
					for (int p = 0; p < inst.Parameters.Length; p++) {
						// If parameter types are not equivalent, this instruction doesn't match the pseudo-instruction prototype
						if (!inst.Parameters[p].Type.IsEquivalent(Specification.pseudo_instruction_arguments[i][p])) {
							valid = false;
							break;
						}
					}
					// Confirm that number of parameters match too
					if (inst.Parameters.Length != Specification.pseudo_instruction_arguments[i].Length) valid = false;
					
					// If inst does match this pseudo-instruction, then produce the replacement instructions
					if (valid) {
						Instruction[] replacements = new Instruction[Specification.pseudo_instruction_definitions[i].Length];
						
						// Loop over replacements and get their parameters
						for (int s = 0; s < replacements.Length; s++) {
							string pseudo = Specification.pseudo_instruction_definitions[i][s];
							string[] vals = new string[inst.Parameters.Length];

							// Loop over parameters to get their values from inst
							for (int v = 0; v < vals.Length; v++) {
								vals[v] = inst.Parameters[v].Mneumonic;
							}
							replacements[s] = new Instruction(string.Format(pseudo, vals));
						}
						return replacements;
					}
				}
			}

			// If inst is not a pseudo-instruction, return null
			return null;
		}
		
        /// <summary>
        /// Parses a line of BISC code into a single binary instruction.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>A single BISC instruction in binary form.</returns>
        static UInt32? ParseInstruction(string line) {
            string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            UInt32 instruction = 0x00000000;

            byte? opcode = ParseOpcode(parameters[0]);
			if (!opcode.HasValue) return null;
            instruction |= (UInt32) opcode.Value << 24;

			ArgumentType[] arg_types = Specification.instruction_format_definitions[(int) Specification.instruction_formats[opcode.Value]];
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
                    case ArgumentType.Immediate16:
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
        /// Parses a string instruction into a binary opcode.
        /// </summary>
        /// <param name="mneumonic">String to parse.</param>
        /// <returns>Opcode in binary form.</returns>
        public static byte? ParseOpcode(string mneumonic) {
            if (opcodes.TryGetValue(mneumonic.ToUpper(), out byte opcode)) return opcode;
            //InvalidInstruction(mneumonic);
            return null;
        }

        /// <summary>
        /// Parses a register name into its index into the register file.
        /// </summary>
        /// <param name="mneumonic">Register name.</param>
        /// <returns>Register index.</returns>
        public static byte? ParseRegister(string mneumonic) {
            for (byte i = 0; i < Specification.NUM_REGISTERS; i++) {
                if (((Register)i).IsValid() && mneumonic == Specification.REGISTER_NAMES[i]) return i;
            }
            return null;
        }
		
		static UInt32? ParseLabel(string mneumonic) {
			if (symbols.TryGetValue(mneumonic, out UInt32 value)) return value;
			return null;
		}
		
		static UInt32 ResolveSymbol(string symbol) {
			// Check for byte indices
			int byte_start = 0;
			int byte_end = 3;
			bool hasByteRange = false;
			if (symbol.IndexOf('(') >= 0) {
				string[] indices = symbol.Substring(symbol.IndexOf('(') + 1, symbol.IndexOf(')') - symbol.IndexOf('(') - 1).Split(':');
				byte_start = int.Parse(indices[0]);
				byte_end = int.Parse(indices[1]);
				
				symbol = symbol.Substring(0, symbol.IndexOf('('));
				hasByteRange = true;
			}
			
			if (!symbols.TryGetValue(symbol, out UInt32 value)) return 0xFFFFFFFF;
			if (hasByteRange) {
					value = GetByteRange(value, byte_start, byte_end);
			}
			return value;
		}
		
        public static UInt16? ParseMemory(string mneumonic) {
			byte? reg = ParseRegister(mneumonic.Substring(0, 2));
			if (!reg.HasValue || mneumonic.IndexOf('[') <= 0) return null;
			
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
                    imm ^= 0xFF;
                }
            }
			
            return Specification.AssembleInteger16(reg.Value, imm);
        }

        /// <summary>
        /// Parses a string into an unsigned 16-bit integer.
        /// </summary>
        /// <param name="mneumonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
        /// <returns>Unsigned 16-bit integer.</returns>
        public static UInt16? ParseInteger16(string mneumonic) {
			
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
                    imm ^= 0xFFFF;
                }
                return imm;
            }

            //InvalidValue(mneumonic);
            return null;
        }
		
		/// <summary>
        /// Parses a string into an unsigned 32-bit integer.
        /// </summary>
        /// <param name="mneumonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
        /// <returns>Unsigned 32-bit integer.</returns>
        public static UInt32? ParseInteger32(string mneumonic) {
			
			// Check for label or symbol
			if (symbols.ContainsKey(mneumonic)) {
				return symbols[mneumonic];
			}
			
            // Check for ASCII character
            if (mneumonic[0] == '\'') {
                if (mneumonic.Length != 3 || mneumonic[2] != '\'') {
                    //InvalidValue(mneumonic);
                    return null;
                }
                return mneumonic[1];
            }
			
			// Check for byte indices
			int byte_start = 0;
			int byte_end = 3;
			bool hasByteRange = false;
			if (mneumonic.IndexOf('(') >= 0) {
				string[] indices = mneumonic.Substring(mneumonic.IndexOf('(') + 1, mneumonic.IndexOf(')') - mneumonic.IndexOf('(') - 1).Split(':');
				byte_start = int.Parse(indices[0]);
				byte_end = int.Parse(indices[1]);
				
				mneumonic = mneumonic.Substring(0, mneumonic.IndexOf('('));
				hasByteRange = true;
			}
			
            // Check if value has prefix
			UInt32 value = 0;
            if (mneumonic.Length >= 3 && mneumonic[0] == '0') {
                // Check for hexadecimal value
                if (mneumonic[1] == 'x' || mneumonic[1] == 'X') value = Convert.ToUInt32(mneumonic.Substring(2), 16);
                // Check for binary value
                if (mneumonic[1] == 'b' || mneumonic[1] == 'B') value = Convert.ToUInt32(mneumonic.Substring(2), 2);
				if (hasByteRange) {
					value = GetByteRange(value, byte_start, byte_end);
				}
				return value;
            }

            // Check for negative value
            bool isNegative = false;
            if (mneumonic[0] == '-') {
                isNegative = true;
                mneumonic = mneumonic.Substring(1);
            }
            if (UInt32.TryParse(mneumonic, out UInt32 imm)) {
                if (isNegative) {
                    imm--;
                    imm ^= 0xFFFFFFFF;
                }
                value = imm;
				if (hasByteRange) {
					value = GetByteRange(value, byte_start, byte_end);
				}
				return value;
            }

            //InvalidValue(mneumonic);
            return null;
        }
		
		static UInt32 GetByteRange(UInt32 value, int byte_start, int byte_end) {
			int num_bytes = byte_end - byte_start;
			UInt32 mask = 0;
			while (num_bytes >= 0) {
				mask |= (UInt32) (0xFF << 8 * num_bytes);
				num_bytes--;
			} 
			return (value >> byte_start * 8) & mask;
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
