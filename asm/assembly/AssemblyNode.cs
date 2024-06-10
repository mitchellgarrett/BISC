using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Node { }
		
		public class Program : Node {
			public readonly List<BlockItem> Body;
			
			public Program(List<BlockItem> body) {
				Body = body;
			}
			
			public override string ToString() {
				string output = $"Program()\n";
				foreach (BlockItem item in Body) output += item.ToString() + '\n';
				return output;
			}
		}
		
		public abstract class BlockItem : Node { }
		
		public class Label : BlockItem {
			public readonly string Identifier;
			
			public Label(string identifier) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"Label(\"{Identifier}\")";
			}
		}
		
		public abstract class Constant : Node { }
		
		public class Immediate : Constant {
			public readonly UInt32 Value;
			
			public Immediate(UInt32 value) {
				Value = value;
			}
			
			public override string ToString() {
				return $"Immediate(0x{Value:x8})";
			}
		}
		
		public class Symbol : Constant {
			public readonly string Identifier;
			
			public Symbol(string identifier) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"Symbol(\"{Identifier}\")";
			}
		}
	}
}