using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	// TODO: Maybe change name back to AssemblyResult; not much of a tree
	public class AssemblyTree {
		public List<AssemblyNode.BlockItem> Body;
			
			public AssemblyTree(List<AssemblyNode.BlockItem> body) {
				Body = body;
			}
			
			public override string ToString() {
				string output = $"Program()\n";
				foreach (var item in Body) output += item.ToString() + '\n';
				return output;
			}
	}
}