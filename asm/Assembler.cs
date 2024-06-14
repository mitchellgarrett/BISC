using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {

	/// <summary>
	/// BISC Assembler.
	/// </summary>
	public static class Assembler {

		/// <summary>
		/// Assembles BISC source code into an intermediate program representation.
		/// </summary>
		/// <param name="source">Source code.</param>
		/// <returns>An executable program.</returns>
		public static AssemblyTree Assemble(string file_name, string source) {
			List<Token> tokens = Lexer.Tokenize(source);

			foreach (var token in tokens) Console.WriteLine(token);

			AssemblyTree program = Parser.Parse(tokens, file_name);
			
			Console.WriteLine(program);
			
			Preprocessor.EvaluateMacros(program);
			
			SemanticAnalyzer.AssignAddresses(program);
			SemanticAnalyzer.ResolveLabels(program);

			// First-pass optimizations
			//Optimizer.Optimize(program);

			Console.WriteLine(program);

			return program;
		}

		/// <summary>
		/// Parses a string into an unsigned 32-bit integer.
		/// </summary>
		/// <param name="lexeme">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
		/// <returns>Unsigned 32-bit integer.</returns>
		public static UInt32? ParseImmediate(string lexeme) {
			// Check for ASCII character
			if (lexeme[0] == '\'')
			{
				if (lexeme.Length == 3)
				{
					if (lexeme[2] != '\'') return null;
					return lexeme[1];
				}

				// Check for escape characters
				if (lexeme.Length == 4)
				{
					if (lexeme[1] != '\\' || lexeme[3] != '\'') return null;

					switch (lexeme[2])
					{
						case '0': return '\0';
						case 'b': return '\b';
						case 't': return '\t';
						case 'n': return '\n';
						case 'r': return '\r';
						case '\\': return '\\';
						case '"': return '"';
					}
				}

				return null;
			}

			// Check if value has prefix
			UInt32 value = 0;
			if (lexeme.Length >= 3 && lexeme[0] == '0')
			{
				// Check for hexadecimal value
				if (lexeme[1] == 'x' || lexeme[1] == 'X') value = Convert.ToUInt32(lexeme.Substring(2), 16);
				// Check for binary value
				if (lexeme[1] == 'b' || lexeme[1] == 'B') value = Convert.ToUInt32(lexeme.Substring(2), 2);
				return value;
			}

			// Check for negative value
			bool isNegative = false;
			if (lexeme[0] == '-')
			{
				isNegative = true;
				lexeme = lexeme.Substring(1);
			}
			if (UInt32.TryParse(lexeme, out UInt32 imm))
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
