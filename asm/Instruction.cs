namespace FTG.Studios.BISC.Assembler {
	
	public class Instruction {

		public Opcode Opcode;
		public Token[] Parameters;

		public Instruction(params Token[] args) {
			if (args != null) Parameters = args;
			else Parameters = new Token[0];
        }

		public override string ToString() {
			string value = $"{Opcode}";
			for (int i = 0; i < Parameters.Length; i++) {
				Token arg = Parameters[i];
				switch (arg.Type) {
					case TokenType.Register:
						value += $" {arg.Mnemonic} ({arg.Type}, 0x{arg.Value:x2})";
						break;
					case TokenType.Immediate:
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
	}
}