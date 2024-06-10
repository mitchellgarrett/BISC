using System;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Directive : BlockItem { }
		
		public class SectionDefinition : Directive {
			public readonly string Identifier;
			
			public SectionDefinition(string identifier) {
				Identifier = identifier;
			}

			public override string ToString() {
				return $"SectionDefinition(\"{Identifier}\")";
			}
		}
		
		public class DataInitializer : Directive {
			public readonly byte[] Data;
			
			public DataInitializer(byte[] data) {
				Data = data;
			}
			
			public override string ToString()
		{
			string output = "Data(";
			foreach (var b in Data) output += $"{b:x2} ";
			output = output[..^1] + ')';
			return output;
		}
		}
	}
}