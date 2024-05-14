using System;

namespace FTG.Studios.BISC.Asm
{

	public class RInstruction : Instruction
	{

		public readonly Token Register;

		public RInstruction(Opcode opcode, Token register) : base(opcode)
		{
			Register = register;
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			UInt32 machine_code = (UInt32)Opcode;
			machine_code |= Register.Value.Value << 8;
			return machine_code.DisassembleUInt32();
		}

		public override string ToString()
		{
			return $"{Opcode} {Register.Mnemonic} ({Register.Type}, 0x{Register.Value:x2})";
		}
	}
}