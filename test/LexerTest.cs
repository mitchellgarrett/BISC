using NUnit.Framework;
using System;
using System.Collections.Generic;
using FTG.Studios.BISC.Asm;

namespace FTG.Studios.BISC.Test {
	
	[TestFixture]
	public class LexerTest {

		[Test]
		public void Tokenize_CanLex_Newline() {
			string source = "\n";

			Token expected_token = new Token(TokenType.LineSeperator, 1, 1);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(1, tokens.Count);

			Assert.AreEqual(expected_token, tokens[0]);
        }

		[Test]
		public void Tokenize_CanLex_CommentWithoutNewline() {
			string source = "; this here is a comment";

			Token expected_comment_token = new Token(TokenType.Comment, source, null, 1, 1);
			Token expected_newline_token = new Token(TokenType.LineSeperator, 1, source.Length + 1);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(2, tokens.Count);

			Assert.AreEqual(expected_comment_token, tokens[0]);
			Assert.AreEqual(expected_newline_token, tokens[1]);
		}

		[Test]
		public void Tokenize_CanLex_CommentWithNewline() {
			const string expected_comment = "; this here is a comment";
			string source = $"{expected_comment}\n";

			Token expected_comment_token = new Token(TokenType.Comment, expected_comment, null, 1, 1);
			Token expected_newline_token = new Token(TokenType.LineSeperator, 1, source.Length);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(2, tokens.Count);

			Assert.AreEqual(expected_comment_token, tokens[0]);
			Assert.AreEqual(expected_newline_token, tokens[1]);
		}

		[Test]
		public void Tokenize_CanLex_CommentAfterSemicolon(){}
		
		[Test]
		public void Tokenize_CanLex_CommentAfterInstruction(){}
		
		[Test]
		public void Tokenize_CanLex_CommentAfterLabel(){}

		[Test]
		public void Tokenize_CanLex_LabelDefinition() {
			const string expected_identifier = "THIS_IS_A_LABEL";

			Token expected_identifier_token = new Token(TokenType.Identifier, expected_identifier, null, 1, 1);
			Token expected_delimeter_token = new Token(TokenType.LabelDelimeter, 1, expected_identifier.Length + 1);

			string source = $"{expected_identifier}:";
			
			List<Token> tokens = Lexer.Tokenize(source);
			
			Assert.AreEqual(2, tokens.Count);

			Assert.AreEqual(expected_identifier_token, tokens[0]);
			Assert.AreEqual(expected_delimeter_token, tokens[1]);
		}

		[Test]
		public void Tokenize_CanLex_InstructionAtEndOfFile() { }

		// FIXME: These tests will break if the opcode is also used as a pseudo op

		//[ValueSource(nameof(values))]
		[Test]
		public void Tokenize_CanLex_NInstruction() {
			const Opcode expected_opcode = Opcode.NOP;
			string source = expected_opcode.ToString();

			Token expected_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, 1);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(1, tokens.Count);

			Assert.AreEqual(expected_token, tokens[0]);
		}

		[Test]
		public void Tokenize_CanLex_RInstruction() {
			const Opcode expected_opcode = Opcode.CALL;
			const Register expected_register = Register.R0;

			string source = $"{expected_opcode} {expected_register}";

			Token expected_opcode_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, 1);
			Token expected_register_token = new Token(TokenType.Register, expected_register.ToString(), (int)expected_register, 1, expected_opcode.ToString().Length + 1);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(2, tokens.Count);

