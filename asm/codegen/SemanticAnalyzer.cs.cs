using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static class SemanticAnalyzer {
		
		public static void AssignAddresses(AssemblyTree program) {
			UInt32 address = 0;
			foreach (var section in program.Sections) {
				foreach (var item in section.Body) {
					item.Address = address;
					address += (UInt32)item.Size;
				}
			}
		}
		
		// TODO: Resolve macros/constant definitions
		public static void ResolveLabels(AssemblyTree program) {
			Dictionary<string, AssemblyNode.Label> labels = new Dictionary<string, AssemblyNode.Label>();
			
			// First pass to define symbols
			foreach (var section in program.Sections) {
				foreach (var item in section.Body) {
					if (item is AssemblyNode.Label label) {
						if (labels.ContainsKey(label.Identifier)) throw new SyntaxErrorException($"Duplicate label '{label.Identifier}'");
						labels.Add(label.Identifier, label);
					}
				}
			}
			
			// Second pass to resolve symbols
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count; i++) {
					if (section.Body[i] is AssemblyNode.IInstruction iinstruction) {
						// Replace immediate value of instruction with the address of the label
						section.Body[i] = new AssemblyNode.IInstruction(iinstruction.Opcode, iinstruction.Destination, ResolveConstant(labels, iinstruction.Immediate));
					}
					
					// TODO: Fix memory oinstructions too
				}
			}
			
			static AssemblyNode.Constant ResolveConstant(Dictionary<string, AssemblyNode.Label> labels, AssemblyNode.Constant constant) {
				if (constant is AssemblyNode.LinkerRelocation relocation) return new AssemblyNode.LinkerRelocation(ResolveConstant(labels, relocation.Value), relocation.Type);
				
				if (constant is AssemblyNode.LabelAccess symbol_access) {
					if (!labels.TryGetValue(symbol_access.Identifier, out AssemblyNode.Label symbol)) 
						throw new SyntaxErrorException($"Unknown label '{symbol_access.Identifier}'");
					
					return new AssemblyNode.Immediate(symbol.Address);
				}
				
				return constant;
			}
		}
	}
}