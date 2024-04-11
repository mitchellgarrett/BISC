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
					case TokenType.Opcode:
						Instruction instruction = ParseInstruction(stream);

						Console.WriteLine(instruction);
						program.Instructions.Add(instruction);

						// FIXME: This needs to work for the case wehre two labels are stacked on top of each other
						if (labels.Count > 0)
						{
							foreach (string l in labels)
							{
								program.Labels[l] = instruction;
								//Console.WriteLine("\n\n\n" + label + "\n\n\n");
							}
							labels.Clear();
						}
						break;
					case TokenType.PseudoOp:
						ParsePseudoInstruction(stream);
						break;
					case TokenType.Label:
						string label = ParseLabel(stream);
						if (!string.IsNullOrEmpty(label)) labels.Add(label);
						//Console.WriteLine("\n\n\n" + label + "\n\n\n");
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
				if ((tokens.Count > 0)) MatchFail(tokens.Dequeue(), TokenType.LineSeperator);
			}

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseNInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseRInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;
			inst.Parameters = new Token[1];

			inst.Parameters[0] = tokens.Dequeue();
			MatchFail(inst.Parameters[0], TokenType.Register);

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register, Immediate
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseIInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;
			inst.Parameters = new Token[2];

			inst.Parameters[0] = tokens.Dequeue();
			MatchFail(inst.Parameters[0], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			inst.Parameters[1] = tokens.Dequeue();
			if (!Match(inst.Parameters[1], TokenType.Immediate) && !Match(inst.Parameters[1], TokenType.Label)) Fail(inst.Parameters[1], TokenType.Immediate);

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register[Immediate]
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseMInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;
			inst.Parameters = new Token[3];

			inst.Parameters[0] = tokens.Dequeue();
			MatchFail(inst.Parameters[0], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			inst.Parameters[1] = tokens.Dequeue();
			MatchFail(inst.Parameters[1], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.OpenBracket);

			inst.Parameters[2] = tokens.Dequeue();
			if (!Match(inst.Parameters[2], TokenType.Immediate) && !Match(inst.Parameters[2], TokenType.Label)) Fail(inst.Parameters[2], TokenType.Immediate);

			MatchFail(tokens.Dequeue(), TokenType.CloseBracket);

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseDInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;
			inst.Parameters = new Token[2];

			inst.Parameters[0] = tokens.Dequeue();
			MatchFail(inst.Parameters[0], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			inst.Parameters[1] = tokens.Dequeue();
			MatchFail(inst.Parameters[1], TokenType.Register);

			return inst;
		}

		/// <summary>
		/// Parse an instruction with format: Opcode Register, Register, Register
		/// </summary>
		/// <param name="tokens">Token stream.</param>
		/// <returns>A single BISC instruction.</returns>
		static Instruction ParseTInstruction(LinkedList<Token> tokens)
		{
			Instruction inst = new Instruction();
			Token opcode = tokens.Dequeue();
			MatchFail(opcode, TokenType.Opcode);

			inst.Opcode = (Opcode)opcode.Value.Value;
			inst.Parameters = new Token[3];

			inst.Parameters[0] = tokens.Dequeue();
			MatchFail(inst.Parameters[0], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			inst.Parameters[1] = tokens.Dequeue();
			MatchFail(inst.Parameters[1], TokenType.Register);

			MatchFail(tokens.Dequeue(), TokenType.Seperator);

			inst.Parameters[2] = tokens.Dequeue();
			MatchFail(inst.Parameters[2], TokenType.Register);

			return inst;
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
		static string ParseLabel(LinkedList<Token> tokens)
		{
			Token label = tokens.Dequeue();
			MatchFail(label, TokenType.Label);
			MatchFail(tokens.Dequeue(), TokenType.LabelDelimeter);
			return label.Mnemonic;
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
			//Console.Error.WriteLine($"Invalid token: {token}");
			//Environment.Exit(1);
			throw new ArgumentException($"Invalid token: {token}");

		}

		static void Fail(Token token, TokenType expected)
		{
			//Console.Error.WriteLine($"Invalid token: {token} (expected {expected})");
			//Environment.Exit(1);
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
