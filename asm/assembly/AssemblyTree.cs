using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public class AssemblyTree {
		public readonly List<AssemblyNode.Section> Sections;
			
			public AssemblyTree(List<AssemblyNode.Section> sections) {
				Sections = sections;
			}
			
			public override string ToString() {
				string output = $"Program()\n";
				foreach (var section in Sections) output += section.ToString() + '\n';
				return output;
			}
	}
}