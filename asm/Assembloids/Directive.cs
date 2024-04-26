namespace FTG.Studios.BISC.Asm
{

	public class Directive : Assembloid
	{

		public string Identifier;

		public Directive(string identifer)
		{
			Identifier = identifer;
		}

		public override bool HasUndefinedSymbol()
		{
			return false;
		}
	}
}