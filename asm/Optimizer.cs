using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC {
	
	public static class Optimizer {
		
		public static void Optimize(List<Instruction> instructions) {
			for (int i = 0; i < instructions.Count; i++) {
				Instruction inst = instructions[i];
				if (inst.Opcode == Opcode.LUI && inst.Parameters[1].Value == 0) {
					instructions.RemoveAt(i--);
				}
			}
		}
	}
}