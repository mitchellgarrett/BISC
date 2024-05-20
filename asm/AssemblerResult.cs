using System;
using System.Collections.Generic;
using System.Linq;

namespace FTG.Studios.BISC.Asm
{

	public class AssemblerResult
	{

		public readonly List<SectionData> SectionData;

		readonly Dictionary<string, BEEF.SectionFlag> default_section_flags = new Dictionary<string, BEEF.SectionFlag>() {
			{".text", BEEF.SectionFlag.Readable | BEEF.SectionFlag.Executable },
			{".data", BEEF.SectionFlag.Readable | BEEF.SectionFlag.Writable | BEEF.SectionFlag.InititializedData }
		};

		public int SizeInBytes { get; private set; }

		public AssemblerResult()
		{
			SectionData = new List<SectionData>();

			// Initialize with .text section
			SectionData.Add(new SectionData(".text"));
		}

		public void Add(AssemblerData data)
		{
			if (data is SectionDirective)
			{
				SectionDirective section = data as SectionDirective;
				if (section.Identifier == SectionData.Last().Identifier) return;

				SectionData.Add(new SectionData(section.Identifier));
				return;
			}

			SizeInBytes += data.Size;
			SectionData.Last().Add(data);
		}

		Label GetLabel(string identifer)
		{
			foreach (var section in SectionData)
			{
				foreach (var data in section.Data)
				{
					if (data is Label)
					{
						Label label = data as Label;
						if (label.Identifier == identifer) return label;
					}
				}
			}
			return null;
		}

		public void AssignAddresses()
		{
			UInt32 address = 0;
			foreach (var section in SectionData)
			{
				foreach (var data in section.Data)
				{
					data.Address = address;
					address += (UInt32)data.Size;
				}
			}
		}

		public void ResolveUndefinedSymboles()
		{
			foreach (var section in SectionData)
			{
				foreach (var data in section.Data)
				{
					if (!data.HasUndefinedSymbol) continue;
					if (!(data is Instruction)) continue;

					if (data is IInstruction)
					{
						IInstruction iinstruction = data as IInstruction;

						Token immediate = iinstruction.Immediate;
						if (immediate.Type == TokenType.Identifier && !immediate.Value.HasValue)
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
						if (offset.Type == TokenType.Identifier && !offset.Value.HasValue)
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
		}

		public byte[] ToBinary() {
			List<byte> data = new List<byte>(SizeInBytes);
			foreach (SectionData section in SectionData) {
				data.AddRange(section.Assemble());
            }
			return data.ToArray();
        }

		public BEEF.ObjectFile ToObjectFile()
		{
			int section_count = SectionData.Count;

			// Initialize object file header with standard values
			BEEF.ObjectFile obj = new BEEF.ObjectFile();
			obj.FileHeader = new BEEF.FileHeader()
			{
				MagicNumber = BEEF.FileHeader.MAGIC_NUMBER,
				Architecture = 0xb,
				Endianness = BEEF.Endianness.Little,
				EntryPoint = 0,
				SectionTableOffset = BEEF.FileHeader.SizeInBytes,
				SectionCount = (UInt16)section_count
			};

			obj.SectionHeaders = new BEEF.SectionHeader[section_count];
			obj.SectionData = new byte[section_count][];

			// Iterate over sections and build section header
			UInt32 section_offset = (UInt32)(BEEF.FileHeader.SizeInBytes + BEEF.SectionHeader.SizeInBytes * section_count);
			UInt32 section_address = 0;
			for (int section_index = 0; section_index < section_count; section_index++)
			{
				string section_name = SectionData[section_index].Identifier;
				byte[] section_data = SectionData[section_index].Assemble();

				BEEF.SectionFlag section_flags;
				if (!default_section_flags.TryGetValue(section_name, out section_flags)) section_flags = BEEF.SectionFlag.None;

				// Initialize section header
				BEEF.SectionHeader sheader = new BEEF.SectionHeader()
				{
					Type = BEEF.SectionType.Program,
					Flags = section_flags,
					Offset = section_offset,
					Address = section_address,
					Size = (UInt32)section_data.Length,
					Name = section_name
				};

				obj.SectionHeaders[section_index] = sheader;
				obj.SectionData[section_index] = section_data;

				section_offset += (UInt32)section_data.Length;
				section_address += (UInt32)section_data.Length;
			}

			return obj;
		}

		public override string ToString()
		{
			string output = string.Empty;
			foreach (var section in SectionData)
			{
				output += $"{section}\n";
				foreach (var data in section.Data)
				{
					output += $"{data}\n";
				}
				return output;
			}
			return output;
		}
	}
}