namespace FTG.Studios.BISC.Asm
{

	public static class Optimizer
	{

		public static void Optimize(AssemblerResult program)
		{
			for (int i = 0; i < program.Assembloids.Count; i++)
			{
				if (!(program.Assembloids[i] is IInstruction current_iinstruction)) continue;

				if (i < program.Assembloids.Count - 1)
				{
					if (!(program.Assembloids[i + 1] is IInstruction next_iinstruction)) continue;


					// If current inst is LLI, next inst is LUI, both have same dest reg, and next inst loads 0
					// then the LUI {reg}, 0 operation is redundant as LLI already zeros out the upper 16 bits
					if (current_iinstruction.Opcode == Opcode.LLI && next_iinstruction.Opcode == Opcode.LUI && current_iinstruction.Immediate.Value == next_iinstruction.Immediate.Value)
					{
						// Check if upper 16 bits are 0 cause that's how LUI is currently implemented, which is dumb
						if (next_iinstruction.Immediate.Value.HasValue && (next_iinstruction.Immediate.Value.Value >> 16) == 0)
							program.RemoveAt(i + 1);
					}
				}
			}
		}
	}
}