using System;

namespace FTG.Studios.BISC.Asm
{

	public abstract class Assembloid
	{
		public UInt32 Address;

		public abstract bool HasUndefinedSymbol();
	}
}