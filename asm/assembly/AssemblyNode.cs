using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Node { }
		
		public abstract class BlockItem : Node { }
		
		public class Section : BlockItem {
			public readonly string Identifier;
			public readonly List<BlockItem> Body;
			
			public Section(string identifier, List<BlockItem> body) {
				Identifier = identifier;
				Body = body;
			}
			
			public override string ToString() {
				string output = $"Section(\"{Identifier}\")\n";
				foreach (BlockItem item in Body) output += item.ToString() + '\n';
				return output;
			}
		}
		
		public class Label : BlockItem {
			public readonly string Identifier;
			
			public Label(string identifier) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"Label(\"{Identifier}\")";
			}
		}
	}
}