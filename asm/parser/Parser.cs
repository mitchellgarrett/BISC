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
			
			List<AssemblyNode.Section> sections = ParseProgram(stream);
			return new AssemblyTree(sections);
		}
		
		static List<AssemblyNode.Section> ParseProgram(LinkedList<Token> tokens) {
			// Add newline to end of program
			tokens.AddLast(new Token(TokenType.LineSeperator, 0, 0));
			
			List<AssemblyNode.Section> sections = new List<AssemblyNode.Section>();
			
			// Default to .text section
			AssemblyNode.SectionDefinition previous_section_definition = new AssemblyNode.SectionDefinition(".text");
			List<AssemblyNode.BlockItem> current_section_data = new List<AssemblyNode.BlockItem>();
			
			while (tokens.Count > 0) {
				AssemblyNode.BlockItem item = ParseBlockItem(tokens);
				
				// If we encounter another section definition, then dump the current_section_data contents into a new Section object and start over
				if (item is AssemblyNode.SectionDefinition current_section_definition) {
					if (current_section_data.Count > 0)
						sections.Add(new AssemblyNode.Section(previous_section_definition.Identifier, current_section_data));
					
					previous_section_definition = current_section_definition;
					current_section_data = new List<AssemblyNode.BlockItem>();
				} else if (item != null) {
					current_section_data.Add(item);
				}
			}
			
			// Add trailing section
			if (current_section_data.Count > 0)
				sections.Add(new AssemblyNode.Section(previous_section_definition.Identifier, current_section_data));
		
			return sections;
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
