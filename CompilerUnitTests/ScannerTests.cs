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

        /*
        Basic tests for all token types
        */

        [Test]
        public void TestProgramToken()
        {
            Scanner lexer = CreateStringLexer("program");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwProgram));
        }

        [Test]
        public void TestProcedureToken()
        {
            Scanner lexer = CreateStringLexer("procedure");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwProcedure));
        }

        [Test]
        public void TestFunctionToken()
        {
            Scanner lexer = CreateStringLexer("function");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwFunction));
        }

        [Test]
        public void TestArrayToken()
        {
            Scanner lexer = CreateStringLexer("array");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwArray));
        }

        [Test]
        public void TestBeginToken()
        {
            Scanner lexer = CreateStringLexer("begin");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwBegin));
        }

        [Test]
        public void TestElseToken()
        {
            Scanner lexer = CreateStringLexer("else");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwElse));
        }

        [Test]
        public void TestOfToken()
        {
            Scanner lexer = CreateStringLexer("of");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwOf));
        }

        [Test]
        public void TestReturnToken()
        {
            Scanner lexer = CreateStringLexer("return");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwReturn));
        }

        [Test]
        public void TestThenToken()
        {
            Scanner lexer = CreateStringLexer("then");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwThen));
        }

        [Test]
        public void TestVarKwToken()
        {
            Scanner lexer = CreateStringLexer("var");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwVar));
        }

        [Test]
        public void TestWhileKwToken()
        {
            Scanner lexer = CreateStringLexer("while");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwWhile));
        }

        [Test]
        public void TestEndKwToken()
        {
            Scanner lexer = CreateStringLexer("end");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwEnd));
        }

        [Test]
        public void TestDoKwToken()
        {
            Scanner lexer = CreateStringLexer("do");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwDo));
        }

        [Test]
        public void TestAssertKwToken()
        {
            Scanner lexer = CreateStringLexer("assert");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwAssert));
        }

        [Test]
        public void TestIdentifierToken()
        {
            Scanner lexer = CreateStringLexer("Some1_var");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.Identifier));
            Assert.That(token.Content, Is.EqualTo("Some1_var"));
        }

        [Test]
        public void TestLParenToken()
        {
            Scanner lexer = CreateStringLexer("(");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.LParen));
        }

        [Test]
        public void TestRParenToken()
        {
            Scanner lexer = CreateStringLexer(")");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RParen));
        }

        [Test]
        public void TestLBracketToken()
        {
            Scanner lexer = CreateStringLexer("[");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.LBracket));
        }

        [Test]
        public void TestRBracketToken()
        {
            Scanner lexer = CreateStringLexer("]");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RBracket));
        }

        [Test]
        public void TestOpAssignmentToken()
        {
            Scanner lexer = CreateStringLexer(":=");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpAssignment));
        }

        [Test]
        public void TestColonToken()
        {
            Scanner lexer = CreateStringLexer(":");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.Colon));
        }

        [Test]
        public void TestCommaToken()
        {
            Scanner lexer = CreateStringLexer(",");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.Comma));
        }

        [Test]
        public void TestDotToken()
        {
            Scanner lexer = CreateStringLexer(".");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpDot));
        }

        [Test]
        public void TestMinusToken()
        {
            Scanner lexer = CreateStringLexer("-");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.Minus));
        }

        [Test]
        public void TestPlusToken()
        {
            Scanner lexer = CreateStringLexer("+");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.Plus));
        }

        [Test]
        public void TestOpMultiplyToken()
        {
            Scanner lexer = CreateStringLexer("*");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpMultiply));
        }

        [Test]
        public void TestOpDivideToken()
        {
            Scanner lexer = CreateStringLexer("/");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpDivide));
        }

        [Test]
        public void TestModToken()
        {
            Scanner lexer = CreateStringLexer("%");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpModulus));
        }

        [Test]
        public void TestOpLessToken()
        {
            Scanner lexer = CreateStringLexer("<");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpLess));
        }

        [Test]
        public void TestMoreToken()
        {
            Scanner lexer = CreateStringLexer(">");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpMore));
        }

        [Test]
        public void TestLessOrEqualsToken()
        {
            Scanner lexer = CreateStringLexer("<=");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpLessOrEquals));
        }

        [Test]
        public void TestMoreOrEqualsToken()
        {
            Scanner lexer = CreateStringLexer(">=");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpMoreOrEquals));
        }

        [Test]
        public void TestNotEqualsToken()
        {
            Scanner lexer = CreateStringLexer("<>");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpNotEquals));
        }

        [Test]
        public void TestOpEqualsToken()
        {
            Scanner lexer = CreateStringLexer("=");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpEquals));
        }

        [Test]
        public void TestOpAndToken()
        {
            Scanner lexer = CreateStringLexer("and");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpAnd));
        }

        [Test]
        public void TestOrToken()
        {
            Scanner lexer = CreateStringLexer("or");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpOr));
        }

        [Test]
        public void TestTerminatorToken()
        {
            Scanner lexer = CreateStringLexer(";");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.LineTerm));
        }

        [Test]
        public void TestIntLiteralToken()
        {
            Scanner lexer = CreateStringLexer("123");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.IntLiteral));
            Assert.That(token.Content, Is.EqualTo("123"));
        }

        [Test]
        public void TestRealLiteraTokenlWithoutExponent()
        {
            Scanner lexer = CreateStringLexer("1.23");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(token.Content, Is.EqualTo("1.23"));
        }

        [Test]
        public void TestRealLiteralTokenWithExponentWithoutSign()
        {
            Scanner lexer = CreateStringLexer("1.2e3");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(token.Content, Is.EqualTo("1.2e3"));
        }

        [Test]
        public void TestRealLiteralTokenWithExponentWithPlusSign()
        {
            Scanner lexer = CreateStringLexer("15.0e+3");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(token.Content, Is.EqualTo("15.0e+3"));
        }

        [Test]
        public void TestRealLiteralTokenWithExponentWithMinusSign()
        {
            Scanner lexer = CreateStringLexer("0.59e-20");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(token.Content, Is.EqualTo("0.59e-20"));
        }

        [Test]
        public void TestStringToken()
        {
            Scanner lexer = CreateStringLexer("\"some string\"");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(token.Content, Is.EqualTo("some string"));
        }

        /*
        Advanced tests
        */

        [Test]
        public void TestMultipleTokensWithoutSpaces()
        {
            Scanner lexer = CreateStringLexer("var:=(123\"lol\"asd");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.OpAssignment));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.LParen));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.IntLiteral));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.StringLiteral));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.Identifier));
        }

        [Test]
        public void TestOverlappingTokens()
        {
            Scanner lexer = CreateStringLexer("::==");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.Colon));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.OpAssignment));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.OpEquals));
        }

        [Test]
        public void TestMultipleTokensOnSameLine()
        {
            Scanner lexer = CreateStringLexer("( := var");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.LParen));
            Assert.That(token.Column, Is.EqualTo(1));
            Assert.That(token.Line, Is.EqualTo(1));
            token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpAssignment));
            Assert.That(token.Column, Is.EqualTo(3));
            Assert.That(token.Line, Is.EqualTo(1));
            token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(token.Column, Is.EqualTo(6));
            Assert.That(token.Line, Is.EqualTo(1));
        }

        [Test]
        public void TestTokensOnMultipleLines()
        {
            Scanner lexer = CreateStringLexer("=\nassert var");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.OpEquals));
            Assert.That(token.Column, Is.EqualTo(1));
            Assert.That(token.Line, Is.EqualTo(1));
            token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwAssert));
            Assert.That(token.Column, Is.EqualTo(1));
            Assert.That(token.Line, Is.EqualTo(2));
            token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(token.Column, Is.EqualTo(8));
            Assert.That(token.Line, Is.EqualTo(2));
        }

        [Test]
        public void TestSkippingWhitespace()
        {
            Scanner lexer = CreateStringLexer("begin \n\tvar");
            Token token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwBegin));
            Assert.That(token.Column, Is.EqualTo(1));
            Assert.That(token.Line, Is.EqualTo(1));
            token = lexer.GetNextToken();
            Assert.That(token.Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(token.Column, Is.EqualTo(2));
            Assert.That(token.Line, Is.EqualTo(2));
        }

        [Test]
        public void TestStringEscapes()
        {
            Scanner lexer = CreateStringLexer("\"asd \\\"\\n\\t\\\\\"");
            Assert.That(lexer.GetNextToken().Content, Is.EqualTo("asd \"\n\t\\"));
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

        [Test]
        public void TestNoDigitsAfterExponent()
        {
            Scanner lexer = CreateStringLexer("1.2e");
            Token token = lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: Expected digits while reading exponent at line 1 column 4"));
            Assert.That(token.Content, Is.EqualTo("1.2"));
        }

        [Test]
        public void TestNoDigitsAfterExponentSign()
        {
            Scanner lexer = CreateStringLexer("15.02e-");
            Token token = lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: Expected digits while reading exponent at line 1 column 7"));
            Assert.That(token.Content, Is.EqualTo("15.02"));
        }

        [Test]
        public void TestBadExponentDoesNotEatNextToken()
        {
            Scanner lexer = CreateStringLexer("15.02e;2.4e-;");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.LineTerm));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.RealLiteral));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.LineTerm));
        }

        /*
        Tests for comments
        */

        [Test]
        public void Scanner_WithSingleLineComment_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var//int\nend");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwEnd));
        }

        [Test]
        public void Scanner_WithSingleLineCommentAndWhitespace_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var //int \n \n  while");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwWhile));
        }

        [Test]
        public void Scanner_WithSingleLineCommentAtBeginningOfLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("//int\ndo");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwDo));
        }

        [Test]
        public void Scanner_WithMultipleSingleLineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var //int//123\n  //456\nwhile");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwWhile));
        }

        [Test]
        public void Scanner_WithMultilineCommentOnOneLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int*/\nend");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwEnd));
        }

        [Test]
        public void Scanner_WithMultilineCommentOnMultipleLines_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int\n123\nabc*/\nprogram");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwProgram));
        }

        [Test]
        public void Scanner_WithNestedMultilineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int/*123*/abc*/\nthen");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwThen));
        }

        [Test]
        public void Scanner_WithMultilineCommentAndWhitespace_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var \n/*int*/ \n \n  procedure");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwProcedure));
        }

        [Test]
        public void Scanner_WithMultilineCommentEndingAtFileEnd_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var/*int\nstring*/");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.EOF));
        }

        [Test]
        public void Scanner_WithMultilineCommentAtBeginningOfLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("/*int*/return");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwReturn));
        }

        [Test]
        public void Scanner_WithMultilineCommentBetweenCodeOnSameLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* int */ of");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwOf));
        }

        [Test]
        public void Scanner_WithUnclosedMultilineComment_ThrowsException()
        {
            Scanner lexer = CreateStringLexer("var/*int\nstring");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            lexer.GetNextToken();
            string errorMessage = Errors.GetErrors()[0].ToString();
            Assert.That(errorMessage, Is.EqualTo("LexicalError: EOF while scanning comment beginning at line 1 column 4"));
        }

        [Test]
        public void Scanner_WithSingleAndMultilineComments_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* int */ //123\n/*321*/while");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwWhile));
        }

        [Test]
        public void Scanner_WithSingleLineCommentInsideMultiline_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var /* //int */var");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
        }

        [Test]
        public void Scanner_WithMultilineCommentInsideSingleLine_SkipsCommentedCode()
        {
            Scanner lexer = CreateStringLexer("var ///*123\nreturn");
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwVar));
            Assert.That(lexer.GetNextToken().Type, Is.EqualTo(TokenType.KwReturn));
        }
    }
}
