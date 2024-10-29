namespace FTG.Studios.BISC.Asm {

	public static class Optimizer {
		
		/// <summary>
		/// Removes LUI instructions that reduandly load 0.
		/// </summary>
		/// <param name="program">The program to optimize.</param>
		public static void RemoveRedundantLoads(AssemblyTree program) {
			foreach (var section in program.Sections) {
				for (int i = 0; i < section.Body.Count - 1; i++) {
					if (!(section.Body[i] is AssemblyNode.IInstruction current)) continue;
					if (!(section.Body[i + 1] is AssemblyNode.IInstruction next)) continue;
					
					// If the current/next instruction form a LLI/LUI pair that load to the same register
					// and the LUI loads 0, then it is redundant and can be removed
					if (current.Opcode != Opcode.LLI || next.Opcode != Opcode.LUI) continue;
					if (current.Destination.Type != next.Destination.Type) continue;
					// TODO: Evaluate value
				}
			}
		}
	}
}