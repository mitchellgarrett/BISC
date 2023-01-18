using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Assembler {

    public static class Parser {

        public static Program Parse(List<Token> tokens) {
            // First pass: resolve pseduo-instructions to real instructions
            Queue<Token> stream;
            for (int i = 0; i < tokens.Count; i++) {
                if (tokens[i].Type == TokenType.PseudoOp) {
                    stream = new Queue<Token>();
                    int len;
                    for (len = 0; i + len < tokens.Count; len++) {
                        stream.Enqueue(tokens[i + len]);
                        if (tokens[i + len].Type == TokenType.LineSeperator) break;
                    }
                    Token[] instructions = ParsePseudoInstruction(stream);
                    if (instructions != null) {
                        tokens.RemoveRange(i, len);
                        tokens.InsertRange(i--, instructions);
                    }
                }
            }

            // Second pass: parse instructions and labels
            stream = new Queue<Token>(tokens);
            string label = null;
            Program program = new Program();
            while (stream.Count > 0) {
                Console.WriteLine(stream.Peek());
                switch (stream.Peek().Type) {
                    case TokenType.Opcode:
                        program.Instructions.Add(ParseInstruction(stream));
                        if (!string.IsNullOrEmpty(label)) {
                            program.Labels[label] = program.Instructions[program.Instructions.Count - 1];
                            label = null;
                        }
                        break;
                    case TokenType.Label:
                        label = ParseLabel(stream);
                        break;
                    default:
                        Fail(stream.Peek(), TokenType.Opcode);
                        break;
                }

                while (stream.Count > 0 && (Match(stream.Peek(), TokenType.LineSeperator) || Match(stream.Peek(), TokenType.Comment))) {
                    stream.Dequeue();
                }
            }
            return program;
        }

        static Instruction ParseInstruction(Queue<Token> tokens) {
            Token opcode = tokens.Peek();
            InstructionFormat format = Specification.instruction_formats[opcode.Value.Value];
            Instruction inst = null;
            switch (format) {
                case InstructionFormat.I:   inst = ParseIInstruction(tokens);   break;
                case InstructionFormat.R:   inst = ParseRInstruction(tokens);   break;
                case InstructionFormat.RI:  inst = ParseRIInstruction(tokens);  break;
                case InstructionFormat.M:   inst = ParseMInstruction(tokens);   break;
                case InstructionFormat.RD:  inst = ParseRDInstruction(tokens);  break;
                case InstructionFormat.RRD: inst = ParseRRDInstruction(tokens); break;
            }

            if (tokens.Count > 0) {
                if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
                MatchFail(tokens.Dequeue(), TokenType.LineSeperator);
            }

            return inst;
        }

        static Instruction ParseIInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;

            return inst;
        }

        static Instruction ParseRInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[1];

            inst.Parameters[0] = tokens.Dequeue();
            MatchFail(inst.Parameters[0], TokenType.Register);

            return inst;
        }

        static Instruction ParseRIInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[2];

            inst.Parameters[0] = tokens.Dequeue();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.Seperator);

            inst.Parameters[1] = tokens.Dequeue();
            MatchFail(inst.Parameters[1], TokenType.Immediate);

            return inst;
        }

        static Instruction ParseMInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[3];

            inst.Parameters[0] = tokens.Dequeue();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.Seperator);

            inst.Parameters[1] = tokens.Dequeue();
            MatchFail(inst.Parameters[1], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.OpenBracket);

            inst.Parameters[2] = tokens.Dequeue();
            MatchFail(inst.Parameters[2], TokenType.Immediate);

            MatchFail(tokens.Dequeue(), TokenType.CloseBracket);
            
            return inst;
        }

        static Instruction ParseRDInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[2];

            inst.Parameters[0] = tokens.Dequeue();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.Seperator);

            inst.Parameters[1] = tokens.Dequeue();
            MatchFail(inst.Parameters[1], TokenType.Register);

            return inst;
        }

        static Instruction ParseRRDInstruction(Queue<Token> tokens) {
            Instruction inst = new Instruction();
            Token opcode = tokens.Dequeue();
            MatchFail(opcode, TokenType.Opcode);

            inst.Opcode = (Opcode)opcode.Value.Value;
            inst.Parameters = new Token[3];

            inst.Parameters[0] = tokens.Dequeue();
            MatchFail(inst.Parameters[0], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.Seperator);

            inst.Parameters[1] = tokens.Dequeue();
            MatchFail(inst.Parameters[1], TokenType.Register);

            MatchFail(tokens.Dequeue(), TokenType.Seperator);

            inst.Parameters[2] = tokens.Dequeue();
            MatchFail(inst.Parameters[2], TokenType.Register);

            return inst;
        }

        static Token[] ParsePseudoInstruction(Queue<Token> tokens) {
            Token pseudo_op = tokens.Dequeue();
            MatchFail(pseudo_op, TokenType.PseudoOp);

            List<Token> args = new List<Token>();
            while (tokens.Count > 0) {
                Token token = tokens.Dequeue();
                if (Match(token, TokenType.LineSeperator)) break;
                if (Match(token, TokenType.Comment)) continue;
                args.Add(token);
            }

            int index = (int)pseudo_op.Value;
            for (; index < Specification.pseudo_instruction_names.Length; index++) {
                if (pseudo_op.Mnemonic == Specification.pseudo_instruction_names[index]) {
                    bool valid = true;
                    Queue<Token> temp_stream = new Queue<Token>(args);
                    List<Token> temp_args = new List<Token>(3);
                    ArgumentType[] arg_types = Specification.pseudo_instruction_arguments[index];
                    for (int a = 0; valid && a < arg_types.Length; a++) {
                        switch (arg_types[a]) {
                            case ArgumentType.None:
                                if (temp_stream.Count > 0) valid = false;
                                break;
                            case ArgumentType.Register:
                                if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Register)) valid = false;
                                else temp_args.Add(temp_stream.Dequeue());
                                break;
                            case ArgumentType.Memory:
                                if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Register)) valid = false;
                                else temp_args.Add(temp_stream.Dequeue());
                                if (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.OpenBracket)) valid = false;
                                if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Immediate)) valid = false;
                                else temp_args.Add(temp_stream.Dequeue());
                                if (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.CloseBracket)) valid = false;
                                break;
                            case ArgumentType.Immediate32:
                                if (temp_stream.Count == 0 || !Match(temp_stream.Peek(), TokenType.Immediate)) valid = false;
                                else temp_args.Add(temp_stream.Dequeue());
                                break;
                        }
                        if (a < arg_types.Length - 1 && (temp_stream.Count == 0 || !Match(temp_stream.Dequeue(), TokenType.Seperator))) valid = false;
                    }
                    if (temp_stream.Count > 0) valid = false;
                    if (valid) {
                        args = temp_args;
                        break;
                    }
                }
            }

            if (index >= Specification.pseudo_instruction_names.Length) {
                Opcode? opcode = Syntax.GetOpcode(pseudo_op.Mnemonic);
                if (opcode.HasValue) {
                    pseudo_op.Type = TokenType.Opcode;
                    pseudo_op.Value = (UInt32)opcode.Value;
                } else {
                    Fail(pseudo_op, TokenType.Opcode);
                }
                args.Insert(0, pseudo_op);
                return args.ToArray();
            }

            for (int i = args.Count; i < 3; i++) {
                args.Add(new Token(TokenType.Invalid, 0, 0));
            }
            
            string[] definition = Specification.pseudo_instruction_definitions[index];
            string[] replacements = new string[definition.Length];
            Array.Copy(definition, replacements, replacements.Length);

            List<Token> replacement_tokens = new List<Token>();
            for (int i = 0; i < replacements.Length; i++) {
                replacements[i] = string.Format(replacements[i], args[0].Mnemonic, args[1].Mnemonic, args[2].Mnemonic);
                replacements[i] += '\n';
                List<Token> vals = Lexer.Tokenize(replacements[i]);
                for (int v = 0; v < vals.Count; v++) {
                    Token t = vals[v];
                    t.LineNo = pseudo_op.LineNo;
                }
                replacement_tokens.AddRange(vals);
            }

            return replacement_tokens.ToArray();
        }

        static string ParseLabel(Queue<Token> tokens) {
            Token label = tokens.Dequeue();
            MatchFail(label, TokenType.Label);
            MatchFail(tokens.Dequeue(), TokenType.LabelDelimeter);
            return label.Mnemonic;
        }

        static bool Match(Token token, TokenType type) {
            return token.Type == type;
        }

        static void MatchFail(Token token, TokenType type) {
            if (!Match(token, type)) Fail(token, type);
        }

        static void Fail(Token token) {
            Console.Error.WriteLine($"Invalid token: {token}");
            Environment.Exit(1);
        }

        static void Fail(Token token, TokenType expected) {
            Console.Error.WriteLine($"Invalid token: {token} (expected {expected})");
            Environment.Exit(1);
        }
    }
}
