namespace FTG.Studios.BISC.Asm
{

	public abstract class Directive : AssemblerData
	{

		public Directive()
		{
			Size = 0;
			HasUndefinedSymbol = false;
		}
		public override byte[] Assemble()
		{
			return new byte[] { };
		}
	}
}