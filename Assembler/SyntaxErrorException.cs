using System;

namespace FTG.Studios.BISC.Asm
{

	public class SyntaxErrorException : Exception
	{

		public SyntaxErrorException(string message) : base(message) { }
	}
}