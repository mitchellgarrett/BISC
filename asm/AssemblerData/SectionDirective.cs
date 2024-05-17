namespace FTG.Studios.BISC.Asm
{

	public class SectionDirective : Directive
	{

		public string Identifier;

		public SectionDirective(string identifer)
		{
			Identifier = identifer;
			Size = 0;
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			return new byte[] { };
		}

		public override string ToString()
		{
			return $"{Identifier}: 0x{Address:x8}";
		}
	}
}