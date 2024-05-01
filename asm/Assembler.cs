using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	/// <summary>
	/// BISC Assembler.
	/// </summary>
	public static class Assembler
	{

		/// <summary>
		/// Assembles BISC source code into an intermediate program represnetation.
		/// </summary>
		/// <param name="source">Source code.</param>
		/// <returns>An executable program.</returns>
		public static Program Assemble(string source)
		{
			List<Token> tokens = Lexer.Tokenize(source);
			Program program = Parser.Parse(tokens);

			// First-pass optimizations
			//Optimizer.Optimize(program);

			program.AssignAddresses();
			program.ResolveUndefinedSymboles();

			return program;
		}

		/// <summary>
		/// Parses a string into an unsigned 32-bit integer.
		/// </summary>
		/// <param name="Mnemonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
		/// <returns>Unsigned 32-bit integer.</returns>
		public static UInt32? ParseImmediate(string Mnemonic)
		{
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

	}
}
