using System;

namespace FTG.Studios.BISC.Assembler {

    static class Syntax {

        public const char line_seperator = '\n';
        public const char seperator = ',';
        public const char open_bracket = '[';
        public const char close_bracket = ']';
        public const char single_quote = '\'';
        public const char double_quote = '"';
        public const char label_delimeter = ':';
        public const char comment = ';';

        //public const string line_seperator = @"\r?\n";
        public const string identifer = @"^([_a-zA-Z][_a-zA-Z0-9]*)([\._a-zA-Z][_a-zA-Z0-9]*)*$";
        public const string integer_literal = @"^\d+$";
        public const string hexadecimal_literal = @"^(0x|0X)[a-fA-F0-9]+$";
        public const string binary_literal = @"^(0b|0B)[01]+$";
        public const string decimal_literal = @"^((\d+(\.\d*)?)|(\.\d+))$";
        public const string string_literal = @"^""[a-zA-Z0-9]+""$";

        public static Opcode? GetOpcode(string Mnemonic) {
            for (Opcode opcode = 0; (int)opcode < Enum.GetValues(typeof(Opcode)).Length; opcode++ ) {
                if (Mnemonic == opcode.ToString()) return opcode;
            }
            return null;
        }

        public static Register? GetRegister(string Mnemonic) {
            for (Register reg = 0; (int)reg < Enum.GetValues(typeof(Opcode)).Length; reg++) {
                if (reg.IsValid() && Mnemonic == reg.ToString()) return reg;
            }
            return null;
        }

        public static bool IsImmediate(this TokenType type) {
            return type == TokenType.Label || type == TokenType.Integer;
        }
    }
}
