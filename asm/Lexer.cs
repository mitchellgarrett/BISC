﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FTG.Studios.BISC.Asm {

    public static class Lexer {

        static int lineno = 1;
        static int charno;

        public static void Reset() {
            lineno = 1;
        }

        public static List<Token> Tokenize(string source) {
            List<Token> tokens = new List<Token>();
            charno = 1;

            Token token;
            string current_word = string.Empty;
            bool parse_comment = false;
            for (int i = 0; i < source.Length; i++) {
                char c = source[i];
                charno++;

                if (c == Syntax.carriage_return) {
                    charno--; 
                    continue;
                }

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
                case Syntax.seperator: return new Token(TokenType.Seperator, lineno, charno);
                case Syntax.open_bracket: return new Token(TokenType.OpenBracket, lineno, charno);
                case Syntax.close_bracket: return new Token(TokenType.CloseBracket, lineno, charno);
                case Syntax.label_delimeter: return new Token(TokenType.LabelDelimeter, lineno, charno);
                case Syntax.comment: return new Token(TokenType.Comment, lineno, charno);
            }

            return new Token(TokenType.Invalid, lexeme.ToString(), null, lineno, charno);
        }

        static Token BuildToken(string lexeme) {
            for (int i = 0; i < Specification.pseudo_instruction_names.Length; i++) {
                if (lexeme.ToUpper() == Specification.pseudo_instruction_names[i])
                    return new Token(TokenType.PseudoOp, lexeme.ToUpper(), (UInt32)i, lineno, charno);
            }

            Opcode? opcode;
            if ((opcode = Syntax.GetOpcode(lexeme.ToUpper())).HasValue) {
                return new Token(TokenType.Opcode, opcode.ToString(), (UInt32)opcode.Value, lineno, charno);
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
                return new Token(TokenType.Immediate, lexeme, Assembler.ParseImmediate(lexeme), lineno, charno);
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
