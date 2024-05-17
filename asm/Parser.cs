using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public static class Parser
	{

		/// <summary>
		/// Name of current file being assembled; for debugging purposes.
		/// </summary>
		static string file_name;

		/// <summary>
		/// Parse a program from a list of tokens.
		/// </summary>
		/// <param name="tokens">Tokens to parse.</param>
		/// <returns>A BISC program.</returns>
		public static AssemblerResult Parse(List<Token> tokens, string file_name)
		{
			Parser.file_name = file_name;
			LinkedList<Token> stream = new LinkedList<Token>(tokens);
			AssemblerResult program = new AssemblerResult();

			while (stream.Count > 0)
			{
				switch (stream.Peek().Type)
				{
					case TokenType.DirectivePrefix:
						Directive directive = ParseDirective(stream);
						program.Add(directive);
						break;

					case TokenType.DataInitializer:
						BinaryData binary = ParseDataInitializer(stream);
						program.Add(binary);
						break;

					case TokenType.Opcode:
						Instruction instruction = ParseInstruction(stream);
						program.Add(instruction);
						break;

					case TokenType.PseudoOp:
						ParsePseudoInstruction(stream);
						break;

					case TokenType.Identifier:
						Label label = ParseLabel(stream);
						program.Add(label);
						break;

					case TokenType.Comment:
					case TokenType.LineSeperator:
						stream.Dequeue();
						break;

					default:
						Fail(stream.First.Value, TokenType.Opcode, $"Invalid opcode '{stream.First.Value.Mnemonic}'");
						break;
				}
			}
			return program;
		}

		static Directive ParseDirective(LinkedList<Token> tokens)
		{
			MatchFail(tokens.Dequeue(), TokenType.DirectivePrefix, $"Expected '{Syntax.directive_prefix}'");

			Token operation = tokens.Dequeue();
			MatchFail(operation, TokenType.Identifier, $"Expected directive keyword after '{Syntax.directive_prefix}'");

			switch (operation.Mnemonic.ToUpper())
			{
				case Syntax.directive_section: return ParseSectionInitialization(tokens);
				case Syntax.directive_define: return ParseMacroDefinition(tokens);
			}

			return null;
		}

		static SectionDirective ParseSectionInitialization(LinkedList<Token> tokens)
		{
			Token identifier = tokens.Dequeue();
			// These are being lexed as data initializers rn
			//MatchFail(identifier, TokenType.Identifier);

			return new SectionDirective(identifier.Mnemonic.ToLower());
		}

		// FIXME: This sucks
		static ConstantDefinitionDirective ParseMacroDefinition(LinkedList<Token> tokens)
		{
			Token identifier = tokens.Dequeue();
			MatchFail(identifier, TokenType.Identifier, $"Expected valid identifier after '%define'");

			// TODO: Make this more generic; replace with expression
			Token value = tokens.Dequeue();
			MatchFail(value, TokenType.Immediate, $"Expected immediate after constant definition");

			return new ConstantDefinitionDirective(identifier.Mnemonic, value);
		}

		static BinaryData ParseDataInitializer(LinkedList<Token> tokens)
		{
			Token operation = tokens.Dequeue();
			Token value = tokens.Dequeue();

			byte[] data;
			switch (operation.Mnemonic)
			{
				case Syntax.data_byte:
					MatchFail(value, TokenType.Immediate, "Expected valid byte value after '.byte' initializer");
					byte data_byte = (byte)(UInt32)value.Value;
					data = new byte[1] { data_byte };
					return new BinaryData(data);

				case Syntax.data_half:
					MatchFail(value, TokenType.Immediate, "Expected valid 16 bit value after '.half' initializer");
					UInt16 data_half = (UInt16)(UInt32)value.Value;
					data = data_half.DisassembleUInt16();
					return new BinaryData(data);

				case Syntax.data_word:
					MatchFail(value, TokenType.Immediate, "Expected valid 32 bit value after '.word' initializer");
					UInt32 data_word = (UInt32)value.Value;
					data = data_word.DisassembleUInt32();
					return new BinaryData(data);

				case Syntax.data_zero:
					MatchFail(value, TokenType.Immediate, "Expected valid immediate value after '.zero. initializer");
					UInt32 number_of_zero_bytes = (UInt32)value.Value;
					data = new byte[number_of_zero_bytes];
					return new BinaryData(data);

				case Syntax.data_string:

					MatchFail(value, TokenType.DoubleQuote, $"Expected '{Syntax.double_quote}' after '.string' initializer");

					value = tokens.Dequeue();
					MatchFail(value, TokenType.String, $"Invalid string \"{value.Mnemonic}\"");

					data = null;
					try
					{
						data = ParseString(value.Mnemonic);
					}
					catch (ArgumentException)
					{
						Fail(value, TokenType.String, $"Invalid string \"{value.Mnemonic}\"");
					}

					value = tokens.Dequeue();
					MatchFail(value, TokenType.DoubleQuote, $"String must end with '{Syntax.double_quote}'");

					return new BinaryData(data);
			}
			return null;
		}

		static byte[] ParseString(string value)
		{
			List<byte> bytes = new List<byte>(value.Length);

			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] != '\\')
				{
					bytes.Add((byte)value[i]);
					continue;
				}

				if (i >= value.Length - 1) throw new ArgumentException();

				switch (value[i + 1])
				{
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

		/// <summary>
		/// Parse an instruction from the given token stream.
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Peek();
			InstructionFormat format = Specification.instruction_formats[opcode.Value.Value];
			Instruction inst = null;
			switch (format)
			{
				case InstructionFormat.N: inst = ParseNInstruction(tokens); break;
				case InstructionFormat.R: inst = ParseRInstruction(tokens); break;
				case InstructionFormat.I: inst = ParseIInstruction(tokens); break;
				case InstructionFormat.M: inst = ParseMInstruction(tokens); break;
				case InstructionFormat.D: inst = ParseDInstruction(tokens); break;
				case InstructionFormat.T: inst = ParseTInstruction(tokens); break;
			}

			if (tokens.Count > 0)
			{
				if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
				if (tokens.Count > 0) MatchFail(tokens.Dequeue(), TokenType.LineSeperator, $"Expected newline after instruction '{inst.Opcode}'");
			}

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static NInstruction ParseNInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			return new NInstruction((Opcode)opcode.Value.Value);
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static RInstruction ParseRInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			Token register = tokens.Dequeue();
			MatchFail(register, TokenType.Register, $"Invalid register '{register.Mnemonic}' after opcode '{opcode.Mnemonic}'");

			return new RInstruction((Opcode)opcode.Value.Value, register);
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Immediate
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static IInstruction ParseIInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			Token register = tokens.Dequeue();
			MatchFail(register, TokenType.Register, $"Invalid register '{register.Mnemonic}' after opcode '{opcode.Mnemonic}'");

			MatchFail(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{register.Mnemonic}'");

			Token immediate = tokens.Dequeue();
			if (!Match(immediate, TokenType.Immediate) && !Match(immediate, TokenType.Identifier)) Fail(immediate, TokenType.Immediate, $"Expected immediate value after '{Syntax.seperator}'");

			return new IInstruction((Opcode)opcode.Value.Value, register, immediate);
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register[Immediate]
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static MInstruction ParseMInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register, $"Invalid register '{destination_register.Mnemonic}' after opcode '{opcode.Mnemonic}'");

			MatchFail(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register.Mnemonic}'");

			Token source_register = tokens.Dequeue();
			MatchFail(source_register, TokenType.Register, $"Invalid register '{source_register.Mnemonic}' after '{Syntax.seperator}'");

			MatchFail(tokens.Dequeue(), TokenType.OpenBracket, $"Expected '{Syntax.open_bracket}' after register {source_register.Mnemonic}");

			Token offset = tokens.Dequeue();
			if (!Match(offset, TokenType.Immediate) && !Match(offset, TokenType.Identifier)) Fail(offset, TokenType.Immediate, $"Invalid offset value '{offset.Mnemonic}' after '{Syntax.open_bracket}'");

			MatchFail(tokens.Dequeue(), TokenType.CloseBracket, $"Expected '{Syntax.open_bracket}' after offset {offset.Mnemonic}");

			return new MInstruction((Opcode)opcode.Value.Value, destination_register, source_register, offset);
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static DInstruction ParseDInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register, $"Invalid register '{destination_register.Mnemonic}' after opcode '{opcode.Mnemonic}'");

			MatchFail(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register.Mnemonic}'");

			Token source_register = tokens.Dequeue();
			MatchFail(source_register, TokenType.Register, $"Invalid register '{source_register.Mnemonic}' after '{Syntax.seperator}'");

			return new DInstruction((Opcode)opcode.Value.Value, destination_register, source_register);
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register, Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static TInstruction ParseTInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode, $"Invalid opcode '{opcode.Mnemonic}'");

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register, $"Invalid register '{destination_register.Mnemonic}' after opcode '{opcode.Mnemonic}'");

			MatchFail(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register.Mnemonic}'");

			Token source_register_a = tokens.Dequeue();
			MatchFail(source_register_a, TokenType.Register, $"Invalid register '{source_register_a.Mnemonic}' after '{Syntax.seperator}'");

			MatchFail(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{source_register_a.Mnemonic}'");

			Token source_register_b = tokens.Dequeue();
			MatchFail(source_register_b, TokenType.Register, $"Invalid register '{source_register_a.Mnemonic}' after '{Syntax.seperator}'");

			return new TInstruction((Opcode)opcode.Value.Value, destination_register, source_register_a, source_register_b);
		}

		/// <summary>
		/// Parse a pseudo-instruction into its opcode replacements and push them onto the token stream.
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		static void ParsePseudoInstruction(LinkedList<Token> tokens)
		{
			Token pseudo_op = tokens.Dequeue();
			MatchFail(pseudo_op, TokenType.PseudoOp, $"Invalid opcode '{pseudo_op.Mnemonic}'");

			List<Token> args = new List<Token>();
			while (tokens.Count > 0)
			{
				Token token = tokens.Dequeue();
				if (Match(token, TokenType.LineSeperator)) break;
				if (Match(token, TokenType.Comment)) continue;
				args.Add(token);
			}

			int index = (int)pseudo_op.Value;
			for (; index < Specification.pseudo_instruction_names.Length; index++)
			{
				if (pseudo_op.Mnemonic == Specification.pseudo_instruction_names[index])
				{
					bool valid = true;
					Queue<Token> temp_stream = new Queue<Token>(args);
					List<Token> temp_args = new List<Token>(3);
					ArgumentType[] arg_types = Specification.pseudo_instruction_arguments[index];
					for (int a = 0; valid && a < arg_types.Length; a++)
					{
						switch (arg_types[a])
						{
							case ArgumentType.None:
								if (temp_stream.Count > 0) valid = false;
								break;
							case ArgumentType.Register:
								if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Register)) valid = false;
								else temp_args.Add(temp_stream.Dequeue());
								break;
							case ArgumentType.Memory:
								if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Register)) valid = false;
								else temp_args.Add(temp_stream.Dequeue());
								if (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.OpenBracket)) valid = false;
								if (temp_stream.Count == 0 || (!Match(temp_stream.Peek(), TokenType.Immediate) && !Match(temp_stream.Peek(), TokenType.Identifier))) valid = false;
								else temp_args.Add(temp_stream.Dequeue());
								if (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.CloseBracket)) valid = false;
								break;
							case ArgumentType.Immediate32:
								if (temp_stream.Count == 0 || (!Match(temp_stream.Peek(), TokenType.Immediate) && !Match(temp_stream.Peek(), TokenType.Identifier))) valid = false;
								else temp_args.Add(temp_stream.Dequeue());
								break;
						}
						if (a < arg_types.Length - 1 && (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.Seperator))) valid = false;
					}
					if (temp_stream.Count > 0) valid = false;
					if (valid)
					{
						args = temp_args;
						break;
					}
				}
			}

			if (index >= Specification.pseudo_instruction_names.Length)
			{
				Opcode? opcode = Syntax.GetOpcode(pseudo_op.Mnemonic);
				if (opcode.HasValue)
				{
					pseudo_op.Type = TokenType.Opcode;
					pseudo_op.Value = (UInt32)opcode.Value;
				}
				else
				{
					Fail(pseudo_op, TokenType.Opcode, $"Invalid opcode '{pseudo_op.Mnemonic}'");
				}
				tokens.AddFirst(new Token(TokenType.LineSeperator, pseudo_op.LineNo, 0));
				for (int v = args.Count - 1; v >= 0; v--)
				{
					tokens.AddFirst(args[v]);
				}
				tokens.AddFirst(pseudo_op);
				return;
			}

			for (int i = args.Count; i < 3; i++)
			{
				args.Add(new Token(TokenType.Invalid, 0, 0));
			}

			string[] definition = Specification.pseudo_instruction_definitions[index];
			string[] replacements = new string[definition.Length];
			Array.Copy(definition, replacements, replacements.Length);

			for (int i = replacements.Length - 1; i >= 0; i--)
			{
				replacements[i] = string.Format(replacements[i], args[0].Mnemonic, args[1].Mnemonic, args[2].Mnemonic);
				replacements[i] += '\n';
				List<Token> vals = Lexer.Tokenize(replacements[i]);
				for (int v = vals.Count - 1; v >= 0; v--)
				{
					Token t = vals[v];
					t.LineNo = pseudo_op.LineNo;
					tokens.AddFirst(t);
				}
			}

			return;
		}

		/// <summary>
		/// Parse a label and return its name.
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>Name of the label.</returns>
		static Label ParseLabel(LinkedList<Token> tokens)
		{
			Token label = tokens.Dequeue();
			MatchFail(label, TokenType.Identifier, $"Invalid label identifier '{label.Mnemonic}'");

			MatchFail(tokens.Dequeue(), TokenType.LabelDelimeter, $"Expected '{Syntax.label_delimeter}' after label definition '{label.Mnemonic}'");
			return new Label(label.Mnemonic);
		}

		static bool Match(Token token, TokenType type)
		{
			return token.Type == type;
		}

		static void MatchFail(Token token, TokenType type, string message)
		{
			if (!Match(token, type)) Fail(token, type, message);
		}

		static void Fail(Token token, TokenType expected, string message)
		{
			throw new AssemblerSyntaxErrorException($"\u001b[1m{file_name}:{token.LineNo}:{token.CharNo}: \u001b[31merror:\u001b[39m {message}\u001b[0m");
		}

		static Token Dequeue(this LinkedList<Token> tokens)
		{
			Token node = tokens.First.Value;
			tokens.RemoveFirst();
			return node;
		}

		static Token Peek(this LinkedList<Token> tokens)
		{
			return tokens.First.Value;
		}
	}
}
