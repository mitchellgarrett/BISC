using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static class Preprocessor {
		
		// Convert expressions into immediate values
		static Dictionary<string, AssemblyNode.ConstantDefinition> constants = new Dictionary<string, AssemblyNode.ConstantDefinition>();
		public static void EvaluateConstants(AssemblyTree program) {
			constants.Clear();
			
			// First pass to define symbols
			foreach (var section in program.Sections) {
				foreach (var item in section.Body) {
					if (item is AssemblyNode.ConstantDefinition symbol) {
						if (constants.ContainsKey(symbol.Identifier)) throw new SyntaxErrorException($"Duplicate symbol '{symbol.Identifier}'");
						constants.Add(symbol.Identifier, symbol);
					}
				}
			}
			
			// Second pass to resolve symbols
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count; i++) {
					if (section.Body[i] is AssemblyNode.IInstruction iinstruction) {
						// Replace immediate value of instruction with the address of the label
						section.Body[i] = new AssemblyNode.IInstruction(iinstruction.Opcode, iinstruction.Destination, EvaluateConstant(iinstruction.Immediate));
					}
				}
			}
			
			static AssemblyNode.Constant EvaluateConstant(AssemblyNode.Constant constant) {
				if (constant is AssemblyNode.LinkerRelocation relocation) return new AssemblyNode.LinkerRelocation(EvaluateConstant(relocation.Value), relocation.Type);
				
				if (constant is AssemblyNode.MacroAccess symbol_access) {
					if (!constants.TryGetValue(symbol_access.Identifier, out AssemblyNode.ConstantDefinition symbol)) 
						throw new SyntaxErrorException($"Unknown symbol '{symbol_access.Identifier}'");
					
					return EvaluateConstant(symbol.Value);
				}
				
				return constant;
			}
		}
	}
}