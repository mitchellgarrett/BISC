namespace FTG.Studios.BISC.Asm
{

	public class Directive : Assembloid
	{

		public string Identifier;

		public Directive(string identifer)
		{
			Identifier = identifer;
			Size = 0;
			HasUndefinedSymbol = false;
		}
		public override byte[] Assemble()
		{
			return new byte[] { };
		}
	}
}