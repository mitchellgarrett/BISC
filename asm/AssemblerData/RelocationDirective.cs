namespace FTG.Studios.BISC.Asm
{

	public class RelocationDirective : AssemblerData
	{

		public SectionData Section;
		public Instruction Instruction;
		public Label Symbol;

		public RelocationDirective(SectionData section, Instruction instruction, Label symbol)
		{
			Section = section;
			Instruction = instruction;
			Symbol = symbol;

			// Has undefined symbol if the given immediate value is a label that has not yet been defined
			// TODO: make this happen
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			return new byte[] { };
		}

		public override string ToString()
		{
			return $"Relocation: ${Section.Identifier}:${Symbol.Identifier} ${Instruction}";
		}
	}
}