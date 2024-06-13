using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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
		
		static Dictionary<string, AssemblyNode.Label> labels = new Dictionary<string, AssemblyNode.Label>();
		public static void ResolveSymbols(AssemblyTree program) {
			labels.Clear();
			
			// First pass to define symbols
			foreach (var section in program.Sections) {
				foreach (var item in section.Body) {
					if (item is AssemblyNode.Label label) {
						if (labels.ContainsKey(label.Identifier)) throw new SyntaxErrorException($"Duplicate symbol '{label.Identifier}'");
						labels.Add(label.Identifier, label);
					}
				}
			}
			
			// Second pass to resolve symbols
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count; i++) {
					if (section.Body[i] is AssemblyNode.IInstruction iinstruction) {
						// Replace immediate value of instruction with the address of the label
						section.Body[i] = new AssemblyNode.IInstruction(iinstruction.Opcode, iinstruction.Destination, ResolveConstant(iinstruction.Immediate));
					}
				}
			}
			
			static AssemblyNode.Constant ResolveConstant(AssemblyNode.Constant constant) {
				if (constant is AssemblyNode.LinkerRelocation relocation) return new AssemblyNode.LinkerRelocation(ResolveConstant(relocation.Value), relocation.Type);
				
				if (constant is AssemblyNode.Symbol symbol) {
					if (!labels.TryGetValue(symbol.Identifier, out AssemblyNode.Label label)) 
						throw new SyntaxErrorException($"Unknown symbol '{label.Identifier}'");
					
					return new AssemblyNode.Immediate(label.Address);
				}
				
				return constant;
			}
		}
	}
}