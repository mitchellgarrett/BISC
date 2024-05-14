using System;

namespace FTG.Studios.BISC.Asm
{

	public class IInstruction : Instruction
	{

		public readonly Token Register;
		public Token Immediate;

		public IInstruction(Opcode opcode, Token register, Token immediate) : base(opcode)
		{
			Register = register;
			Immediate = immediate;

			// Has undefined symbol if the given immediate value is a label that has not yet been defined
			HasUndefinedSymbol = immediate.Type == TokenType.Identifier && !immediate.Value.HasValue;
		}

		public override byte[] Assemble()
		{
			UInt32 machine_code = (UInt32)Opcode;

			machine_code |= Register.Value.Value << 8;

			// TODO: Make this case not an absolute hack; this code sucks mega nuts
			// If opcode is LUI, load top 16 bits
			if (Opcode == Opcode.LUI) machine_code |= ((Immediate.Value.Value >> 16) & 0xFFFF) << 16;
			else machine_code |= (Immediate.Value.Value & 0xFFFF) << 16;

			return machine_code.DisassembleUInt32();
		}

		public override string ToString()
		{
			string value = $"{Opcode}";
			value += $" {Register.Mnemonic} ({Register.Type}, 0x{Register.Value:x2})";
			value += $", {Immediate.Mnemonic} ({Immediate.Type}, 0x{Immediate.Value:x4})";
			return value;
		}
	}
}