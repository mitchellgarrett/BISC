using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static class Preprocessor {
		
		// TODO: Should this be different than label resolution?
		// Convert expressions into immediate values
		public static void EvaluateMacros(AssemblyTree program) {
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
		}
	}
}