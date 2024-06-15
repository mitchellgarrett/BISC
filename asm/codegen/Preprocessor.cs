using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static class Preprocessor {
		
		public static void RemoveComments(List<Token> tokens) {
			for (int i = 0; i < tokens.Count; i++) if (tokens[i].Type == TokenType.Comment) tokens.RemoveAt(i--);
		}
		
		public static void ProcessMacros(List<Token> tokens) {
			Dictionary<string, List<Token>> constant_definitions = new Dictionary<string, List<Token>>();
			Dictionary<string, (List<string>, List<Token>)> macro_definitions = new Dictionary<string, (List<string>, List<Token>)>();
			
			// First pass to parse macro definitions
			for (int i = 0; i < tokens.Count; i++) {
				if (tokens[i].Type != TokenType.DirectivePrefix) continue;
				if (tokens[i + 1].Type != TokenType.Identifier) continue;
				
				if (tokens[i + 1].Mnemonic == Syntax.directive_define) {
					if (TryParseConstantDefinition(tokens, i, out string identifier, out List<Token> definition)) {
						if (constant_definitions.ContainsKey(identifier) || macro_definitions.ContainsKey(identifier)) throw new SyntaxErrorException($"TODO: duplicate symbol {identifier}");
						
						constant_definitions.Add(identifier, definition);
					}
				}
				
				if (tokens[i + 1].Mnemonic == Syntax.directive_macro) {
					if (TryParseMacroDefinition(tokens, i, out string identifier, out List<string> parameters, out List<Token> definition)) {
						if (constant_definitions.ContainsKey(identifier) || macro_definitions.ContainsKey(identifier)) throw new SyntaxErrorException($"TODO: duplicate symbol {identifier}");
						
						macro_definitions.Add(identifier, (parameters, definition));
					}
				}
			}
			
			// Second pass to resolve symbols
			for (int i = 0; i < tokens.Count; i++) {
				if (tokens[i].Type != TokenType.MacroExpansionOperator) continue;
				if (tokens[i + 1].Type != TokenType.Identifier) continue;
				
				ResolveMacro(constant_definitions, macro_definitions, tokens, i);
			}
		}
		
		static void ResolveMacro(Dictionary<string, List<Token>> constant_definitions, Dictionary<string, (List<string>, List<Token>)> macro_definitions, List<Token> tokens, int index) {
			if (tokens[index].Type != TokenType.MacroExpansionOperator) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			tokens.RemoveAt(index);
			
			if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			string identifier = tokens[index].Mnemonic;
			tokens.RemoveAt(index);
			
			if (constant_definitions.TryGetValue(identifier, out List<Token> constant_definition)) {
				// Insert definition into tokens list
				// Insert backwards to maintain order
				for (int i = constant_definition.Count - 1; i >= 0; i--) {
					tokens.Insert(index, constant_definition[i]);
				}
				return;
			}
			
			if (macro_definitions.TryGetValue(identifier, out (List<string>, List<Token>) macro_values)) {
				(List<string> macro_parameters, List<Token> macro_definition) = macro_values;
				
				// Parse arguments
				Dictionary<string, List<Token>> arguments = new Dictionary<string, List<Token>>();
				for (int i = 0; i < macro_parameters.Count; i++) {
					List<Token> argument = new List<Token>();
					// TODO: This will fail if it encounters another comma that's not supposed to be the separator
					while (tokens[index].Type != TokenType.Seperator && tokens[index].Type != TokenType.LineSeperator) {
						argument.Add(tokens[index]);
						tokens.RemoveAt(index);
					}
					arguments.Add(macro_parameters[i], argument);
				}
				
				// Replace parameters with given values in macro definition
				macro_definition = new List<Token>(macro_definition);
				for (int i = 0; i < macro_definition.Count - 1; i++) {
					if (macro_definition[i].Type == TokenType.MacroExpansionOperator && macro_definition[i + 1].Type == TokenType.Identifier) {
						if (!arguments.TryGetValue(macro_definition[i + 1].Mnemonic, out List<Token> argument)) continue;
						
						// Remove '$' and identifier
						macro_definition.RemoveAt(i);
						macro_definition.RemoveAt(i);
						
						// Replace identifier with argumen tokens
						for (int a = argument.Count - 1; a >= 0; a--) {
							macro_definition.Insert(i, argument[a]);
						}
						
						i = i - 1 + argument.Count - 1;
					}
				}
				
				// Insert definition into tokens list
				// Insert backwards to maintain order
				for (int i = macro_definition.Count - 1; i >= 0; i--) {
					tokens.Insert(index, macro_definition[i]);
				}
				return;
			}
			
			throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
		}
		
		static bool TryParseConstantDefinition(List<Token> tokens, int index, out string identifier, out List<Token> definition) {
			identifier = null;
			definition = null;
			
			if (tokens[index].Type != TokenType.DirectivePrefix) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			tokens.RemoveAt(index);
			
			if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			if (tokens[index].Mnemonic != Syntax.directive_define) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			tokens.RemoveAt(index);
			
			// Get identifier
			if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			identifier = tokens[index].Mnemonic;
			tokens.RemoveAt(index);
			
			// Loop until end of line to get replacement values
			definition = new List<Token>();
			while (tokens[index].Type != TokenType.LineSeperator) {
				definition.Add(tokens[index]);
				tokens.RemoveAt(index);
			}
			
			// Remove line feed
			tokens.RemoveAt(index);
			
			return true;
		}
		
		static bool TryParseMacroDefinition(List<Token> tokens, int index, out string identifier, out List<string> parameters, out List<Token> definition) {
			identifier = null;
			parameters = null;
			definition = null;
			
			if (tokens[index].Type != TokenType.DirectivePrefix) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			tokens.RemoveAt(index);
			
			if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			if (tokens[index].Mnemonic != Syntax.directive_macro) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			tokens.RemoveAt(index);
			
			// Get identifier
			if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
			identifier = tokens[index].Mnemonic;
			tokens.RemoveAt(index);
			
			// Loop until end of line to get parameters
			parameters = new List<string>();
			while (tokens[index].Type != TokenType.LineSeperator) {
				if (tokens[index].Type != TokenType.Identifier) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
				
				parameters.Add(tokens[index].Mnemonic);
				tokens.RemoveAt(index);
			
				// Parameters should be comma-separated
				if (tokens[index].Type != TokenType.LineSeperator) {
					if (tokens[index].Type != TokenType.Seperator) throw new SyntaxErrorException($"TODO: invalid macro {tokens[index]}");
					tokens.RemoveAt(index);
				}
			}
			
			// Remove line feed
			tokens.RemoveAt(index);

			// Add tokens to definition until we reach '%end'
			definition = new List<Token>();
			while (tokens[index].Type != TokenType.DirectivePrefix && tokens[index + 1].Mnemonic != Syntax.directive_end) {
				definition.Add(tokens[index]);
				tokens.RemoveAt(index);
			}
			
			// Remove '%end'
			tokens.RemoveAt(index);
			tokens.RemoveAt(index);
			
			return true;
		}
		
		// TODO: Make this a step in between lexer and parser?
		// TODO: Should this be different than label resolution?
		// Convert expressions into immediate values
		/*public static void EvaluateMacros(AssemblyTree program) {
			Dictionary<string, AssemblyNode.ConstantDefinition> constant_definitions = new Dictionary<string, AssemblyNode.ConstantDefinition>();
			Dictionary<string, AssemblyNode.MacroDefinition> macro_definitions = new Dictionary<string, AssemblyNode.MacroDefinition>();
			
			// First pass to define symbols
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count; i++) {
					if (section.Body[i] is AssemblyNode.ConstantDefinition constant) {
						section.Body.RemoveAt(i--);
						if (constant_definitions.ContainsKey(constant.Identifier) || macro_definitions.ContainsKey(constant.Identifier)) throw new SyntaxErrorException($"Duplicate symbol '{constant.Identifier}'");
						constant_definitions.Add(constant.Identifier, constant);
						continue;
					}
					
					if (section.Body[i] is AssemblyNode.MacroDefinition macro) {
						section.Body.RemoveAt(i--);
						if (constant_definitions.ContainsKey(macro.Identifier) || macro_definitions.ContainsKey(macro.Identifier)) throw new SyntaxErrorException($"Duplicate symbol '{macro.Identifier}'");
						macro_definitions.Add(macro.Identifier, macro);
						continue;
					}
				}
			}
			
			// Second pass to resolve symbols
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count; i++) {
					if (section.Body[i] is AssemblyNode.IInstruction iinstruction) {
						// Replace immediate value of instruction with the address of the label
						section.Body[i] = new AssemblyNode.IInstruction(iinstruction.Opcode, iinstruction.Destination, EvaluateConstant(constant_definitions, iinstruction.Immediate));
					}
					
					// TODO: Fix memory instructions too
					
					if (section.Body[i] is AssemblyNode.MacroAccess macro_access) {
						// Remove MacroAccess directive
						section.Body.RemoveAt(i);
						
						// Insert macro replacements
						section.Body.InsertRange(i, EvaluateMacro(macro_definitions, macro_access));
					}
				}
			}
			
			static AssemblyNode.Constant EvaluateConstant(Dictionary<string, AssemblyNode.ConstantDefinition> constant_definitions, AssemblyNode.Constant constant) {
				if (constant is AssemblyNode.LinkerRelocation relocation) return new AssemblyNode.LinkerRelocation(EvaluateConstant(constant_definitions, relocation.Value), relocation.Type);
				
				if (constant is AssemblyNode.DefineAccess symbol_access) {
					if (!constant_definitions.TryGetValue(symbol_access.Identifier, out AssemblyNode.ConstantDefinition symbol)) 
						throw new SyntaxErrorException($"Unknown symbol '{symbol_access.Identifier}'");
					
					return EvaluateConstant(constant_definitions, symbol.Value);
				}
				
				return constant;
			}
			
			// TODO: This is garbage
			static AssemblyNode.Operand EvaluateOperand(Dictionary<string, AssemblyNode.Operand> operand_definitions, AssemblyNode.Operand operand) {
				if (operand is AssemblyNode.LinkerRelocation relocation) return new AssemblyNode.LinkerRelocation((AssemblyNode.Constant)EvaluateOperand(operand_definitions, relocation.Value), relocation.Type);
				
				if (operand is AssemblyNode.DefineAccess symbol_access) {
					if (!operand_definitions.TryGetValue(symbol_access.Identifier, out AssemblyNode.Operand value)) 
						throw new SyntaxErrorException($"Unknown symbol '{symbol_access.Identifier}'");
					
					return EvaluateOperand(operand_definitions, value);
				}
				
				return operand;
			}
			
			static List<AssemblyNode.BlockItem> EvaluateMacro(Dictionary<string, AssemblyNode.MacroDefinition> macro_definitions, AssemblyNode.MacroAccess macro_access) {
				// TODO: Make this work with parameters
				if (!macro_definitions.TryGetValue(macro_access.Identifier, out AssemblyNode.MacroDefinition macro_definition)) 
						throw new SyntaxErrorException($"Unknown symbol '{macro_access.Identifier}'");
				
				// Generate argument dictionary
				if (macro_access.Arguments.Count != macro_definition.Parameters.Count)
					throw new SyntaxErrorException($"Macro '{macro_definition.Identifier}' expects {macro_definition.Parameters.Count} arguments but got {macro_access.Arguments.Count}");
				
				Dictionary<string, AssemblyNode.Operand> argument_values = new Dictionary<string, AssemblyNode.Operand>();
				for (int i = 0; i < macro_access.Arguments.Count; i++) {
					string identifier = macro_definition.Parameters[i];
					AssemblyNode.Operand value = macro_access.Arguments[i];
					
					if (argument_values.ContainsKey(identifier)) throw new SyntaxErrorException($"Duplicate argument '{identifier}' in macro '{macro_definition.Identifier}'");
					argument_values.Add(identifier, value);
				}
				
				// Create copy of body to fill with arguments
				List<AssemblyNode.BlockItem> body = new List<AssemblyNode.BlockItem>(macro_definition.Body);
				foreach (var item in body) {
					// TODO: fill with arguments
				}
					
				return macro_definition.Body;
			}
		}*/
	}
}