namespace FTG.Studios.BISC.Asm {
	
	public class AssemblyTree {
		public readonly AssemblyNode.Program Program;
		
		public AssemblyTree(AssemblyNode.Program program) {
			Program = program;
		}

		public override string ToString() {
			return Program.ToString();
		}
	}
}