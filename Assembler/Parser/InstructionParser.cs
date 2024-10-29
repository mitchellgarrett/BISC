using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class Parser {
		
		static AssemblyNode.Instruction ParseInstruction(LinkedList<Token> tokens) {
			Token opcode = tokens.Peek();
			InstructionFormat format = Specification.instruction_formats[opcode.Value.Value];
			return format switch {
				InstructionFormat.N => ParseNInstruction(tokens),
				InstructionFormat.R => ParseRInstruction(tokens),
				InstructionFormat.I => ParseIInstruction(tokens),
				InstructionFormat.M => ParseMInstruction(tokens),
				InstructionFormat.D => ParseDInstruction(tokens),
				InstructionFormat.T => ParseTInstruction(tokens),
				_ => null,
			};
		}
		
		static AssemblyNode.NInstruction ParseNInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			
			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");
			
			return new AssemblyNode.NInstruction(opcode);
		}
		
		static AssemblyNode.RInstruction ParseRInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			AssemblyNode.Register register = ParseRegister(tokens);
			
			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");
			
			return new AssemblyNode.RInstruction(opcode, register);
		}
		
		static AssemblyNode.IInstruction ParseIInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			AssemblyNode.Register register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{register}'");

			AssemblyNode.Constant immediate = ParseConstant(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");

			return new AssemblyNode.IInstruction(opcode, register, immediate);
		}
		
		static AssemblyNode.MInstruction ParseMInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			AssemblyNode.Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			AssemblyNode.Register source_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.OpenBracket, $"Expected '{Syntax.open_bracket}' after register {source_register}");

			AssemblyNode.Constant offset = ParseConstant(tokens);

			Expect(tokens.Dequeue(), TokenType.CloseBracket, $"Expected '{Syntax.open_bracket}' after offset {offset}");

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");

			return new AssemblyNode.MInstruction(opcode, destination_register, source_register, offset);
		}
		
		static AssemblyNode.DInstruction ParseDInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			AssemblyNode.Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			AssemblyNode.Register source_register = ParseRegister(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");

			return new AssemblyNode.DInstruction(opcode, destination_register, source_register);
		}
		
		static AssemblyNode.TInstruction ParseTInstruction(LinkedList<Token> tokens) {
			Opcode opcode = ParseOpcode(tokens);
			AssemblyNode.Register destination_register = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{destination_register}'");

			AssemblyNode.Register source_register_a = ParseRegister(tokens);

			Expect(tokens.Dequeue(), TokenType.Seperator, $"Expected '{Syntax.seperator}' after '{source_register_a}'");

			AssemblyNode.Register source_register_b = ParseRegister(tokens);

			if (Match(tokens.Peek(), TokenType.Comment)) tokens.Dequeue();
			Expect(tokens.Dequeue(), TokenType.LineSeperator, $"Line feed expected after instruction '{opcode}'");

			return new AssemblyNode.TInstruction(opcode, destination_register, source_register_a, source_register_b);
		}
	}
}