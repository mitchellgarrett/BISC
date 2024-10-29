using System;

namespace FTG.Studios.BISC.VM
{

	public class IllegalExecutionException : Exception
	{

		public IllegalExecutionException(string message) : base(message) { }
	}
}