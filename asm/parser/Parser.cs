using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {

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
			
			// Add newline to end of program
			tokens.AddLast(new Token(TokenType.LineSeperator, 0, 0));
			
			while (tokens.Count > 0) {
				AssemblyNode.BlockItem item = ParseBlockItem(tokens);
				if (item != null) body.Add(item);
			}
			return new AssemblyNode.Program(body);
		}

		static AssemblyNode.BlockItem ParseBlockItem(LinkedList<Token> tokens) {
			switch (tokens.Peek().Type) {
				case TokenType.Opcode:
					return ParseInstruction(tokens);
				
				case TokenType.PseudoOp:
					ParsePseudoInstruction(tokens);
					return null;
				
				case TokenType.Identifier:
					return ParseLabel(tokens);
				
				// TODO" Make this part of ParseDirective
				case TokenType.DataInitializer:
					return ParseDataInitializer(tokens);
				
				case TokenType.DirectivePrefix:
					return ParseDirective(tokens);
				
				case TokenType.Comment:
				case TokenType.LineSeperator:
					tokens.Dequeue();
					return null;
				
				default:
					Fail(tokens.Peek(), TokenType.Opcode, $"Invalid opcode '{tokens.Peek().Mnemonic}'");
					return null;
			}
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
			
			// Check for relocation directives
			if (
				Match(token, TokenType.DirectivePrefix) && 
				tokens.Peek().Mnemonic == Syntax.directive_relocation_lo || 
				tokens.Peek().Mnemonic == Syntax.directive_relocation_hi
			) {
				return ParseLinkerRelocation(tokens);
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
	}
}
