using System;

namespace FTG.Studios.BISC.Asm
{

	public abstract class AssemblerData
	{
		public UInt32 Address;
		public int Size { get; protected set; }
		public bool HasUndefinedSymbol { get; protected set; }

		public abstract byte[] Assemble();
	}
}