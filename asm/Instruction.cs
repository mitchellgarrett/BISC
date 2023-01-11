using System;

namespace FTG.Studios.BISC.Assembler {
	
	public class Instruction {

		public Opcode Opcode;
		public Token[] Parameters;

		public Instruction() { }

		public override string ToString() {
			string value = $"{Opcode}";
			for (int i = 0; i < Parameters.Length; i++) {
				Token arg = Parameters[i];
				switch (arg.Type) {
					case TokenType.Register:
						value += $" {arg.Mnemonic} ({arg.Type}, 0x{arg.Value:x2})";
						break;
					case TokenType.Integer:
						if (Parameters.Length == 3)
							value += $" {arg.Mnemonic} ({arg.Type}, 0x{arg.Value:x2})";
						else 
							value += $" {arg.Mnemonic} ({arg.Type}, 0x{arg.Value:x4})";
						break;
				}
				if (i < Parameters.Length - 1) value += ",";
			}
			return value;
		}

		/*public int Line;
		public UInt32 Address;
		public string Mnemonic;
		public Opcode? Opcode;
		public Parameter[] Parameters;
		
		public Instruction() { }
		
		public Instruction(string source) {
			int comment_index = source.IndexOf(Specification.COMMENT);
			if (comment_index >= 0) source = source.Substring(0, comment_index);
            if (string.IsNullOrEmpty(source)) return;

			// TODO: Known bug where this doesn't work when loading ',', ' ', or '\t' chars 
			string[] parameters = source.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (parameters.Length == 0) return;

			if (parameters[0].IndexOf(':') > 0) {
				Mnemonic = "SYMBOL";
				Parameters = new Parameter[1];
				Parameters[0].Type = ArgumentType.Symbol;
				Parameters[0].Mnemonic = parameters[0].Substring(0, parameters[0].IndexOf(':'));
				return;
			}
			
			Mnemonic = parameters[0].ToUpper();
			Opcode = (Opcode?) Assembler.ParseOpcode(Mnemonic);
			
			Parameters = new Parameter[parameters.Length - 1];
			for (int i = 0; i < Parameters.Length; i++) {
				Parameters[i].Mnemonic = parameters[i + 1];
			}
			
			for (int i = 0; i < Parameters.Length; i++) {
				byte? reg = Assembler.ParseRegister(Parameters[i].Mnemonic);
				if (reg.HasValue) {
					Parameters[i].Type = ArgumentType.Register;
					Parameters[i].Value = reg.Value;
					continue;
				}
				
				UInt32? imm = Assembler.ParseInteger32(Parameters[i].Mnemonic);
				if (imm.HasValue) {
					Parameters[i].Type = ArgumentType.Immediate32;
					Parameters[i].Value = imm.Value;
					continue;
				}
				
				UInt16? mem = Assembler.ParseMemory(Parameters[i].Mnemonic);
				if (mem.HasValue) {
					Parameters[i].Type = ArgumentType.Memory;
					Parameters[i].Value = mem.Value;
					continue;
				}
				
				Parameters[i].Type = ArgumentType.Symbol;
				Parameters[i].Value = 0xFFFFFFFF;
			}
		}
		
		public UInt32 Assemble() {
			UInt32 machine_code = 0x00000000;
			machine_code |= (UInt32) Opcode.Value << 24;
			for (int i = 0; i < Parameters.Length; i++) {
				switch (Parameters[i].Type) {
					case ArgumentType.Register:
						machine_code |= (UInt32) Parameters[i].Value << (2 - i) * 8;
						break;
					case ArgumentType.Memory:
					case ArgumentType.Immediate16:
					case ArgumentType.Immediate32:
						machine_code |= (UInt32) Parameters[i].Value << (1 - i) * 8;
						break;
				}
				
			}
			return machine_code;
		}
		
		public override string ToString() {
			string value = $"{Mnemonic}";
			for (int i = 0; i < Parameters.Length; i++) {
				switch (Parameters[i].Type) {
					case ArgumentType.Register:
						value += $" {Parameters[i].Mnemonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x2})";
						break;
					case ArgumentType.Memory:
					case ArgumentType.Immediate16:
						value += $" {Parameters[i].Mnemonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x4})";
						break;
					case ArgumentType.Immediate32:
						value += $" {Parameters[i].Mnemonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x8})";
						break;
					case ArgumentType.Symbol:
						value += $" {Parameters[i].Mnemonic} ({Parameters[i].Type})";
						break;
				}
				if (i < Parameters.Length - 1) value += ",";
			}
			return value;
		}
		
		public struct Parameter {
			public string Mnemonic;
			public ArgumentType Type;
			public UInt32 Value;
		}*/
	}
}