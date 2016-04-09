using NUnit.Framework;
using MiniPascalCompiler;

namespace CompilerUnitTests
{
    [TestFixture]
    public class ScannerTests
    {
        ErrorHandler Errors;

        [SetUp]
        public void Init()
        {
            Errors = new ErrorHandler();
        }

        public Scanner CreateStringLexer(string source)
        {
            SourceReader reader = new SourceReader(new System.IO.StringReader(source));
            return new Scanner(reader, Errors);
        }

        [Test]
        public void TestVarKwToken()
        {
            Scanner lexer = CreateStringLexer("var");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwVar, token.Type);
        }

        [Test]
        public void TestWhileKwToken()
        {
            Scanner lexer = CreateStringLexer("while");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwWhile, token.Type);
        }

        [Test]
        public void TestEndKwToken()
        {
            Scanner lexer = CreateStringLexer("end");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwEnd, token.Type);
        }

        [Test]
        public void TestDoKwToken()
        {
            Scanner lexer = CreateStringLexer("do");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwDo, token.Type);
        }

        [Test]
        public void TestAssertKwToken()
        {
            Scanner lexer = CreateStringLexer("assert");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwAssert, token.Type);
        }

        [Test]
        public void TestIdentifierToken()
        {
            Scanner lexer = CreateStringLexer("Some1_var");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.Identifier, token.Type);
            Assert.AreEqual("Some1_var", token.Content);
        }

        [Test]
        public void TestLParenToken()
        {
            Scanner lexer = CreateStringLexer("(");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.LParen, token.Type);
        }

        [Test]
        public void TestRParenToken()
        {
            Scanner lexer = CreateStringLexer(")");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.RParen, token.Type);
        }

        [Test]
        public void TestNumberToken()
        {
            Scanner lexer = CreateStringLexer("123");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.IntLiteral, token.Type);
            Assert.AreEqual("123", token.Content);
        }

        [Test]
        public void TestOpAssignmentToken()
        {
            Scanner lexer = CreateStringLexer(":=");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpAssignment, token.Type);
        }

        [Test]
        public void TestColonToken()
        {
            Scanner lexer = CreateStringLexer(":");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.Colon, token.Type);
        }

        [Test]
        public void TestOpMinusToken()
        {
            Scanner lexer = CreateStringLexer("-");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.Minus, token.Type);
        }

        [Test]
        public void TestOpPlusToken()
        {
            Scanner lexer = CreateStringLexer("+");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.Plus, token.Type);
        }

        [Test]
        public void TestOpMultiplyToken()
        {
            Scanner lexer = CreateStringLexer("*");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpMultiply, token.Type);
        }

        [Test]
        public void TestOpDivideToken()
        {
            Scanner lexer = CreateStringLexer("/");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpDivide, token.Type);
        }

        [Test]
        public void TestOpLessToken()
        {
            Scanner lexer = CreateStringLexer("<");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpLess, token.Type);
        }

        [Test]
        public void TestOpEqualsToken()
        {
            Scanner lexer = CreateStringLexer("=");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpEquals, token.Type);
        }

        [Test]
        public void TestOpAndToken()
        {
            Scanner lexer = CreateStringLexer("and");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpAnd, token.Type);
        }

        [Test]
        public void TestTerminatorToken()
        {
            Scanner lexer = CreateStringLexer(";");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.LineTerm, token.Type);
        }

        [Test]
        public void TestStringToken()
        {
            Scanner lexer = CreateStringLexer("\"some string\"");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.StringLiteral, token.Type);
            Assert.AreEqual("some string", token.Content);
        }

        /*
        Advanced tests
        */

        [Test]
        public void TestMultipleTokensWithoutSpaces()
        {
            Scanner lexer = CreateStringLexer("var:=(123\"lol\"asd");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.OpAssignment, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.LParen, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.IntLiteral, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.StringLiteral, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.Identifier, lexer.GetNextToken().Type);
        }

        [Test]
        public void TestOverlappingTokens()
        {
            Scanner lexer = CreateStringLexer("::==");
            Assert.AreEqual(TokenType.Colon, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.OpAssignment, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.OpEquals, lexer.GetNextToken().Type);
        }

        [Test]
        public void TestMultipleTokensOnSameLine()
        {
            Scanner lexer = CreateStringLexer("( := var");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.LParen, token.Type);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(1, token.Line);
            token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpAssignment, token.Type);
            Assert.AreEqual(3, token.Column);
            Assert.AreEqual(1, token.Line);
            token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwVar, token.Type);
            Assert.AreEqual(6, token.Column);
            Assert.AreEqual(1, token.Line);
        }

        [Test]
        public void TestTokensOnMultipleLines()
        {
            Scanner lexer = CreateStringLexer("=\nassert var");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.OpEquals, token.Type);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(1, token.Line);
            token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwAssert, token.Type);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(2, token.Line);
            token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwVar, token.Type);
            Assert.AreEqual(8, token.Column);
            Assert.AreEqual(2, token.Line);
        }

        [Test]
        public void TestSkippingWhitespace()
        {
            Scanner lexer = CreateStringLexer("begin \n\tvar");
            Token token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwBegin, token.Type);
            Assert.AreEqual(1, token.Column);
            Assert.AreEqual(1, token.Line);
            token = lexer.GetNextToken();
            Assert.AreEqual(TokenType.KwVar, token.Type);
            Assert.AreEqual(2, token.Column);
            Assert.AreEqual(2, token.Line);
        }

        [Test]
        public void TestStringEscapes()
        {
            Scanner lexer = CreateStringLexer("\"asd \\\"\\n\\t\\\\\"");
            Assert.AreEqual("asd \"\n\t\\", lexer.GetNextToken().Content);
        }

        [Test]
        public void TestUnknownStringEscape()
        {
            Scanner lexer = CreateStringLexer("\"asd \\a\"");
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: Unrecognized escape sequence '\\a' at line 1 column 7"));
        }

        [Test]
        public void TestUnclosedString()
        {
            Scanner lexer = CreateStringLexer("\"asd\n");
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: EOL while scanning string literal at line 1 column 5"));
        }

        [Test]
        public void TestEmptyUnclosedString()
        {
            Scanner lexer = CreateStringLexer("\"");
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: EOL while scanning string literal at line 1 column 1"));
        }

        [Test]
        public void TestUnknownToken()
        {
            Scanner lexer = CreateStringLexer("@");
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: Unknown token '@' at line 1 column 1"));
        }

        [Test]
        public void TestTokenAfterUnknownToken()
        {
            Scanner lexer = CreateStringLexer("@var");
            Token token = lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: Unknown token '@' at line 1 column 1"));
            Assert.That(token.Type, Is.EqualTo(TokenType.KwVar));
        }

        /*
        Tests for comments
        */

        [Test]
        public void Scanner_WithSingleLineComment_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var//int\nend");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwEnd, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithSingleLineCommentAndWhitespace_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var //int \n \n  while");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwWhile, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithSingleLineCommentAtBeginningOfLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("//int\ndo");
            Assert.AreEqual(TokenType.KwDo, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultipleSingleLineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var //int//123\n  //456\nwhile");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwWhile, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentOnOneLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int*/\nend");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwEnd, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentOnMultipleLines_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int\n123\nabc*/\nprogram");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwProgram, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithNestedMultilineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int/*123*/abc*/\nthen");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwThen, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentAndWhitespace_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var \n/*int*/ \n \n  procedure");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwProcedure, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentEndingAtFileEnd_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int\nstring*/");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.EOF, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentAtBeginningOfLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("/*int*/return");
            Assert.AreEqual(TokenType.KwReturn, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentBetweenCodeOnSameLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* int */ of");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwOf, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithUnclosedMultilineComment_ThrowsException()
        {
            Scanner lexer = CreateStringLexer("var/*int\nstring");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: EOF while scanning comment beginning at line 1 column 4"));
        }

        [Test]
        public void Scanner_WithSingleAndMultilineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* int */ //123\n/*321*/while");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwWhile, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithSingleLineCommentInsideMultiline_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* //int */var");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
        }

        [Test]
        public void Scanner_WithMultilineCommentInsideSingleLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var ///*123\nreturn");
            Assert.AreEqual(TokenType.KwVar, lexer.GetNextToken().Type);
            Assert.AreEqual(TokenType.KwReturn, lexer.GetNextToken().Type);
        }
    }
}
