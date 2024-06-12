using System;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
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