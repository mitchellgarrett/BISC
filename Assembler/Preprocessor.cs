using System;
using System.Collections.Generic;
using System.Linq;

namespace FTG.Studios.BISC {
	
	/// <summary>
	/// Preprocessor for BISC assembler.
	/// </summary>
	public static class Preprocessor {
		
		static Dictionary<string, PseudoOpcode> pseudo_opcodes;
		
		public static void Preprocess(List<string> lines) {
			for (int lineno = 0; lineno < lines.Count; lineno++) {
				string line = lines[lineno];
				int comment_index = line.IndexOf(Specification.COMMENT);
				if (comment_index >= 0) line = line.Substring(0, comment_index);
                if (string.IsNullOrEmpty(line)) continue;
				string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
				for (int i = 0; i < parameters.Length; i++) {
					if (parameters[i].IndexOf('[') >= 0) {
						string[] tokens = parameters[i].Split('[', ']');
						UInt32 value = UInt32.Parse(tokens[0]);
						int index = int.Parse(tokens[1]);
						value = (value >> 16 * index) & 0xFFFF;
						parameters[i] = "0x" + value.ToString("x4");
						lines[lineno] = String.Join(", ", parameters);
					}
				}
			}
		}
		
		public static void ResolvePseudoInstructions(List<string> lines) {
			if (pseudo_opcodes == null) {
                pseudo_opcodes = new Dictionary<string, PseudoOpcode>();
                foreach (PseudoOpcode opcode in Enum.GetValues(typeof(PseudoOpcode))) {
                    pseudo_opcodes[opcode.ToString()] = opcode;
                }
            }
			
			for (int lineno = 0; lineno < lines.Count; lineno++) {
				string line = lines[lineno];
				int comment_index = line.IndexOf(Specification.COMMENT);
				if (comment_index >= 0) line = line.Substring(0, comment_index);
                if (string.IsNullOrEmpty(line)) continue;
				
				string[] instructions = ResolvePseudoInstruction(line);
				if (instructions != null) {
					lines.RemoveAt(lineno);
					lines.InsertRange(lineno--, instructions);
				}
			}
		}
		
		public static void ResolveSymbols(List<string> lines) {
			
		}
		
		static string[] ResolvePseudoInstruction(string line) {
			string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (!pseudo_opcodes.TryGetValue(parameters[0].ToUpper(), out PseudoOpcode opcode)) return null;
			string[] instructions = new string[Specification.pseudo_instructions[(int) opcode].Length];
			Array.Copy(Specification.pseudo_instructions[(int) opcode], instructions, instructions.Length);
			for (int i = 0; i < instructions.Length; i++) {
				instructions[i] = string.Format(instructions[i].ToLower(), parameters);
			}
			return instructions;
        }
	}
}