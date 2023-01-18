namespace FTG.Studios.BISC.Assembler {
	
	public static class Optimizer {
		
		public static void Optimize(Program program) {
			for (int i = 0; i < program.Instructions.Count; i++) {
				Instruction inst = program.Instructions[i];
				if (i < program.Instructions.Count - 1) {
					Instruction next = program.Instructions[i + 1];
					// If current inst is LLI, next inst is LUI, both have same dest reg, and next inst loads 0
					// then the LUI {reg}, 0 operation is redundant as LLI already zeros out the upper 16 bits
					if (inst.Opcode == Opcode.LLI && next.Opcode == Opcode.LUI && inst.Parameters[0].Value == next.Parameters[0].Value && next.Parameters[1].Value == 0) {
						program.Instructions.RemoveAt(i + 1);
					}
				}
			}
		}
	}
}