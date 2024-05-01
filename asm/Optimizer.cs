namespace FTG.Studios.BISC.Asm
{

	public static class Optimizer
	{

		public static void Optimize(AssemblerResult program)
		{
			for (int i = 0; i < program.Data.Count; i++)
			{
				if (!(program.Data[i] is IInstruction current_iinstruction)) continue;

				if (i < program.Data.Count - 1)
				{
					if (!(program.Data[i + 1] is IInstruction next_iinstruction)) continue;


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