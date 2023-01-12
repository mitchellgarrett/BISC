﻿using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Assembler {

    public static class Parser {

        public static Program Parse(List<Token> tokens) {
            LinkedList<Token> stream = new LinkedList<Token>(tokens);

            Program program = new Program();
            while (stream.Count > 0) {
                switch (stream.First.Value.Type) {
                    case TokenType.Opcode:
                        program.Instructions.Add(ParseInstruction(stream));
                        break;
                    case TokenType.PseudoOp:
                        //program.Instructions.AddRange(ParsePseudoInstruction(stream));
                        ParsePseudoInstruction(stream);
                        break;
                    case TokenType.Label:
                        ParseLabel(stream);
                        break;
                }

                while (stream.Count > 0 && (Match(stream.First.Value, TokenType.LineSeperator) || Match(stream.First.Value, TokenType.Comment))) {
                    stream.RemoveFirst();
                }
            }
            return program;
        }

        static Instruction ParseInstruction(LinkedList<Token> tokens) {
            Token opcode = tokens.First.Value;
            InstructionFormat format = Specification.instruction_formats[opcode.Value.Value];
            Instruction inst = null;
            switch (format) {
                case InstructionFormat.I:   inst = ParseIInstruction(tokens); break;
                case InstructionFormat.R:   inst = ParseRInstruction(tokens); break;
                case InstructionFormat.RI:  inst = ParseRIInstruction(tokens); break;
                case InstructionFormat.M:   inst = ParseMInstruction(tokens); break;
                case InstructionFormat.RD:  inst = ParseRDInstruction(tokens); break;
                case InstructionFormat.RRD: inst = ParseRRDInstruction(tokens); break;
            }

            if (tokens.Count > 0) {
                if (Match(tokens.First.Value, TokenType.Comment)) tokens.RemoveFirst();
                MatchFail(tokens.First.Value, TokenType.LineSeperator);
                tokens.RemoveFirst();
            }

            return inst;
        }

        static Instruction ParseIInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            MatchFail(opcode, TokenType.Opcode);
            tokens.RemoveFirst();

            inst.Opcode = (Opcode)opcode.Value.Value;

            return inst;
        }

        static Instruction ParseRInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            MatchFail(opcode, TokenType.Opcode);
            tokens.RemoveFirst();

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[1];

            inst.Parameters[0] = tokens.First.Value;
            MatchFail(inst.Parameters[0], TokenType.Register);
            tokens.RemoveFirst();

            return inst;
        }

        static Instruction ParseRIInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            MatchFail(opcode, TokenType.Opcode);
            tokens.RemoveFirst();

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[2];

            inst.Parameters[0] = tokens.First.Value;
            MatchFail(inst.Parameters[0], TokenType.Register);
            tokens.RemoveFirst();

            MatchFail(tokens.First.Value, TokenType.Seperator);
            tokens.RemoveFirst();

            inst.Parameters[1] = tokens.First.Value;
            MatchFail(inst.Parameters[1], TokenType.Immediate);
            tokens.RemoveFirst();

            return inst;
        }

        static Instruction ParseMInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            MatchFail(opcode, TokenType.Opcode);
            tokens.RemoveFirst();

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[3];

            inst.Parameters[0] = tokens.First.Value;
            MatchFail(inst.Parameters[0], TokenType.Register);
            tokens.RemoveFirst();

            MatchFail(tokens.First.Value, TokenType.Seperator);
            tokens.RemoveFirst();

            inst.Parameters[1] = tokens.First.Value;
            MatchFail(inst.Parameters[1], TokenType.Register);
            tokens.RemoveFirst();

            MatchFail(tokens.First.Value, TokenType.OpenBracket);
            tokens.RemoveFirst();

            inst.Parameters[2] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[2], TokenType.Immediate);

            MatchFail(tokens.First.Value, TokenType.CloseBracket);
            tokens.RemoveFirst();

            return inst;
        }

        static Instruction ParseRDInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[2];

            inst.Parameters[0] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.First.Value, TokenType.Seperator);
            tokens.RemoveFirst();

            inst.Parameters[1] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[1], TokenType.Register);

            return inst;
        }

        static Instruction ParseRRDInstruction(LinkedList<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[3];

            inst.Parameters[0] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.First.Value, TokenType.Seperator);
            tokens.RemoveFirst();

            inst.Parameters[1] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[1], TokenType.Register);

            MatchFail(tokens.First.Value, TokenType.Seperator);
            tokens.RemoveFirst();

            inst.Parameters[2] = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(inst.Parameters[2], TokenType.Register);

            return inst;
        }

        static Instruction[] ParsePseudoInstruction(LinkedList<Token> tokens) {
            Token pseduo_op = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(pseduo_op, TokenType.PseudoOp);

            List<Token> args = new List<Token>();
            while (tokens.Count > 0) {
                Token token = tokens.First.Value;
                tokens.RemoveFirst();
                if (Match(token, TokenType.LineSeperator)) break;
                if (Match(token, TokenType.Comment)) continue;
                args.Add(token);
            }

            List<ArgumentType> arg_types = new List<ArgumentType>();
            for (int i = 0; i < args.Count; i++) {
                Token arg = args[i];
                switch (arg.Type) {
                    case TokenType.Register:
                        if (i < args.Count - 1) {
                            if (Match(args[i + 1], TokenType.Seperator)) {
                                arg_types.Add(ArgumentType.Register);
                                args.RemoveAt(i + 1);
                                i--;
                            } else if(Match(args[i + 1], TokenType.OpenBracket)) {
                                arg_types.Add(ArgumentType.Memory);
                                args.RemoveAt(i + 1);
                                i--;
                            }
                        } else {
                            arg_types.Add(ArgumentType.Register);
                        }
                        break;
                    case TokenType.Immediate:
                        if (arg_types[i - 1] == ArgumentType.Register) {
                            arg_types.Add(ArgumentType.Immediate32);
                            if (i < args.Count - 1) {
                                MatchFail(args[i + 1], TokenType.Seperator);
                                args.RemoveAt(i + 1);
                                i--;
                            }
                        } else if (arg_types[i - 1] == ArgumentType.Memory) {
                            MatchFail(args[i + 1], TokenType.CloseBracket);
                            args.RemoveAt(i + 1);
                            i--;
                        } else {
                            arg_types.Add(ArgumentType.Immediate32);
                        }
                        break;
                }
            }

            Console.WriteLine("\n\n\nPSEDUO ARGS");
            Console.WriteLine(pseduo_op.Mnemonic);
            for (int i = 0; i < args.Count; i++) {
                Console.WriteLine(arg_types[i]);
                Console.WriteLine(args[i]);
            }
            Console.WriteLine("\n\n\n");
            
            for (int i = args.Count - 1; i < 3; i++) {
                args.Add(new Token(TokenType.Invalid, 0, 0));
            }

            int index = (int)pseduo_op.Value;
            for (; index < Specification.pseudo_instruction_names.Length; index++) {
                if (pseduo_op.Mnemonic == Specification.pseudo_instruction_names[index]) {
                    bool valid = true;
                    if (arg_types.Count != Specification.pseudo_instruction_arguments[index].Length)
                            valid = false;
                    for (int i = 0; valid && i < Specification.pseudo_instruction_arguments[index].Length; i++) {
                        if (arg_types[i] != Specification.pseudo_instruction_arguments[index][i])
                            valid = false;
                    }
                    if (valid) break;
                }
            }

            // Fix finding pseudos with same name and distinguishing between args
            string[] definition = Specification.pseudo_instruction_definitions[index];
            string[] replacements = new string[definition.Length];
            Array.Copy(definition, replacements, replacements.Length);

            for (int i = 0; i < replacements.Length; i++) {
                replacements[i] = string.Format(replacements[i], args[0].Mnemonic, args[1].Mnemonic, args[2].Mnemonic);
                replacements[i] += '\n';
                List<Token> vals = Lexer.Tokenize(replacements[i]);
                for (int v = vals.Count - 1; v >= 0; v--) {
                    tokens.AddFirst(vals[v]);
                }
            }

            return null;
        }

        static void ParseLabel(LinkedList<Token> tokens) {
            Token label = tokens.First.Value;
            tokens.RemoveFirst();
            MatchFail(label, TokenType.Label);
            MatchFail(tokens.First.Value, TokenType.LabelDelimeter);
            tokens.RemoveFirst();
        }

        static bool Match(Token token, TokenType type) {
            return token.Type == type;
        }

        static void MatchFail(Token token, TokenType type) {
            if (!Match(token, type)) Fail(token, type);
        }

        static void Fail(Token token) {
            Console.Error.WriteLine($"Invalid token: {token}");
        }

        static void Fail(Token token, TokenType expected) {
            Console.Error.WriteLine($"Invalid token: {token} (expected {expected})");
        }
    }
}
