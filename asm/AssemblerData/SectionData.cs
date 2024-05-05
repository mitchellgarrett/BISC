using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public class SectionData
	{

		public string Identifier;
		public UInt32 Address;

		public readonly List<AssemblerData> Data;

		public SectionData(string identifer)
		{
			this.Identifier = identifer;
			Data = new List<AssemblerData>();
		}

		public void Add(AssemblerData data)
		{
			Data.Add(data);
		}

		public byte[] Assemble()
		{
			List<byte> binary = new List<byte>();
			foreach (var data in Data)
			{
				binary.AddRange(data.Assemble());
			}
			return binary.ToArray();
		}
	}
}