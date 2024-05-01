using System;

namespace FTG.Studios.BISC.Asm
{

	public class MInstruction : Instruction
	{

		public readonly Token DestinationRegister;
		public readonly Token SourceRegister;
		public Token Offset;

		public MInstruction(Opcode opcode, Token destination_register, Token source_register, Token immediate) : base(opcode)
		{
			DestinationRegister = destination_register;
			SourceRegister = source_register;
			Offset = immediate;

			// Has undefined symbol if the given immediate value is a label that has not yet been defined
			HasUndefinedSymbol = immediate.Type == TokenType.Label && !immediate.Value.HasValue;
		}

		public override byte[] Assemble()
		{
			UInt32 machine_code = (UInt32)Opcode;

			machine_code |= DestinationRegister.Value.Value << 8;
			machine_code |= SourceRegister.Value.Value << 16;
			machine_code |= (Offset.Value.Value & 0xFF) << 24;

			return Specification.DisassembleInteger32(machine_code);
		}

		public override string ToString()
		{
			string value = $"{Opcode}";
			value += $" {DestinationRegister.Mnemonic} ({DestinationRegister.Type}, 0x{DestinationRegister.Value:x2})";
			value += $", {SourceRegister.Mnemonic} ({SourceRegister.Type}, 0x{SourceRegister.Value:x2})";
			value += $", {Offset.Mnemonic} ({Offset.Type}, 0x{Offset.Value:x2})";
			return value;
		}
	}
}