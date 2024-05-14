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
			return ((UInt32)Opcode).DisassembleUInt32();
		}

		public override string ToString()
		{
			return Opcode.ToString();
		}
	}
}