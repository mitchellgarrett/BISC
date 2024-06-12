using System;

namespace FTG.Studios.BISC.Asm {

	static class Syntax {

		public const char carriage_return = '\r';
		public const char line_seperator = '\n';
		public const char seperator = ',';
		public const char open_bracket = '[';
		public const char close_bracket = ']';
		public const char open_parenthesis = '(';
		public const char close_parenthesis = ')';
		public const char single_quote = '\'';
		public const char double_quote = '"';
		public const char label_delimeter = ':';
		public const char comment = ';';

		public const string identifer = @"^([_a-zA-Z][_a-zA-Z0-9]*)([\._a-zA-Z][_a-zA-Z0-9]*)*$";
		public const string integer_literal = @"^-?\d+$";
		public const string hexadecimal_literal = @"^(0x|0X)[a-fA-F0-9]+$";
		public const string binary_literal = @"^(0b|0B)[01]+$";
		public const string decimal_literal = @"^((\d+(\.\d*)?)|(\.\d+))$";
		public const string char_literal = @"^'([ -~]|\\0|\\b|\\t|\\n|\\r)'$";

		public const char directive_prefix = '%';
		public const char macro_expansion_operator = '$';
		public const char data_prefix = '.';

		public const string directive_section = "section";
		public const string directive_define = "define";
		public const string directive_relocation_lo = "lo";
		public const string directive_relocation_hi = "hi";

		public const string data_byte = ".byte";
		public const string data_half = ".half";
		public const string data_word = ".word";
		public const string data_string = ".string";
		// TODO: consider renaming this to .null
		public const string data_zero = ".zero";

		public static Opcode? GetOpcode(string mnemonic)
		{
			for (Opcode opcode = 0; (int)opcode < Enum.GetValues(typeof(Opcode)).Length; opcode++)
			{
				if (mnemonic == opcode.ToString()) return opcode;
			}
			return null;
		}

		public static Register? GetRegister(string mnemonic)
		{
			for (Register reg = 0; (int)reg < Enum.GetValues(typeof(Register)).Length; reg++)
			{
				if (mnemonic == reg.ToString()) return reg;
			}
			return null;
		}

		public static bool IsImmediate(this TokenType type)
		{
			return type == TokenType.Identifier || type == TokenType.Immediate;
		}
	}
}
