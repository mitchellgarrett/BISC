using NUnit.Framework;
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
	}
}