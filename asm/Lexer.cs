using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.BISC.Assembler {

    public static class Lexer {

        static int lineno;
        static int charno;

        public static List<Token> Tokenize(string source) {
            List<Token> tokens = new List<Token>();
            source = source.Replace("\r\n", "\n");

            lineno = charno = 1;

            Token token;
            string current_word = string.Empty;
            bool parse_comment = false;
            for (int i = 0; i < source.Length; i++) {
                char c = source[i];
                charno++;
                
                if (parse_comment && c != Syntax.line_seperator) {
                    current_word += c;
                    continue;
                }

                if (c == Syntax.line_seperator) {
                    if (parse_comment) parse_comment = false;
                    if (!string.IsNullOrEmpty(current_word))
                        tokens.Add(BuildToken(current_word));
                    tokens.Add(new Token(TokenType.LineSeperator, lineno, charno));
                    current_word = string.Empty;
                    lineno++;
                    charno = 0;
                    continue;
                }

                if (c == Syntax.comment) {
                    parse_comment = true;
                    current_word += c;
                    continue;
                }

                if (char.IsWhiteSpace(c)) {
                    if (!string.IsNullOrEmpty(current_word)) {
                        tokens.Add(BuildToken(current_word));
                        current_word = string.Empty;
                    }
                    continue;
                }

                token = BuildToken(c);
                if (token.IsValid) {
                    if (!string.IsNullOrEmpty(current_word))
                        tokens.Add(BuildToken(current_word));
                    tokens.Add(token);
                    current_word = string.Empty;
                    continue;
                }

                current_word += c;
            }

            if (!string.IsNullOrEmpty(current_word)) {
                tokens.Add(BuildToken(current_word));
            }

            return tokens;
        }

        static Token BuildToken(char lexeme) {
            switch (lexeme) {
                //case Syntax.line_seperator: return new Token(TokenType.LineSeperator);
                case Syntax.seperator: return new Token(TokenType.Seperator, lineno, charno);
                case Syntax.open_bracket: return new Token(TokenType.OpenBracket, lineno, charno);
                case Syntax.close_bracket: return new Token(TokenType.CloseBracket, lineno, charno);
                //case Syntax.single_quote: return new Token(TokenType.SingleQuote);
                //case Syntax.double_quote: return new Token(TokenType.DoubleQuot);
                case Syntax.label_delimeter: return new Token(TokenType.LabelDelimeter, lineno, charno);
                case Syntax.comment: return new Token(TokenType.Comment, lineno, charno);
            }

            return new Token(TokenType.Invalid, lexeme.ToString(), null, lineno, charno);
        }

        static Token BuildToken(string lexeme) {
            Opcode? opcode;
            if ((opcode = Syntax.GetOpcode(lexeme.ToUpper())).HasValue) {
                return new Token(TokenType.Opcode, opcode.ToString(), (UInt32)opcode.Value, lineno, charno);
            }

            for (int i = 0; i < Specification.pseudo_instruction_names.Length; i++) {
                if (lexeme.ToUpper() == Specification.pseudo_instruction_names[i])
                    return new Token(TokenType.PseudoOp, lexeme.ToUpper(), (UInt32)i, lineno, charno);
            }

            Register? register;
            if ((register = Syntax.GetRegister(lexeme.ToUpper())).HasValue) {
                return new Token(TokenType.Register, register.ToString(), (UInt32)register.Value, lineno, charno);
            }

            if (Regex.IsMatch(lexeme, Syntax.integer_literal) ||
                Regex.IsMatch(lexeme, Syntax.hexadecimal_literal) ||
                Regex.IsMatch(lexeme, Syntax.binary_literal) ||
                Regex.IsMatch(lexeme, Syntax.char_literal)
            ) {
                return new Token(TokenType.Immediate, lexeme, Assembler.ParseInteger32(lexeme), lineno, charno);
            }

            if (Regex.IsMatch(lexeme, Syntax.identifer)) {
                return new Token(TokenType.Label, lexeme, null, lineno, charno);
            }

            if (lexeme.Length > 0 && lexeme[0] == Syntax.comment) {
                return new Token(TokenType.Comment, lexeme, null, lineno, charno);
            }

            return new Token(TokenType.Invalid, lexeme, null, lineno, charno);
        }
    }
}
