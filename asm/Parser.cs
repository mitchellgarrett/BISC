using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public static partial class Parser {

		/// <summary>
		/// Name of current file being assembled; for debugging purposes.
		/// </summary>
		static string file_name;

		/// <summary>
		/// Parse a program from a list of tokens.
		/// </summary>
		/// <param name="tokens">Tokens to parse.</param>
		/// <returns>A BISC program.</returns>
		public static AssemblyTree Parse(List<Token> tokens, string file_name)
		{
			Parser.file_name = file_name;
			LinkedList<Token> stream = new LinkedList<Token>(tokens);
			
			AssemblyNode.Program program = ParseProgram(stream);
			return new AssemblyTree(program);
		}
		
		static AssemblyNode.Program ParseProgram(LinkedList<Token> tokens) {
			List<AssemblyNode.BlockItem> body = new List<AssemblyNode.BlockItem>();
			while (tokens.Count > 0) {
				AssemblyNode.BlockItem item = ParseBlockItem(tokens);
				if (item != null) body.Add(item);
			}
			return new AssemblyNode.Program(body);
		}

		static AssemblyNode.BlockItem ParseBlockItem(LinkedList<Token> tokens) {
			switch (tokens.Peek().Type)
				{
					case TokenType.Opcode: return ParseInstruction(tokens);
					
					case TokenType.Identifier: return ParseLabel(tokens);
					
					case TokenType.Comment:
					case TokenType.LineSeperator:
						tokens.Dequeue();
						return null;
						
					case TokenType.PseudoOp:
						ParsePseudoInstruction(tokens);
						return null;
					
					/*case TokenType.DirectivePrefix:
						return ParseDirective(tokens);

					case TokenType.DataInitializer:
						return ParseDataInitializer(tokens);
*/
					default:
						Fail(tokens.Peek(), TokenType.Opcode, $"Invalid opcode '{tokens.Peek().Mnemonic}'");
						return null;
				}
		}
		
		static AssemblyNode.Instruction ParseInstruction(LinkedList<Token> tokens)
		{
			Token opcode = tokens.Peek();
			InstructionFormat format = Specification.instruction_formats[opcode.Value.Value];
			switch (format)
			{
				case InstructionFormat.N: return ParseNInstruction(tokens);
				case InstructionFormat.R: return ParseRInstruction(tokens);
				case InstructionFormat.I: return ParseIInstruction(tokens);
				case InstructionFormat.M: return ParseMInstruction(tokens);
				case InstructionFormat.D: return ParseDInstruction(tokens);
				case InstructionFormat.T: return ParseTInstruction(tokens);
				default: return null;
			}
		}
		
		static AssemblyNode.NInstruction ParseNInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			
			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);
			
			return new AssemblyNode.NInstruction(opcode);
		}
		
		static AssemblyNode.RInstruction ParseRInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			Register register = ParseRegister(tokens);
			
			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);
			
			return new AssemblyNode.RInstruction(opcode, register);
		}
		
		static AssemblyNode.IInstruction ParseIInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			Register register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{register}'");

			AssemblyNode.Constant immediate = ParseConstant(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);

			return new AssemblyNode.IInstruction(opcode, register, immediate);
		}
		
		static AssemblyNode.MInstruction ParseMInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			Register source_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.OpenBracket, $"Expected '{Syntax.open_bracket}' after register {source_register}");

			AssemblyNode.Constant offset = ParseConstant(tokens);

			Expect(tokens.Dequeue(), TokenType.CloseBracket, $"Expected '{Syntax.open_bracket}' after offset {offset}");

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);

			return new AssemblyNode.MInstruction(opcode, destination_register, source_register, offset);
		}
		
		static AssemblyNode.DInstruction ParseDInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			Register source_register = ParseRegister(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);

			return new AssemblyNode.DInstruction(opcode, destination_register, source_register);
		}
		
		static AssemblyNode.TInstruction ParseTInstruction(LinkedList<Token> tokens)
		{
			Opcode opcode = ParseOpcode(tokens);
			Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			Register source_register_a = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{source_register_a}'");

			Register source_register_b = ParseRegister(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, null);

			return new AssemblyNode.TInstruction(opcode, destination_register, source_register_a, source_register_b);
		}
		
		static Opcode ParseOpcode(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			Expect(token, TokenType.Opcode, $"Invalid opcode '{token.Mnemonic}'");
			return (Opcode)token.Value.Value;
		}
		
		static Register ParseRegister(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			Expect(token, TokenType.Register, $"Invalid register '{token.Mnemonic}'");
			return (Register)token.Value.Value;
		}
		
		static AssemblyNode.Constant ParseConstant(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			
			if (Match(token, TokenType.Immediate)) {
				return new AssemblyNode.Immediate(token.Value.Value);
			}
			
			if (Match(token, TokenType.Identifier)) {
				return new AssemblyNode.Symbol(token.Mnemonic);
			}
			
			Fail(token, TokenType.Immediate, $"Invalid immediate '{token.Mnemonic}'");
			return null;
		}
		
		static AssemblyNode.Label ParseLabel(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			Expect(token, TokenType.Identifier, $"Invalid identifier '{token.Mnemonic}'");
			Expect(tokens.Dequeue(), TokenType.LabelDelimeter, $"Expected '{Syntax.label_delimeter}' after label '{token.Mnemonic}'");
			return new AssemblyNode.Label(token.Mnemonic);
		}
		
		// TODO: This is garbage
		// Maybe have PseudoInstruction node that gets pushed onto assembler tree and resolved later?
		static void ParsePseudoInstruction(LinkedList<Token> tokens)
		{
			Token pseudo_op = tokens.Dequeue();
			Expect(pseudo_op, TokenType.PseudoOp, $"Invalid opcode '{pseudo_op.Mnemonic}'");

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
	}
}
