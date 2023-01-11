using System;

namespace FTG.Studios.BISC.Assembler {

    public enum TokenType { Invalid, Opcode, PseudoOp, Label, Register, Integer, Character, Decimal, OpenBracket, CloseBracket, Seperator, LineSeperator, Comment, LabelDelimeter }

    public struct Token {
        
        public TokenType Type;
        public string Mnemonic;
        public UInt32? Value;
        public int LineNo;
        public int CharNo;

        public bool IsValid { get { return Type != TokenType.Invalid; } }

        public Token(TokenType type, int ln, int ch) {
            Type = type;
            Mnemonic = null;
            Value = null;
            LineNo = ln;
            CharNo = ch;
        }

        public Token(TokenType type, string mnemonic, UInt32? value, int ln, int ch) {
            Type = type;
            Mnemonic = mnemonic;
            Value = value;
            LineNo = ln;
            CharNo = ch;
        }

        /*public static Token Invalid {
            get { return new Token(TokenType.Invalid); }
        }*/

        public override string ToString() {
            if (Value != null) return $"(Ln: {LineNo}, Ch: {CharNo}) <{Type}, {Value}>";
            return $"<{Type}>";
        }
    }
}
