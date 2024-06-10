using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {

	public static partial class Parser {
		
		static AssemblyNode.Directive ParseDirective(LinkedList<Token> tokens) {
			Expect(tokens.Dequeue(), TokenType.DirectivePrefix, "FIXME: directive prefix");
			Expect(tokens.Peek(), TokenType.Identifier, "FIXME: directive prefix");
			
			switch (tokens.Peek().Mnemonic.ToLower()) {
				case Syntax.directive_section: return ParseSectionDefinition(tokens);
				default:
					Fail(tokens.Peek(), TokenType.Identifier, $"Invalid directive '{tokens.Peek().Mnemonic}' after directive prefix '{Syntax.directive_prefix}'");
					return null;
			}
		}
		
		static AssemblyNode.SectionDefinition ParseSectionDefinition(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			return new AssemblyNode.SectionDefinition(token.Mnemonic);
		}
		
		static AssemblyNode.DataInitializer ParseDataInitializer(LinkedList<Token> tokens) {
			Token operation = tokens.Dequeue();
			return operation.Mnemonic switch {
				Syntax.data_byte => ParseByteInitializer(tokens),
				Syntax.data_half => ParseHalfWordInitializer(tokens),
				Syntax.data_word => ParseWordInitializer(tokens),
				Syntax.data_zero => ParseNullInitializer(tokens),
				Syntax.data_string => ParseStringInitializer(tokens),
				_ => null,
			};
		}
		
		static AssemblyNode.DataInitializer ParseByteInitializer(LinkedList<Token> tokens) {
			Token value = tokens.Dequeue();
			Expect(value, TokenType.Immediate, "Expected valid byte value after '.byte' initializer");
			byte[] data = new byte[1] { (byte)(UInt32)value.Value };
			return new AssemblyNode.DataInitializer(data);
		}
		
		static AssemblyNode.DataInitializer ParseHalfWordInitializer(LinkedList<Token> tokens) {
			Token value = tokens.Dequeue();
			Expect(value, TokenType.Immediate, "Expected valid 16 bit value after '.half' initializer");
			UInt16 data_half = (UInt16)(UInt32)value.Value;
			byte[] data = data_half.DisassembleUInt16();
			return new AssemblyNode.DataInitializer(data);
		}
		
		static AssemblyNode.DataInitializer ParseWordInitializer(LinkedList<Token> tokens) {
			Token value = tokens.Dequeue();
			Expect(value, TokenType.Immediate, "Expected valid 32 bit value after '.word' initializer");
			UInt32 data_word = (UInt32)value.Value;
			byte[] data = data_word.DisassembleUInt32();
			return new AssemblyNode.DataInitializer(data);
		}
		
		static AssemblyNode.DataInitializer ParseNullInitializer(LinkedList<Token> tokens) {
			Token value = tokens.Dequeue();
			Expect(value, TokenType.Immediate, "Expected valid immediate value after '.zero. initializer");
			UInt32 number_of_zero_bytes = (UInt32)value.Value;
			byte[] data = new byte[number_of_zero_bytes];
			return new AssemblyNode.DataInitializer(data);
		}
		
		static AssemblyNode.DataInitializer ParseStringInitializer(LinkedList<Token> tokens) {
			Token value = tokens.Dequeue();
			Expect(value, TokenType.DoubleQuote, $"Expected '{Syntax.double_quote}' after '.string' initializer");

			value = tokens.Dequeue();
			Expect(value, TokenType.String, $"Invalid string \"{value.Mnemonic}\"");

			byte[] data = null;
			try {
				data = ParseString(value.Mnemonic);
			} catch (ArgumentException) {
				Fail(value, TokenType.String, $"Invalid string \"{value.Mnemonic}\"");
			}

			value = tokens.Dequeue();
			Expect(value, TokenType.DoubleQuote, $"String must end with '{Syntax.double_quote}'");

			return new AssemblyNode.DataInitializer(data);
		}
		
		static byte[] ParseString(string value) {
			List<byte> bytes = new List<byte>(value.Length);

			for (int i = 0; i < value.Length; i++) {
				if (value[i] != '\\') {
					bytes.Add((byte)value[i]);
					continue;
				}

				if (i >= value.Length - 1) throw new ArgumentException();

				switch (value[i + 1]) {
					case '0':
						bytes.Add((byte)'\0');
						break;

					case 'b':
						bytes.Add((byte)'\b');
						break;

					case 't':
						bytes.Add((byte)'\t');
						break;

					case 'n':
						bytes.Add((byte)'\n');
						break;

					case 'r':
						bytes.Add((byte)'\r');
						break;

					case '\\':
						bytes.Add((byte)'\\');
						break;

					case '"':
						bytes.Add((byte)'"');
						break;

					default: throw new ArgumentException();
				}

				// Increment index to skip rest of escape sequence
				i++;
			}

			return bytes.ToArray();
		}
	}
}