using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	/// <summary>
	/// BISC Assembler.
	/// </summary>
	public static class Assembler
	{

		static Dictionary<string, byte> opcodes;
		static Dictionary<string, UInt32> symbols;
		static Program program;

		/// <summary>
		/// Assembles BISC source code into binary opcodes.
		/// </summary>
		/// <param name="source">Source code.</param>
		/// <returns>An executable program.</returns>
		public static byte[] Assemble(string source)
		{

			// Initialize opcodes dictionary to convert string Mnemonic to byte value
			if (opcodes == null)
			{
				opcodes = new Dictionary<string, byte>();
				foreach (Opcode opcode in Enum.GetValues(typeof(Opcode)))
				{
					opcodes[opcode.ToString()] = (byte)opcode;
				}
			}

			// Initialize dictionaries for known symbol values and instructions with unresolved symbols
			symbols = new Dictionary<string, UInt32>();
			List<Instruction> unresolved_symbols = new List<Instruction>();

			List<Token> tokens = Lexer.Tokenize(source);
			if (tokens.Count <= 0) return new byte[] { };
			foreach (Token token in tokens)
			{
				Console.WriteLine(token);
			}

			program = Parser.Parse(tokens);

			if (program.Count <= 0) return new byte[] { };

			// First-pass optimizations
			//Optimizer.Optimize(program);

			UInt32 address = 0;
			for (int i = 0; i < program.Count; i++)
			{
				program[i].Address = address;
				if (program[i] is Instruction) address += 4;
			}

			for (int a = 0; a < program.Count; a++)
			{
				Assembloid assembloid = program[a];

				if (assembloid.HasUndefinedSymbol())
				{
					if (assembloid is Instruction)
					{
						Instruction instruction = assembloid as Instruction;
						for (int i = 0; i < instruction.Parameters.Length; i++)
						{
							Token arg = instruction.Parameters[i];
							if (arg.Type == TokenType.Label && !arg.Value.HasValue)
							{
								arg.Type = TokenType.Immediate;
								Label label = program.GetLabel(arg.Mnemonic);

								if (label == null)
								{
									throw new ArgumentException($"(Ln: {arg.LineNo}, Ch: {arg.CharNo}) Undefined symbol: '{arg.Mnemonic}'\n'{instruction}'");
								}

								arg.Value = label.Address;
								instruction.Parameters[i] = arg;
								/*if (!program.Labels.TryGetValue(arg.Mnemonic, out Instruction label))
								{
									foreach (var o in program.Labels)
									{
										Console.WriteLine(o);
									}
									throw new ArgumentException($"(Ln: {arg.LineNo}, Ch: {arg.CharNo}) Undefined symbol: '{arg.Mnemonic}'\n'{inst}'");
								}
								arg.Value = program.Labels[arg.Mnemonic].Address;
								inst.Parameters[i] = arg;*/
							}
						}
					}
				}
				Console.WriteLine(assembloid);
			}

			//byte[] machine_code = new byte[program.SizeInBytes];
			List<byte> machine_code = new List<byte>();
			for (int i = 0; i < program.Count; i++)
			{
				if (program[i] is Instruction)
				{
					//machine_code[machine_code_index++] = AssembleInstruction(program[i] as Instruction);
					machine_code.AddRange(Specification.DisassembleInteger32(AssembleInstruction(program[i] as Instruction)));
				}

				if (program[i] is Binary)
				{
					//machine_code[machine_code_index++] = AssembleInstruction(program[i] as Instruction);
					machine_code.AddRange((program[i] as Binary).Data);
				}
			}

			if (machine_code.Count != program.SizeInBytes) Console.WriteLine($"Machine code length: {machine_code.Count}, expected length: {program.SizeInBytes}");

			return machine_code.ToArray();
		}

		static UInt32 AssembleInstruction(Instruction inst)
		{
			UInt32 machine_code = (UInt32)inst.Opcode;
			for (int i = 0; i < inst.Parameters.Length; i++)
			{
				Token arg = inst.Parameters[i];
				switch (arg.Type)
				{
					case TokenType.Register:
						machine_code |= arg.Value.Value << (i + 1) * 8;
						break;
					case TokenType.Label:
					/*if (!arg.Value.HasValue) {
						arg.Value = program.Labels[arg.Mnemonic].Address;
					}
					// This fall-through is intentional - labels are treated as immediates
					goto case TokenType.Immediate;*/
					case TokenType.Immediate:
						// TODO: Make this case not an absolute hack; this code sucks mega nuts
						// If 3 parameters, immediate has to be 8 bits, otherwise 16 bits
						if (inst.Parameters.Length == 3) machine_code |= (arg.Value.Value & 0xFF) << (i + 1) * 8;
						// If opcode is LUI, load top 16 bits
						else if (inst.Opcode == Opcode.LUI) machine_code |= ((arg.Value.Value >> 16) & 0xFFFF) << (i + 1) * 8;
						else machine_code |= (arg.Value.Value & 0xFFFF) << (i + 1) * 8;
						break;
				}

			}
			return machine_code;
		}

		/// <summary>
		/// Parses a string instruction into a binary opcode.
		/// </summary>
		/// <param name="Mnemonic">String to parse.</param>
		/// <returns>Opcode in binary form.</returns>
		public static byte? ParseOpcode(string Mnemonic)
		{
			if (opcodes.TryGetValue(Mnemonic.ToUpper(), out byte opcode)) return opcode;
			return null;
		}

		/// <summary>
		/// Parses a register name into its index into the register file.
		/// </summary>
		/// <param name="Mnemonic">Register name.</param>
		/// <returns>Register index.</returns>
		public static byte? ParseRegister(string Mnemonic)
		{
			for (byte i = 0; i < Specification.NUM_REGISTERS; i++)
			{
				if (Mnemonic == Specification.REGISTER_NAMES[i]) return i;
			}
			return null;
		}

		static UInt32? ParseLabel(string Mnemonic)
		{
			if (symbols.TryGetValue(Mnemonic, out UInt32 value)) return value;
			return null;
		}

		static UInt32 ResolveSymbol(string symbol)
		{
			if (!symbols.TryGetValue(symbol, out UInt32 value)) return 0xFFFFFFFF;
			return value;
		}

		public static UInt16? ParseMemory(string reg, string offset)
		{
			byte? reg_val = ParseRegister(reg);
			byte? imm = (byte)ParseImmediate(offset);
			return Specification.AssembleInteger16(reg_val.Value, imm.Value);
		}

		public static UInt16? ParseMemory(string Mnemonic)
		{
			byte? reg = ParseRegister(Mnemonic.Substring(0, 2));
			if (!reg.HasValue || Mnemonic.IndexOf('[') <= 0) return null;

			string val = Mnemonic.Substring(Mnemonic.IndexOf('[') + 1, Mnemonic.IndexOf(']') - Mnemonic.IndexOf('[') - 1);

			// Check for negative value
			bool isNegative = false;
			if (val[0] == '-')
			{
				isNegative = true;
				val = val.Substring(1);
			}

			if (byte.TryParse(val, out byte imm))
			{
				if (isNegative)
				{
					imm--;
					imm ^= 0xFF;
				}
			}

			return Specification.AssembleInteger16(reg.Value, imm);
		}

		/// <summary>
		/// Parses a string into an unsigned 32-bit integer.
		/// </summary>
		/// <param name="Mnemonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
		/// <returns>Unsigned 32-bit integer.</returns>
		public static UInt32? ParseImmediate(string Mnemonic)
		{

			// Check for label or symbol
			if (symbols.ContainsKey(Mnemonic))
			{
				return symbols[Mnemonic];
			}

			// Check for ASCII character
			if (Mnemonic[0] == '\'')
			{
				if (Mnemonic.Length != 3 || Mnemonic[2] != '\'')
				{
					//InvalidValue(Mnemonic);
					return null;
				}
				return Mnemonic[1];
			}

			// Check if value has prefix
			UInt32 value = 0;
			if (Mnemonic.Length >= 3 && Mnemonic[0] == '0')
			{
				// Check for hexadecimal value
				if (Mnemonic[1] == 'x' || Mnemonic[1] == 'X') value = Convert.ToUInt32(Mnemonic.Substring(2), 16);
				// Check for binary value
				if (Mnemonic[1] == 'b' || Mnemonic[1] == 'B') value = Convert.ToUInt32(Mnemonic.Substring(2), 2);
				return value;
			}

			// Check for negative value
			bool isNegative = false;
			if (Mnemonic[0] == '-')
			{
				isNegative = true;
				Mnemonic = Mnemonic.Substring(1);
			}
			if (UInt32.TryParse(Mnemonic, out UInt32 imm))
			{
				if (isNegative)
				{
					imm--;
					imm ^= 0xFFFFFFFF;
				}
				value = imm;
				return value;
			}

			return null;
		}

		static void InvalidValue(string Mnemonic)
		{
			//Console.Error.WriteLine($"Invalid value at line {lineno}: {Mnemonic}");
		}

		static void InvalidInstruction(string Mnemonic)
		{
			//Console.Error.WriteLine($"Invalid instruction at line {lineno}: {Mnemonic}");
		}
	}
}
