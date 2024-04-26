namespace FTG.Studios.BISC.Asm
{

	public class Label : Assembloid
	{

		public string Identifier;

		public Label(string identifer)
		{
			Identifier = identifer;
		}

		public override bool HasUndefinedSymbol()
		{
			return false;
		}
	}
}