			Assert.AreEqual(expected_opcode_token, tokens[0]);
			Assert.AreEqual(expected_register_token, tokens[1]);
		}

		[Test]
		public void Tokenize_CanLex_IInstruction() {
			const Opcode expected_opcode = Opcode.LLI;
			const Register expected_register = Register.R0;
			const UInt32 expected_immediate = 0xffff;

			string source = $"{expected_opcode} {expected_register}, {expected_immediate}";

			int charno = 1;
			Token expected_opcode_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, charno);

			charno += expected_opcode.ToString().Length + 1;
			Token expected_register_token = new Token(TokenType.Register, expected_register.ToString(), (int)expected_register, 1, charno);

			charno += expected_register.ToString().Length;
			Token expected_seperator_token = new Token(TokenType.Seperator, 1, charno);

			charno += 2;
			Token expected_immediate_token = new Token(TokenType.Immediate, expected_immediate.ToString(), expected_immediate, 1, charno);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(4, tokens.Count);

			Assert.AreEqual(expected_opcode_token, tokens[0]);
			Assert.AreEqual(expected_register_token, tokens[1]);
			Assert.AreEqual(expected_seperator_token, tokens[2]);
			Assert.AreEqual(expected_immediate_token, tokens[3]);
		}

		[Test]
		public void Tokenize_CanLex_MInstruction() {
			const Opcode expected_opcode = Opcode.LDW;
			const Register expected_destination_register = Register.R0;
			const Register expected_source_register = Register.R1;
			const UInt32 expected_offset = 0xff;

			string source = $"{expected_opcode} {expected_destination_register}, {expected_source_register}[{expected_offset}]";

			int charno = 1;
			Token expected_opcode_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, 1);

			charno += expected_opcode.ToString().Length + 1;
			Token expected_destination_register_token = new Token(TokenType.Register, expected_destination_register.ToString(), (int)expected_destination_register, 1, charno);

			charno += expected_destination_register.ToString().Length;
			Token expected_seperator_token = new Token(TokenType.Seperator, 1, charno);

			charno += 2;
			Token expected_source_register_token = new Token(TokenType.Register, expected_source_register.ToString(), (int)expected_source_register, 1, charno);

			charno += expected_source_register.ToString().Length;
			Token expected_open_bracket_token = new Token(TokenType.OpenBracket, 1, charno);

			charno += 1;
			Token expected_offset_token = new Token(TokenType.Immediate, expected_offset.ToString(), expected_offset, 1, charno);

			charno += expected_offset_token.ToString().Length;
			Token expected_close_bracket_token = new Token(TokenType.CloseBracket, 1, charno);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(7, tokens.Count);

			Assert.AreEqual(expected_opcode_token, tokens[0]);
			Assert.AreEqual(expected_destination_register_token, tokens[1]);
			Assert.AreEqual(expected_seperator_token, tokens[2]);
			Assert.AreEqual(expected_source_register_token, tokens[3]);
			Assert.AreEqual(expected_open_bracket_token, tokens[4]);
			Assert.AreEqual(expected_offset_token, tokens[5]);
			Assert.AreEqual(expected_close_bracket_token, tokens[6]);
		}

		[Test]
		public void Tokenize_CanLex_DInstruction() {
			const Opcode expected_opcode = Opcode.MOV;
			const Register expected_destination_register = Register.R0;
			const Register expected_source_register = Register.R1;

			string source = $"{expected_opcode} {expected_destination_register}, {expected_source_register}";

			int charno = 1;
			Token expected_opcode_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, charno);

			charno += expected_opcode.ToString().Length + 1;
			Token expected_destination_register_token = new Token(TokenType.Register, expected_destination_register.ToString(), (int)expected_destination_register, 1, charno);

			charno += expected_destination_register.ToString().Length;
			Token expected_seperator_token = new Token(TokenType.Seperator, 1, charno);

			charno += 2;
			Token expected_source_register_token = new Token(TokenType.Register, expected_source_register.ToString(), (int)expected_source_register, 1, charno);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(4, tokens.Count);

			Assert.AreEqual(expected_opcode_token, tokens[0]);
			Assert.AreEqual(expected_destination_register_token, tokens[1]);
			Assert.AreEqual(expected_seperator_token, tokens[2]);
			Assert.AreEqual(expected_source_register_token, tokens[3]);
		}

		[Test]
		public void Tokenize_CanLex_TInstruction() {
			const Opcode expected_opcode = Opcode.MOV;
			const Register expected_destination_register = Register.R0;
			const Register expected_source_register_a = Register.R1;
			const Register expected_source_register_b = Register.R2;

			string source = $"{expected_opcode} {expected_destination_register}, {expected_source_register_a}, {expected_source_register_b}";

			int charno = 1;
			Token expected_opcode_token = new Token(TokenType.Opcode, expected_opcode.ToString(), (int)expected_opcode, 1, charno);

			charno += expected_opcode.ToString().Length + 1;
			Token expected_destination_register_token = new Token(TokenType.Register, expected_destination_register.ToString(), (int)expected_destination_register, 1, charno);

			charno += expected_destination_register.ToString().Length;
			Token expected_seperator_a_token = new Token(TokenType.Seperator, 1, charno);

			charno += 2;
			Token expected_source_register_a_token = new Token(TokenType.Register, expected_source_register_a.ToString(), (int)expected_source_register_a, 1, charno);

			charno += expected_source_register_a.ToString().Length;
			Token expected_seperator_b_token = new Token(TokenType.Seperator, 1, charno);

			charno += 2;
			Token expected_source_register_b_token = new Token(TokenType.Register, expected_source_register_b.ToString(), (int)expected_source_register_b, 1, charno);

			List<Token> tokens = Lexer.Tokenize(source);

			Assert.AreEqual(6, tokens.Count);

			Assert.AreEqual(expected_opcode_token, tokens[0]);
			Assert.AreEqual(expected_destination_register_token, tokens[1]);
			Assert.AreEqual(expected_seperator_a_token, tokens[2]);
			Assert.AreEqual(expected_source_register_a_token, tokens[3]);
			Assert.AreEqual(expected_seperator_b_token, tokens[4]);
			Assert.AreEqual(expected_source_register_b_token, tokens[5]);
		}
	}
}