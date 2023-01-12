namespace FTG.Studios.BISC.Assembler {
	
	public static class Optimizer {
		
		public static void Optimize(Program program) {
			for (int i = 0; i < program.Instructions.Count; i++) {
				Instruction inst = program.Instructions[i];
				if (inst.Opcode == Opcode.LUI && inst.Parameters[1].Value == 0) {
					program.Instructions.RemoveAt(i--);
				}
			}
		}
	}
}