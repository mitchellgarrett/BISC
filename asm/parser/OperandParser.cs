using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class Parser {
		
		static Opcode ParseOpcode(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			Expect(token, TokenType.Opcode, $"Invalid opcode '{token.Mnemonic}'");
			return (Opcode)token.Value.Value;
		}
		
		static AssemblyNode.Register ParseRegister(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			Expect(token, TokenType.Register, $"Invalid register '{token.Mnemonic}'");
			return new AssemblyNode.Register((Register)token.Value.Value);
		}
		
		static AssemblyNode.Constant ParseConstant(LinkedList<Token> tokens) {
			Token token = tokens.Dequeue();
			
			if (Match(token, TokenType.Immediate)) {
				return new AssemblyNode.Immediate(token.Value.Value);
			}
			
			if (Match(token, TokenType.Identifier)) {
				return new AssemblyNode.LabelAccess(token.Mnemonic);
			}
			
			// Check for macros
			if (Match(token, TokenType.MacroExpansionOperator)) {
				token = tokens.Dequeue();
				Expect(token, TokenType.Identifier, "TODO: Invalid macro");
				return new AssemblyNode.DefineAccess(token.Mnemonic);
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