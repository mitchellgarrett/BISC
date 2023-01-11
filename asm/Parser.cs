using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Assembler {

    public static class Parser {

        public static Program Parse(List<Token> tokens) {
            Queue<Token> stream = new Queue<Token>(tokens);

            Program program = new Program();
            while (stream.Count > 0) {
                if (Match(stream.Peek(), TokenType.Opcode)) {
                    program.Instructions.Add(ParseInstruction(stream));
                } else if (Match(stream.Peek(), TokenType.Label)) {
                    MatchFail(stream.Dequeue(), TokenType.Label);
                    MatchFail(stream.Dequeue(), TokenType.LabelDelimeter);
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
                case InstructionFormat.I:   inst = ParseIInstruction(tokens); break;
                case InstructionFormat.R:   inst = ParseRInstruction(tokens); break;
                case InstructionFormat.RI:  inst = ParseRIInstruction(tokens); break;
                case InstructionFormat.M:   inst = ParseMInstruction(tokens); break;
                case InstructionFormat.RD:  inst = ParseRDInstruction(tokens); break;
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
            inst.Parameters = new Token[0];

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
            MatchFail(inst.Parameters[1], TokenType.Integer);

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
            MatchFail(inst.Parameters[2], TokenType.Integer);

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

        static Instruction[] ParsePseudoInstruction(Queue<Token> tokens) {
            return null;
        }

        static void ParseLabel(Queue<Token> tokens) {
            Token label = tokens.Dequeue();
            MatchFail(label, TokenType.Label);
            MatchFail(tokens.Dequeue(), TokenType.LabelDelimeter);

            if (tokens.Count > 0) {
                if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
                MatchFail(tokens.Dequeue(), TokenType.LineSeperator);
            }
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
