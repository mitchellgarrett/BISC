﻿using System;

namespace FTG.Studios.BISC.Asm {

	public enum TokenType { Invalid, Opcode, PseudoOp, Identifier, Register, Immediate, Seperator, LineSeperator, LabelDelimeter, OpenBracket, CloseBracket, OpenParenthesis, CloseParenthesis, Comment, DirectivePrefix, DataInitializer, SingleQuote, DoubleQuote, String, MacroExpansionOperator }

	public struct Token {

		public TokenType Type;
		public string Mnemonic;
		// TODO: Make this an object
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

		public override string ToString() {
			string value = $"(Ln: {LineNo}, Ch: {CharNo}) <{Type}";
			if (Mnemonic != null) value += $", {Mnemonic}";
			if (Value != null) value += $" ({Value})";
			value += '>';
			return value;
		}
	}
}
