using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public static class Parser
	{

		/// <summary>
		/// Parse a program from a list of tokens.
		/// </summary>
		/// <param name="tokens">Tokens to parse.</param>
		/// <returns>A BISC program.</returns>
		public static Program Parse(List<Token> tokens)
		{
			LinkedList<Token> stream = new LinkedList<Token>(tokens);
			List<string> labels = new List<string>();

			Program program = new Program();

			while (stream.Count > 0)
			{
				switch (stream.Peek().Type)
				{
					case TokenType.Directive:
						ParseDirective(stream);
						break;

					case TokenType.DataInitializer:
						Binary binary = ParseDataInitializer(stream);
						program.Add(binary);
						break;

					case TokenType.Opcode:
						Instruction instruction = ParseInstruction(stream);
						program.Add(instruction);
						break;

					case TokenType.PseudoOp:
						ParsePseudoInstruction(stream);
						break;

					case TokenType.Label:
						Label label = ParseLabel(stream);
						program.Add(label);
						break;

					case TokenType.Comment:
					case TokenType.LineSeperator:
						stream.Dequeue();
						break;

					default:
						Fail(stream.First.Value, TokenType.Opcode);
						break;
				}
			}
			return program;
		}

		static Instruction ParseDirective(LinkedList<Token> tokens)
		{
			Token directive = tokens.Dequeue();
			return null;
		}

		static Binary ParseDataInitializer(LinkedList<Token> tokens)
		{
			Token operation = tokens.Dequeue();
			Token value = tokens.Dequeue();

			byte[] data;
			switch (operation.Mnemonic)
			{
				case Syntax.data_byte:
					MatchFail(value, TokenType.Immediate);
					int data_byte = (int)value.Value;
					data = new byte[1];
					data[0] = (byte)data_byte;
					return new Binary(data);

				case Syntax.data_half:
					MatchFail(value, TokenType.Immediate);
					int data_half = (int)value.Value;
					data = Specification.DisassembleInteger16((UInt16)data_half);
					return new Binary(data);

				case Syntax.data_word:
					MatchFail(value, TokenType.Immediate);
					int data_word = (int)value.Value;
					data = Specification.DisassembleInteger32((UInt32)data_word);
					return new Binary(data);

				case Syntax.data_zero:
					MatchFail(value, TokenType.Immediate);
					int number_of_zero_bytes = (int)value.Value;
					data = new byte[number_of_zero_bytes];
					return new Binary(data);
			}
			return null;
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
				if (tokens.Count > 0) MatchFail(tokens.Dequeue(), TokenType.LineSeperator);
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
			MatchFail(opcode, TokenType.Opcode);

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
			MatchFail(opcode, TokenType.Opcode);

			Token register = tokens.Dequeue();
			MatchFail(register, TokenType.Register);

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
			MatchFail(opcode, TokenType.Opcode);

			Token register = tokens.Dequeue();
			MatchFail(register, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			Token immediate = tokens.Dequeue();
			if (!Match(immediate, TokenType.Immediate) && !Match(immediate, TokenType.Label)) Fail(immediate, TokenType.Immediate);

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
			MatchFail(opcode, TokenType.Opcode);

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			Token source_register = tokens.Dequeue();
			MatchFail(source_register, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.OpenBracket);

			Token offset = tokens.Dequeue();
			if (!Match(offset, TokenType.Immediate) && !Match(offset, TokenType.Label)) Fail(offset, TokenType.Immediate);

			MatchFail(tokens.Dequeue(), TokenType.CloseBracket);

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
			MatchFail(opcode, TokenType.Opcode);

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			Token source_register = tokens.Dequeue();
			MatchFail(source_register, TokenType.Register);

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
			MatchFail(opcode, TokenType.Opcode);

			Token destination_register = tokens.Dequeue();
			MatchFail(destination_register, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			Token source_register_a = tokens.Dequeue();
			MatchFail(source_register_a, TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			Token source_register_b = tokens.Dequeue();
			MatchFail(source_register_b, TokenType.Register);

			return new TInstruction((Opcode)opcode.Value.Value, destination_register, source_register_a, source_register_b);
		}

		/// <summary>
		/// Parse a pseudo-instruction into its opcode replacements and push them onto the token stream.
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		static void ParsePseudoInstruction(LinkedList<Token> tokens)
		{
			Token pseudo_op = tokens.Dequeue();
			MatchFail(pseudo_op, TokenType.PseudoOp);

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
								if (temp_stream.Count == 0 || (!Match(temp_stream.Peek(), TokenType.Immediate) && !Match(temp_stream.Peek(), TokenType.Label))) valid = false;
								else temp_args.Add(temp_stream.Dequeue());
								if (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.CloseBracket)) valid = false;
								break;
							case ArgumentType.Immediate32:
								if (temp_stream.Count == 0 || (!Match(temp_stream.Peek(), TokenType.Immediate) && !Match(temp_stream.Peek(), TokenType.Label))) valid = false;
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
					Fail(pseudo_op, TokenType.Opcode);
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
			MatchFail(label, TokenType.Label);
			MatchFail(tokens.Dequeue(), TokenType.LabelDelimeter);
			return new Label(label.Mnemonic);
		}

		static bool Match(Token token, TokenType type)
		{
			return token.Type == type;
		}

		static void MatchFail(Token token, TokenType type)
		{
			if (!Match(token, type)) Fail(token, type);
		}

		static void Fail(Token token)
		{
			throw new ArgumentException($"Invalid token: {token}");

		}

		static void Fail(Token token, TokenType expected)
		{
			throw new ArgumentException($"Invalid token: {token} (expected {expected})");
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
