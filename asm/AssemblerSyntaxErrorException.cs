using System;

namespace FTG.Studios.BISC.Asm
{

	public class AssemblerSyntaxErrorException : Exception
	{

		public AssemblerSyntaxErrorException(string message) : base(message) { }
	}
}