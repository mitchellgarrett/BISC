namespace FTG.Studios.BISC.Asm
{

	public class ConstantDefinitionDirective : Directive
	{
		public string Identifier;
		public Token Value;

		public ConstantDefinitionDirective(string identifer, Token value)
		{
			Identifier = identifer;
			Value = value;
		}
		public override byte[] Assemble()
		{
			return new byte[] { };
		}

		public override string ToString()
		{
			return $"{Identifier} = {Value}";
		}
	}
}