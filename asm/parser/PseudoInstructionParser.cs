using System;
using System.Collections.Generic;
using System.Linq;

namespace FTG.Studios.BISC.Asm {
	
	// TODO: Maybe make this a preprocessor step in the future
	public static partial class Parser {
		
		static void ParsePseudoInstruction(LinkedList<Token> tokens) {
			Token pseudo_opcode = tokens.Dequeue();
			Expect(pseudo_opcode, TokenType.PseudoOp, $"Invalid opcode '{pseudo_opcode}'");
			
			// Grab all arguments until end of line
			List<Token> arguments = new List<Token>();
			while (!Match(tokens.Peek(), TokenType.LineSeperator)) {
				Token token = tokens.Dequeue();
				if (Match(token, TokenType.Comment)) continue;
				arguments.Add(token);
			}
			// Dequeue line seperator
			tokens.Dequeue();
			
			int pseudo_index;
			List<Token> pseudo_operands = null;
			for (pseudo_index = (int)pseudo_opcode.Value; pseudo_index < Specification.pseudo_instruction_names.Length; pseudo_index++) {
				if (pseudo_opcode.Mnemonic.ToUpper() != Specification.pseudo_instruction_names[pseudo_index]) continue;
				ArgumentType[] argument_types = Specification.pseudo_instruction_arguments[pseudo_index];
				
				// If the arguments match the tokens, then we have found our pseudo instruction and we can break out of the loop
				if (TryParsePseudoInstruction(argument_types, new LinkedList<Token>(arguments), out pseudo_operands)) break;
			}
			
			// If a pseudo instruction match doesn't exist, it might be a regular instruction
			if (pseudo_index >= Specification.pseudo_instruction_names.Length) {
				Opcode? opcode = Syntax.GetOpcode(pseudo_opcode.Mnemonic.ToUpper());
				if (!opcode.HasValue) Fail(pseudo_opcode, TokenType.Opcode, $"Invalid opcode '{pseudo_opcode.Mnemonic}'");
				
				pseudo_opcode.Type = TokenType.Opcode;
				pseudo_opcode.Value = (UInt32)opcode.Value;
				
				// The pseudo instruction has a valid opcode, so push the original tokens onto the queue
				tokens.AddFirst(new Token(TokenType.LineSeperator, pseudo_opcode.LineNo, 0));
				for (int i = arguments.Count - 1; i >= 0; i--) tokens.AddFirst(arguments[i]);
				tokens.AddFirst(pseudo_opcode);
				return;
			}
			
			// Get list of instructions that the pseudo instruction expands to
			string[] pseudo_instruction_definition = Specification.pseudo_instruction_definitions[pseudo_index];
			
			// Replace each instruction in the pseudo instruction expansion which its proper arguments
			for (int i = 0; i < pseudo_instruction_definition.Length; i++) {
				// Format operands into the instruction definition
				string current_instruction = string.Format(pseudo_instruction_definition[i], pseudo_operands.Select(o => o.Mnemonic).ToArray());
				current_instruction += '\n';
				
				// Parse formatted instruction and push its tokens onto the queue
				List<Token> pseudo_instruction_tokens = Lexer.Tokenize(current_instruction);
				for (int v = pseudo_instruction_tokens.Count - 1; v >= 0; v--) {
					Token token = pseudo_instruction_tokens[v];
					token.LineNo = pseudo_opcode.LineNo;
					tokens.AddFirst(token);
				}
			}
		}
		
		static bool TryParsePseudoInstruction(ArgumentType[] argument_types, LinkedList<Token> tokens, out List<Token> operands) {
			operands = new List<Token>(argument_types.Length);
			for (int index = 0; index < argument_types.Length; index++) {
				Token register_token, immediate_token;
				switch (argument_types[index]) {
					case ArgumentType.None: 
						if (tokens.Count > 0) return false;
						break;
						
					case ArgumentType.Register:
						if (!TryParseRegister(tokens, out register_token)) return false;
						operands.Add(register_token);
						break;
						
					case ArgumentType.Immediate32:
						if (!TryParseImmediate32(tokens, out immediate_token)) return false;
						operands.Add(immediate_token);
						break;
					
					// TODO: Implement memory accesses in pseudo instructions
					case ArgumentType.Memory:
						if (!TryParseMemory(tokens, out register_token, out immediate_token)) return false;
						operands.Add(register_token);
						operands.Add(immediate_token);
						break;
				}
				
				// Arguments must be separated by a ','
				if (index < argument_types.Length - 1) {
					if (tokens.Count < 1) return false;
					if (!Match(tokens.Dequeue(), TokenType.Seperator)) return false;
				}
			}
			
			return true;
		}
		
		static bool TryParseRegister(LinkedList<Token> tokens, out Token register) {
			register = new Token(TokenType.Invalid, 0, 0);
			if (tokens.Count < 1) return false;
			
			register = tokens.Dequeue();
			if (!Match(register, TokenType.Register)) return false;
			
			return true;
		}
		
		static bool TryParseImmediate32(LinkedList<Token> tokens, out Token immediate) {
			immediate = new Token(TokenType.Invalid, 0, 0);
			if (tokens.Count < 1) return false;
			
			immediate = tokens.Dequeue();
			if (!Match(immediate, TokenType.Immediate) && !Match(immediate, TokenType.Identifier)) return false;
			
			return true;
		}
		
		static bool TryParseMemory(LinkedList<Token> tokens, out Token register, out Token offset) {
			register = offset = new Token(TokenType.Invalid, 0, 0);
			if (tokens.Count < 4) return false;
			
			register = tokens.Dequeue();
			if (!Match(register, TokenType.Register)) return false;
			
			if (!Match(tokens.Dequeue(), TokenType.OpenBracket)) return false;
			
			offset = tokens.Dequeue();
			if (!Match(offset, TokenType.Immediate) && !Match(offset, TokenType.Identifier)) return false;
			
			if (!Match(tokens.Dequeue(), TokenType.CloseBracket)) return false;
			
			return true;
		}
	}
}