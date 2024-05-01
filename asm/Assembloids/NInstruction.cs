using System;

namespace FTG.Studios.BISC.Asm
{

	public class NInstruction : Instruction
	{

		public NInstruction(Opcode opcode) : base(opcode)
		{
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			return Specification.DisassembleInteger32((UInt32)Opcode);
		}

		public override string ToString()
		{
			return Opcode.ToString();
		}
	}
}