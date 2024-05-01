using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public class AssemblerResult
	{

		public readonly List<AssemblerData> Data;

		public int SizeInBytes { get; private set; }

		public AssemblerResult()
		{
			Data = new List<AssemblerData>();
		}

		public void Add(AssemblerData data)
		{
			SizeInBytes += data.Size;
			Data.Add(data);
		}

		public void RemoveAt(int index)
		{
			SizeInBytes -= Data[index].Size;
			Data.RemoveAt(index);
		}

		Label GetLabel(string identifer)
		{
			foreach (AssemblerData data in Data)
			{
				if (data is Label)
				{
					Label label = data as Label;
					if (label.Identifier == identifer) return label;
				}
			}
			return null;
		}

		public void AssignAddresses()
		{
			UInt32 address = 0;
			foreach (AssemblerData data in Data)
			{
				data.Address = address;
				address += (UInt32)data.Size;
			}
		}

		public void ResolveUndefinedSymboles()
		{
			foreach (AssemblerData data in Data)
			{
				if (!data.HasUndefinedSymbol) continue;
				if (!(data is Instruction)) continue;

				if (data is IInstruction)
				{
					IInstruction iinstruction = data as IInstruction;

					Token immediate = iinstruction.Immediate;
					if (immediate.Type == TokenType.Label && !immediate.Value.HasValue)
					{
						Label label = GetLabel(immediate.Mnemonic);

						if (label == null) throw new ArgumentException($"(Ln: {immediate.LineNo}, Ch: {immediate.CharNo}) Undefined symbol: '{immediate.Mnemonic}'\n'{iinstruction}'");

						immediate.Value = label.Address;
						immediate.Type = TokenType.Immediate;

						iinstruction.Immediate = immediate;
					}
				}

				if (data is MInstruction)
				{
					MInstruction minstruction = data as MInstruction;

					Token offset = minstruction.Offset;
					if (offset.Type == TokenType.Label && !offset.Value.HasValue)
					{
						Label label = GetLabel(offset.Mnemonic);

						if (label == null) throw new ArgumentException($"(Ln: {offset.LineNo}, Ch: {offset.CharNo}) Undefined symbol: '{offset.Mnemonic}'\n'{minstruction}'");

						offset.Value = label.Address;
						offset.Type = TokenType.Immediate;

						minstruction.Offset = offset;
					}
				}
			}
		}

		public byte[] Assemble()
		{
			List<byte> machine_code = new List<byte>();
			foreach (AssemblerData data in Data)
			{
				machine_code.AddRange(data.Assemble());
			}

			if (machine_code.Count != SizeInBytes) throw new ArgumentException($"Machine code length: {machine_code.Count}, expected length: {SizeInBytes}");

			return machine_code.ToArray();
		}

		public override string ToString()
		{
			string output = string.Empty;
			foreach (AssemblerData data in Data)
			{
				output += $"{data}\n";
			}
			return output;
		}
	}
}