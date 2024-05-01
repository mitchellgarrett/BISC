using System;

namespace FTG.Studios.BISC.Asm
{

	public abstract class Instruction : Assembloid
	{

		public Opcode Opcode { get; protected set; }

		public Instruction(Opcode opcode)
		{
			Opcode = opcode;
			Size = 4;
		}
	}
}