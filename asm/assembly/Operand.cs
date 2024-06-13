using System;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Operand : Node {
			public abstract string GetMnemonic();
		}
		
		public class Register : Operand {
			public readonly BISC.Register Type;
			
			public Register(BISC.Register register) {
				Type = register;
			}

			public override string ToString() {
				return $"Register({Type})";
			}
			
			public override string GetMnemonic() {
				return Type.ToString();
			}
		}
		
		public abstract class Constant : Operand { }
		
		public class Immediate : Constant {
			public readonly UInt32 Value;
			
			public Immediate(UInt32 value) {
				Value = value;
			}
			
			public override string ToString() {
				return $"Immediate(0x{Value:x8})";
			}
			
			public override string GetMnemonic() {
				return Value.ToString();
			}
		}
		
		public class LabelAccess : Constant {
			public readonly string Identifier;
			
			public LabelAccess(string identifier) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"LabelAccess(\"{Identifier}\")";
			}
			
			public override string GetMnemonic() {
				return Identifier;
			}
		}
		
		public class MacroAccess : Constant {
			public readonly string Identifier;
			
			public MacroAccess(string identifier) {
				Identifier = identifier;
			}
			
			public override string ToString() {
				return $"MacroAccess(\"{Identifier}\")";
			}
			
			public override string GetMnemonic() {
				return $"{Syntax.macro_expansion_operator}{Identifier}";
			}
		}
	}
}