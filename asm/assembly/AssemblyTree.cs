using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public class AssemblyTree {
		public readonly List<AssemblyNode.BlockItem> Body;
			
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