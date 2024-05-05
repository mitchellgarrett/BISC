namespace FTG.Studios.BISC.Asm
{

	public class Section : Directive
	{

		public string Identifier;

		public Section(string identifer)
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