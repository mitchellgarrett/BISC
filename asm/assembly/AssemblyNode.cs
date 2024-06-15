using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Node { }
		
		// TODO: Add line/column numbers
		// TODO: Add undefined symbol bool
		public abstract class BlockItem : Node { 
			public readonly int Size;
			public UInt32 Address;
			
			public BlockItem(int size) {
				Size = size;
			}
		}
		
		// TODO: Add size field
		public class Section : Node {
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
			
			public Label(string identifier) : base(0) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"Label(\"{Identifier}\")";
			}
		}
	}
}