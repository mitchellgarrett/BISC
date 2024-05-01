using System;

namespace FTG.Studios.BISC.Asm
{

	public class TInstruction : Instruction
	{

		public readonly Token DestinationRegister;
		public readonly Token SourceRegisterA;
		public readonly Token SourceRegisterB;

		public TInstruction(Opcode opcode, Token destination_register, Token source_register_a, Token source_register_b) : base(opcode)
		{
			DestinationRegister = destination_register;
			SourceRegisterA = source_register_a;
			SourceRegisterB = source_register_b;
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			UInt32 machine_code = (UInt32)Opcode;

			machine_code |= DestinationRegister.Value.Value << 8;
			machine_code |= SourceRegisterA.Value.Value << 16;
			machine_code |= SourceRegisterB.Value.Value << 24;

			return Specification.DisassembleInteger32(machine_code);
		}

		public override string ToString()
		{
			string value = $"{Opcode}";
			value += $" {DestinationRegister.Mnemonic} ({DestinationRegister.Type}, 0x{DestinationRegister.Value:x2})";
			value += $", {SourceRegisterA.Mnemonic} ({SourceRegisterA.Type}, 0x{SourceRegisterA.Value:x2})";
			value += $", {SourceRegisterB.Mnemonic} ({SourceRegisterB.Type}, 0x{SourceRegisterB.Value:x2})";
			return value;
		}
	}
}