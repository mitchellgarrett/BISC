using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {

		public abstract class Directive : BlockItem {
			protected Directive() : base(0) { }
		}

		public class SectionDefinition : Directive {
			public readonly string Identifier;
			
			public SectionDefinition(string identifier) {
				Identifier = identifier;
			}

			public override string ToString() {
				return $"SectionDefinition(\"{Identifier}\")";
			}
		}
		
		public class ConstantDefinition : Directive {
			public readonly string Identifier;
			public readonly Constant Value;
			
			public ConstantDefinition(string identifier, Constant value) {
				Identifier = identifier;
				Value = value;
			}

			public override string ToString() {
				return $"ConstantDefinition(\"{Identifier}\", {Value})";
			}
		}
		
		public class MacroDefinition : Directive {
			public readonly string Identifier;
			public readonly List<string> Parameters;
			public readonly List<BlockItem> Body;
			
			public MacroDefinition(string identifier, List<string> parameters, List<BlockItem> body) {
				Identifier = identifier;
				Parameters = parameters;
				Body = body;
			}

			public override string ToString() {
				string output = $"MacroDefinition(\"{Identifier}\"";
				foreach (string parameter in Parameters) output += parameter + ", ";
				if (Parameters.Count > 0) output = output[..^2];
				output += ")\n";
				foreach (BlockItem item in Body) output += item.ToString() + '\n';
				output += "MacroEnd()";
				return output;
			}
		}
		
		public class MacroAccess : Directive {
			public readonly string Identifier;
			public readonly List<string> Arguments;
			
			public MacroAccess(string identifier, List<string> arguments) {
				Identifier = identifier;
				Arguments = arguments;
			}
			
			public override string ToString() {
				string output = $"MacroAccess(\"{Identifier}\"";
				foreach (string parameter in Arguments) output += parameter + ", ";
				if (Arguments.Count > 0) output = output[..^2];
				output += ')';
				return output;
			}
		}
		
		public class MacroEnd : Directive {

			public override string ToString() {
				return "MacroEnd()";
			}
		}
		
		public class DataInitializer : Directive {
			public readonly byte[] Data;
			
			public DataInitializer(byte[] data) {
				Data = data;
			}
			
			public override string ToString() {
				string output = "Data(";
				foreach (var b in Data) output += $"{b:x2} ";
				output = output[..^1] + ')';
				return output;
			}
		}
		
		public class LinkerRelocation : Constant {
			public enum RelocationType { Lo, Hi };
			
			public readonly Constant Value;
			public readonly RelocationType Type;
			
			public LinkerRelocation(Constant value, RelocationType type) {
				Value = value;
				Type = type;
			}

			public override string ToString() {
				return $"{Type}({Value})";
			}
			
			public override string GetMnemonic() {
				return Type switch {
					RelocationType.Lo => $"%lo({Value.GetMnemonic()})",
					RelocationType.Hi => $"%hi({Value.GetMnemonic()})",
					_ => null,
				};
			}
		}
	}
}