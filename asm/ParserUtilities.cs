using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class Parser {
		
		static bool Match(Token token, TokenType expected_type) {
			return token.Type == expected_type;
		}

		static void Expect(Token token, TokenType expected_type, string message) {
			if (!Match(token, expected_type)) Fail(token, expected_type, message);
		}

		static void Fail(Token token, TokenType expected_type, string message) {
			throw new SyntaxErrorException($"\u001b[1m{file_name}:{token.LineNo}:{token.CharNo}: \u001b[31merror:\u001b[39m {message}\u001b[0m");
		}

		static Token Dequeue(this LinkedList<Token> tokens) {
			Token node = tokens.First.Value;
			tokens.RemoveFirst();
			return node;
		}

		static Token Peek(this LinkedList<Token> tokens) {
			return tokens.First.Value;
		}
	}
}