using System;
using System.Linq;

namespace FTG.Studios.BISC {
	
	public class Instruction {
		
		public UInt32 Address;
		public string Mneumonic;
		public Opcode? Opcode;
		public Parameter[] Parameters;
		
		public Instruction() { }
		
		public Instruction(string source) {
			int comment_index = source.IndexOf(Specification.COMMENT);
			if (comment_index >= 0) source = source.Substring(0, comment_index);
            if (string.IsNullOrEmpty(source)) return;
			string[] parameters = source.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			
			if (parameters[0].IndexOf(':') > 0) {
				Mneumonic = "SYMBOL";
				Parameters = new Parameter[1];
				Parameters[0].Type = ArgumentType.Symbol;
				Parameters[0].Mneumonic = parameters[0].Substring(0, parameters[0].IndexOf(':'));
				return;
			}
			
			Mneumonic = parameters[0].ToUpper();
			Opcode = (Opcode?) Assembler.ParseOpcode(Mneumonic);
			
			Parameters = new Parameter[parameters.Length - 1];
			for (int i = 0; i < Parameters.Length; i++) {
				Parameters[i].Mneumonic = parameters[i + 1];
			}
			
			for (int i = 0; i < Parameters.Length; i++) {
				byte? reg = Assembler.ParseRegister(Parameters[i].Mneumonic);
				if (reg.HasValue) {
					Parameters[i].Type = ArgumentType.Register;
					Parameters[i].Value = reg.Value;
					continue;
				}
				
				UInt32? imm = Assembler.ParseInteger32(Parameters[i].Mneumonic);
				if (imm.HasValue) {
					Parameters[i].Type = ArgumentType.Immediate32;
					Parameters[i].Value = imm.Value;
					continue;
				}
				
				UInt16? mem = Assembler.ParseMemory(Parameters[i].Mneumonic);
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
			string value = $"{Mneumonic}";
			for (int i = 0; i < Parameters.Length; i++) {
				switch (Parameters[i].Type) {
					case ArgumentType.Register:
						value += $" {Parameters[i].Mneumonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x2})";
						break;
					case ArgumentType.Memory:
					case ArgumentType.Immediate16:
						value += $" {Parameters[i].Mneumonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x4})";
						break;
					case ArgumentType.Immediate32:
						value += $" {Parameters[i].Mneumonic} ({Parameters[i].Type}, 0x{Parameters[i].Value:x8})";
						break;
					case ArgumentType.Symbol:
						value += $" {Parameters[i].Mneumonic} ({Parameters[i].Type})";
						break;
				}
				if (i < Parameters.Length - 1) value += ",";
			}
			return value;
		}
		
		public struct Parameter {
			public string Mneumonic;
			public ArgumentType Type;
			public UInt32 Value;
		}
	}
}