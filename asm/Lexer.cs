using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.BISC.Asm
{

	public static class Lexer
	{

		static int lineno;
		static int charno;

		public static List<Token> Tokenize(string source)
		{
			List<Token> tokens = new List<Token>();
			lineno = 1;
			charno = 0;

			Token token;
			string current_word = string.Empty;
			for (int source_index = 0; source_index < source.Length; source_index++)
			{
				char c = source[source_index];
				// TODO: Handle tab size
				charno++;


				// Skip carriage returns
				if (c == Syntax.carriage_return)
				{
					charno--;
					continue;
				}

				// Check for comment and skip rest of line
				if (c == Syntax.comment)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					current_word = c.ToString();
					while (source_index < source.Length - 1 && (c = source[++source_index]) != Syntax.line_seperator)
					{
						current_word += c;
						charno++;
					}

					charno++;
					tokens.Add(new Token(TokenType.Comment, current_word, null, lineno, charno - current_word.Length));

					tokens.Add(new Token(TokenType.LineSeperator, lineno, charno));

					current_word = string.Empty;
					lineno++;
					charno = 0;

					continue;
				}

				// Check for character literal
				if (c == Syntax.single_quote)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					current_word = c.ToString();
					current_word += source[++source_index];
					if (source[source_index] == '\\')
					{
						current_word += source[++source_index];
						charno++;
					}
					current_word += source[++source_index];
					charno += 2;

					// We have an issue
					//if (source[i] != Syntax.single_quote)

					continue;
				}

				// Check for string literal
				if (c == Syntax.double_quote)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					tokens.Add(new Token(TokenType.DoubleQuote, lineno, charno));

					// Loop until closing double quote
					// Skip escaped double quotes
					current_word = string.Empty;
					while (source[++source_index] != Syntax.double_quote || source[source_index - 1] == '\\')
					{
						// TODO: Check for line feed and throw error
						current_word += source[source_index];
						charno++;
					}

					charno++;
					tokens.Add(new Token(TokenType.String, current_word, null, lineno, charno - current_word.Length));

					tokens.Add(new Token(TokenType.DoubleQuote, lineno, charno));
					current_word = string.Empty;

					continue;
				}

				// Handle line feed
				if (c == Syntax.line_seperator)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					tokens.Add(new Token(TokenType.LineSeperator, lineno, charno));

					current_word = string.Empty;
					lineno++;
					charno = 0;
					continue;
				}

				// Handle whitespace
				if (char.IsWhiteSpace(c))
				{
					if (!string.IsNullOrEmpty(current_word))
					{
						tokens.Add(BuildToken(current_word));
						current_word = string.Empty;
					}
					continue;
				}

				token = BuildToken(c);
				if (token.IsValid)
				{
					if (!string.IsNullOrEmpty(current_word))
						tokens.Add(BuildToken(current_word));
					tokens.Add(token);
					current_word = string.Empty;
					continue;
				}

				current_word += c;
			}

			if (!string.IsNullOrEmpty(current_word))
			{
				charno++;
				tokens.Add(BuildToken(current_word));
			}

			return tokens;
		}

		static Token BuildToken(char lexeme)
		{
			switch (lexeme)
			{
				case Syntax.seperator: return new Token(TokenType.Seperator, lineno, charno);
				case Syntax.open_bracket: return new Token(TokenType.OpenBracket, lineno, charno);
				case Syntax.close_bracket: return new Token(TokenType.CloseBracket, lineno, charno);
				case Syntax.label_delimeter: return new Token(TokenType.LabelDelimeter, lineno, charno);
				case Syntax.directive_prefix: return new Token(TokenType.DirectivePrefix, lineno, charno);
				case Syntax.macro_expansion_operator: return new Token(TokenType.MacroExpansionOperator, lineno, charno);
			}

			return new Token(TokenType.Invalid, lexeme.ToString(), null, lineno, charno);
		}

		static Token BuildToken(string lexeme)
		{
			string lexeme_upper = lexeme.ToUpper();

			/*if (lexeme_upper[0] == Syntax.directive_prefix)
			{
				return new Token(TokenType.Directive, lexeme_upper, null, lineno, charno);
			}*/

			if (lexeme[0] == Syntax.data_prefix)
			{
				return new Token(TokenType.DataInitializer, lexeme, null, lineno, charno - lexeme.Length);
			}

			for (int index = 0; index < Specification.pseudo_instruction_names.Length; index++)
			{
				if (lexeme_upper == Specification.pseudo_instruction_names[index])
					return new Token(TokenType.PseudoOp, lexeme, (UInt32)index, lineno, charno - lexeme.Length);
			}

			Opcode? opcode;
			if ((opcode = Syntax.GetOpcode(lexeme_upper)).HasValue)
			{
				return new Token(TokenType.Opcode, opcode.ToString(), (UInt32)opcode.Value, lineno, charno - lexeme.Length);
			}

			Register? register;
			if ((register = Syntax.GetRegister(lexeme_upper)).HasValue)
			{
				return new Token(TokenType.Register, register.ToString(), (UInt32)register.Value, lineno, charno - lexeme.Length);
			}

			if (Regex.IsMatch(lexeme, Syntax.integer_literal) ||
				Regex.IsMatch(lexeme, Syntax.hexadecimal_literal) ||
				Regex.IsMatch(lexeme, Syntax.binary_literal) ||
				Regex.IsMatch(lexeme, Syntax.char_literal)
			)
			{
				return new Token(TokenType.Immediate, lexeme, Assembler.ParseImmediate(lexeme), lineno, charno - lexeme.Length);
			}

			if (Regex.IsMatch(lexeme, Syntax.identifer))
			{
				return new Token(TokenType.Identifier, lexeme, null, lineno, charno - lexeme.Length);
			}

			return new Token(TokenType.Invalid, lexeme, null, lineno, charno - lexeme.Length);
		}
	}
}
