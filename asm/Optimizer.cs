namespace FTG.Studios.BISC.Asm {
	
	public static class Optimizer {
		
		public static void Optimize(Program program) {
			for (int i = 0; i < program.Instructions.Count; i++) {
				Instruction inst = program.Instructions[i];
				if (i < program.Instructions.Count - 1) {
					Instruction next = program.Instructions[i + 1];
					// If current inst is LLI, next inst is LUI, both have same dest reg, and next inst loads 0
					// then the LUI {reg}, 0 operation is redundant as LLI already zeros out the upper 16 bits
					if (inst.Opcode == Opcode.LLI && next.Opcode == Opcode.LUI && inst.Parameters[0].Value == next.Parameters[0].Value) {
						// Check if upper 16 bits are 0 cause that's how LUI is currently implemented, which is dumb
						if (next.Parameters[1].Value.HasValue && (next.Parameters[1].Value.Value >> 16) == 0)
							program.Instructions.RemoveAt(i + 1);
					}
				}
			}
		}
	}
}