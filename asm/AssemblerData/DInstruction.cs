using System;

namespace FTG.Studios.BISC.Asm
{

	public class DInstruction : Instruction
	{

		public readonly Token DestinationRegister;
		public readonly Token SourceRegister;

		public DInstruction(Opcode opcode, Token destination_register, Token source_register) : base(opcode)
		{
			DestinationRegister = destination_register;
			SourceRegister = source_register;
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			UInt32 machine_code = (UInt32)Opcode;

			machine_code |= DestinationRegister.Value.Value << 8;
			machine_code |= SourceRegister.Value.Value << 16;

			return machine_code.DisassembleUInt32();
		}

		public override string ToString()
		{
			string value = $"{Opcode}";
			value += $" {DestinationRegister.Mnemonic} ({DestinationRegister.Type}, 0x{DestinationRegister.Value:x2})";
			value += $", {SourceRegister.Mnemonic} ({SourceRegister.Type}, 0x{SourceRegister.Value:x2})";
			return value;
		}
	}
}