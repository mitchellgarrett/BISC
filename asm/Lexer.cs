using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.BISC.Asm
{

	public static class Lexer
	{

		static int lineno = 1;
		static int charno;

		public static void Reset()
		{
			lineno = 1;
		}

		public static List<Token> Tokenize(string source)
		{
			List<Token> tokens = new List<Token>();
			charno = 1;

			Token token;
			string current_word = string.Empty;
			for (int i = 0; i < source.Length; i++)
			{
				char c = source[i];
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
					while ((c = source[++i]) != Syntax.line_seperator)
					{
						current_word += c;
						charno++;
					}
				}

				// Check for character literal
				if (c == Syntax.single_quote)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					current_word = c.ToString();
					current_word += source[++i];
					if (source[i] == '\\')
					{
						current_word += source[++i];
						charno++;
					}
					current_word += source[++i];
					charno += 2;

					// We have an issue
					//if (source[i] != Syntax.single_quote)

					continue;
				}

				// Check for string literal

				if (c == Syntax.line_seperator)
				{
					if (!string.IsNullOrEmpty(current_word)) tokens.Add(BuildToken(current_word));

					tokens.Add(new Token(TokenType.LineSeperator, lineno, charno));

					current_word = string.Empty;
					lineno++;
					charno = 0;
					continue;
				}

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
				case Syntax.double_quote: return new Token(TokenType.DoubleQuote, lineno, charno);
				case Syntax.comment: return new Token(TokenType.Comment, lineno, charno);
			}

			return new Token(TokenType.Invalid, lexeme.ToString(), null, lineno, charno);
		}

		static Token BuildToken(string lexeme)
		{
			string lexeme_upper = lexeme.ToUpper();

			if (lexeme_upper[0] == Syntax.directive_prefix)
			{
				return new Token(TokenType.Directive, lexeme_upper, null, lineno, charno);
			}

			if (lexeme_upper[0] == Syntax.data_prefix)
			{
				return new Token(TokenType.DataInitializer, lexeme_upper, null, lineno, charno);
			}

			for (int index = 0; index < Specification.pseudo_instruction_names.Length; index++)
			{
				if (lexeme_upper == Specification.pseudo_instruction_names[index])
					return new Token(TokenType.PseudoOp, lexeme_upper, (UInt32)index, lineno, charno);
			}

			Opcode? opcode;
			if ((opcode = Syntax.GetOpcode(lexeme_upper)).HasValue)
			{
				return new Token(TokenType.Opcode, opcode.ToString(), (UInt32)opcode.Value, lineno, charno);
			}

			Register? register;
			if ((register = Syntax.GetRegister(lexeme_upper)).HasValue)
			{
				return new Token(TokenType.Register, register.ToString(), (UInt32)register.Value, lineno, charno);
			}

			if (Regex.IsMatch(lexeme, Syntax.integer_literal) ||
				Regex.IsMatch(lexeme, Syntax.hexadecimal_literal) ||
				Regex.IsMatch(lexeme, Syntax.binary_literal) ||
				Regex.IsMatch(lexeme, Syntax.char_literal)
			)
			{
				return new Token(TokenType.Immediate, lexeme, Assembler.ParseImmediate(lexeme), lineno, charno);
			}

			if (Regex.IsMatch(lexeme, Syntax.identifer))
			{
				return new Token(TokenType.Label, lexeme, null, lineno, charno);
			}

			if (lexeme.Length > 0 && lexeme[0] == Syntax.comment)
			{
				return new Token(TokenType.Comment, lexeme, null, lineno, charno);
			}

			return new Token(TokenType.Invalid, lexeme, null, lineno, charno);
		}
	}
}
