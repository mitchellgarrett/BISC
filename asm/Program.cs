using System;
using System.Collections;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public class Program
	{

		readonly List<Assembloid> assembloids;

		public int SizeInBytes { get; private set; }

		public Program()
		{
			assembloids = new List<Assembloid>();
		}

		public void Add(Assembloid assembloid)
		{
			SizeInBytes += assembloid.Size;
			assembloids.Add(assembloid);
		}

		public Label GetLabel(string identifer)
		{
			foreach (Assembloid assembloid in assembloids)
			{
				if (assembloid is Label)
				{
					Label label = assembloid as Label;
					if (label.Identifier == identifer) return label;
				}
			}
			return null;
		}

		public void AssignAddresses()
		{
			UInt32 address = 0;
			foreach (Assembloid assembloid in assembloids)
			{
				assembloid.Address = address;
				address += (UInt32)assembloid.Size;
			}
		}

		public void ResolveUndefinedSymboles()
		{
			foreach (Assembloid assembloid in assembloids)
			{
				if (!assembloid.HasUndefinedSymbol) continue;
				if (!(assembloid is Instruction)) continue;

				if (assembloid is IInstruction)
				{
					IInstruction iinstruction = assembloid as IInstruction;

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

				if (assembloid is MInstruction)
				{
					MInstruction minstruction = assembloid as MInstruction;

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
			foreach (Assembloid assembloid in assembloids)
			{
				machine_code.AddRange(assembloid.Assemble());
			}

			if (machine_code.Count != SizeInBytes) throw new ArgumentException($"Machine code length: {machine_code.Count}, expected length: {SizeInBytes}");

			return machine_code.ToArray();
		}

		public override string ToString()
		{
			string output = string.Empty;
			foreach (Assembloid assembloid in assembloids)
			{
				output += $"{assembloid}\n";
			}
			return output;
		}
	}
}