using System;
using System.Collections.Generic;
using System.Linq;

namespace FTG.Studios.BISC.Assembler {
	
	/// <summary>
	/// Preprocessor for BISC assembler.
	/// </summary>
	public static class Preprocessor {
		
		public static string Preprocess(string source) {
			source = source.Replace("\r\n", "\n");
			List<string> lines = new List<string>(source.Split('\n'));
            for (int i = 0; i < lines.Count; i++) {
				string line = lines[i];
            }
			return source;
		}
	}
}