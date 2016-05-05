using NUnit.Framework;
using Moq;
using FluentAssertions;
using MiniPascalCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerUnitTests
{
    [TestFixture]
    class ParserTests
    {
        ProgramNode excepted;

        [SetUp]
        public void Init()
        {
            excepted = new ProgramNode(0, 0);
            excepted.Identifier = new IdentifierExpr(0, 0, "testprog");
            excepted.Block = new BlockStmt(0, 0);
        }

        [Test]
        public void TestVariableDeclaration()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwVar },
                { TokenType.Identifier, "asd" },
                { TokenType.Colon },
                { TokenType.Identifier, "int" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new VarDeclarationStmt(0, 0);
            declr.Identifiers.Add(new IdentifierExpr(0, 0, "asd"));
            declr.Type = new SimpleType(0, 0, ExprType.Int);
            excepted.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestMultipleIdentifiersVariableDeclaration()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwVar },
                { TokenType.Identifier, "asd" },
                { TokenType.Comma },
                { TokenType.Identifier, "lol" },
                { TokenType.Comma },
                { TokenType.Identifier, "foo" },
                { TokenType.Colon },
                { TokenType.Identifier, "bool" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new VarDeclarationStmt(0, 0);
            declr.Identifiers.Add(new IdentifierExpr(0, 0, "asd"));
            declr.Identifiers.Add(new IdentifierExpr(0, 0, "lol"));
            declr.Identifiers.Add(new IdentifierExpr(0, 0, "foo"));
            declr.Type = new SimpleType(0, 0, ExprType.Bool);
            excepted.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestArrayDeclaration()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwVar },
                { TokenType.Identifier, "asd" },
                { TokenType.Colon },
                { TokenType.KwArray },
                { TokenType.LBracket },
                { TokenType.IntLiteral, "3" },
                { TokenType.RBracket },
                { TokenType.KwOf },
                { TokenType.Identifier, "int" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new VarDeclarationStmt(0, 0);
            declr.Identifiers.Add(new IdentifierExpr(0, 0, "asd"));
            declr.Type = new ArrayType(0, 0, ExprType.Int, 3);
            excepted.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestProcedureDeclaration()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwProcedure },
                { TokenType.Identifier, "proc" },
                { TokenType.LParen },
                { TokenType.Identifier, "par1" },
                { TokenType.Colon },
                { TokenType.Identifier, "real" },
                { TokenType.RParen },
                { TokenType.LineTerm },
                { TokenType.KwBegin },
                { TokenType.Identifier, "writeln" },
                { TokenType.LParen },
                { TokenType.RParen },
                { TokenType.KwEnd }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new ProcedureDeclarationStmt(0, 0);
            declr.Identifier = new IdentifierExpr(0, 0, "proc");
            declr.Parameters = new ParameterList(0, 0);
            declr.Parameters.AddParameter(new IdentifierExpr(0, 0, "par1"), new SimpleType(0, 0, ExprType.Real), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = new IdentifierExpr(0, 0, "writeln");
            declr.ProcedureBlock.Statements.Add(call);
            excepted.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestProcedureDeclarationWithEmptyReturn()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwProcedure },
                { TokenType.Identifier, "proc" },
                { TokenType.LParen },
                { TokenType.Identifier, "par1" },
                { TokenType.Colon },
                { TokenType.Identifier, "real" },
                { TokenType.RParen },
                { TokenType.LineTerm },
                { TokenType.KwBegin },
                { TokenType.KwReturn},
                { TokenType.KwEnd }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new ProcedureDeclarationStmt(0, 0);
            declr.Identifier = new IdentifierExpr(0, 0, "proc");
            declr.Parameters = new ParameterList(0, 0);
            declr.Parameters.AddParameter(new IdentifierExpr(0, 0, "par1"), new SimpleType(0, 0, ExprType.Real), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            declr.ProcedureBlock.Statements.Add(new ReturnStmt(0, 0));
            excepted.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestProcedureDeclarationWithMultipleParameters()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwProcedure },
                { TokenType.Identifier, "proc" },
                { TokenType.LParen },
                { TokenType.Identifier, "par1" },
                { TokenType.Colon },
                { TokenType.Identifier, "real" },
                { TokenType.Comma },
                { TokenType.KwVar },
                { TokenType.Identifier, "par2" },
                { TokenType.Colon },
                { TokenType.Identifier, "string" },
                { TokenType.RParen },
                { TokenType.LineTerm },
                { TokenType.KwBegin },
                { TokenType.Identifier, "writeln" },
                { TokenType.LParen },
                { TokenType.RParen },
                { TokenType.KwEnd }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new ProcedureDeclarationStmt(0, 0);
            declr.Identifier = new IdentifierExpr(0, 0, "proc");
            declr.Parameters = new ParameterList(0, 0);
            declr.Parameters.AddParameter(new IdentifierExpr(0, 0, "par1"), new SimpleType(0, 0, ExprType.Real), false);
            declr.Parameters.AddParameter(new IdentifierExpr(0, 0, "par2"), new SimpleType(0, 0, ExprType.String), true);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = new IdentifierExpr(0, 0, "writeln");
            declr.ProcedureBlock.Statements.Add(call);
            excepted.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestFunctionDeclaration()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwFunction },
                { TokenType.Identifier, "func" },
                { TokenType.LParen },
                { TokenType.Identifier, "par1" },
                { TokenType.Colon },
                { TokenType.Identifier, "int" },
                { TokenType.RParen },
                { TokenType.Colon },
                { TokenType.Identifier, "bool" },
                { TokenType.LineTerm },
                { TokenType.KwBegin },
                { TokenType.KwReturn},
                { TokenType.IntLiteral, "123" },
                { TokenType.KwEnd }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var declr = new FunctionDeclarationStmt(0, 0);
            declr.Identifier = new IdentifierExpr(0, 0, "func");
            declr.ReturnType = new SimpleType(0, 0, ExprType.Bool);
            declr.Parameters = new ParameterList(0, 0);
            declr.Parameters.AddParameter(new IdentifierExpr(0, 0, "par1"), new SimpleType(0, 0, ExprType.Int), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            returnStmt.ReturnExpression = new IntLiteralExpr(0, 0, 123);
            declr.ProcedureBlock.Statements.Add(returnStmt);
            excepted.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestVariableAssigment()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "var1" },
                { TokenType.OpAssignment },
                { TokenType.IntLiteral, "1" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var assign = new AssignmentStmt(0, 0);
            assign.Identifier = new IdentifierExpr(0, 0, "var1");
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            excepted.Block.Statements.Add(assign);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestArrayAssigment()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "var1" },
                { TokenType.LBracket },
                { TokenType.IntLiteral, "5" },
                { TokenType.RBracket },
                { TokenType.OpAssignment },
                { TokenType.StringLiteral, "moi" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var assign = new AssignmentStmt(0, 0);
            var identifier = new ArrayVariableExpr(0, 0);
            identifier.ArrayIdentifier = new IdentifierExpr(0, 0, "var1");
            identifier.SubscriptExpr = new IntLiteralExpr(0, 0, 5);
            assign.Identifier = identifier;
            assign.AssignmentExpr = new StringLiteralExpr(0, 0, "moi");
            excepted.Block.Statements.Add(assign);

            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestCallWithoutArguments()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "func" },
                { TokenType.LParen },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var call = new CallStmt(0, 0);
            call.ProcedureId = new IdentifierExpr(0, 0, "func");
            excepted.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestCallWithOneArgument()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "func" },
                { TokenType.LParen },
                { TokenType.IntLiteral, "2" },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var call = new CallStmt(0, 0);
            call.Arguments = new ArgumentList(0, 0);
            call.Arguments.Arguments.Add(new IntLiteralExpr(0, 0, 2));
            call.ProcedureId = new IdentifierExpr(0, 0, "func");
            excepted.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestCallWithMultipleArguments()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "func" },
                { TokenType.LParen },
                { TokenType.IntLiteral, "2" },
                { TokenType.Comma },
                { TokenType.StringLiteral, "arg" },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var call = new CallStmt(0, 0);
            call.Arguments = new ArgumentList(0, 0);
            call.Arguments.Arguments.Add(new IntLiteralExpr(0, 0, 2));
            call.Arguments.Arguments.Add(new StringLiteralExpr(0, 0, "arg"));
            call.ProcedureId = new IdentifierExpr(0, 0, "func");
            excepted.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestAssert()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwAssert},
                { TokenType.LParen },
                { TokenType.Identifier, "true" },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var assert = new AssertStmt(0, 0);
            assert.AssertExpr = new IdentifierExpr(0, 0, "true");
            excepted.Block.Statements.Add(assert);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestIfWithoutThen()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwIf},
                { TokenType.Identifier, "true" },
                { TokenType.KwThen },
                { TokenType.KwReturn }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var ifStmt = new IfStmt(0, 0);
            ifStmt.TestExpr = new IdentifierExpr(0, 0, "true");
            ifStmt.TrueStatement = new ReturnStmt(0, 0);
            excepted.Block.Statements.Add(ifStmt);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestIfWithThen()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwIf},
                { TokenType.Identifier, "true" },
                { TokenType.KwThen },
                { TokenType.KwReturn },
                { TokenType.KwElse },
                { TokenType.Identifier, "writeln" },
                { TokenType.LParen },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var ifStmt = new IfStmt(0, 0);
            ifStmt.TestExpr = new IdentifierExpr(0, 0, "true");
            ifStmt.TrueStatement = new ReturnStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = new IdentifierExpr(0, 0, "writeln");
            ifStmt.FalseStatement = call;
            excepted.Block.Statements.Add(ifStmt);
            program.ShouldBeEquivalentTo(excepted);
        }

        [Test]
        public void TestWhile()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwWhile},
                { TokenType.Identifier, "true" },
                { TokenType.KwDo },
                { TokenType.KwReturn }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var whileStmt = new WhileStmt(0, 0);
            whileStmt.TestExpr = new IdentifierExpr(0, 0, "true");
            whileStmt.Body = new ReturnStmt(0, 0);
            excepted.Block.Statements.Add(whileStmt);
            program.ShouldBeEquivalentTo(excepted);
        }

        private IScanner CreateMockScanner(TokenList programTokens, bool createProgramBase = true)
        {
            Queue<KeyValuePair<TokenType, string>> tokens;
            if (createProgramBase)
            {
                tokens = new Queue<KeyValuePair<TokenType, string>>(new TokenList()
                    { { TokenType.KwProgram},
                    { TokenType.Identifier, "testprog" },
                    { TokenType.LineTerm },
                    { TokenType.KwBegin }});
                programTokens.ForEach(tokens.Enqueue);
                tokens.Enqueue(new KeyValuePair<TokenType, string>(TokenType.KwEnd, ""));
                tokens.Enqueue(new KeyValuePair<TokenType, string>(TokenType.OpDot, ""));
            }
            else
            {
                tokens = new Queue<KeyValuePair<TokenType, string>>(programTokens);
            }
            Func<Token> nextToken = () =>
            {
                if (tokens.Count > 0)
                {
                    var newToken = tokens.Dequeue();
                    return new Token(newToken.Key, 0, 0, newToken.Value);
                }
                return new Token(TokenType.EOF, 0, 0);
            };
            var mock = new Mock<IScanner>();
            mock.Setup(x => x.GetNextToken()).Returns(nextToken);
            return mock.Object;
        }

        private class TokenList : List<KeyValuePair<TokenType, string>>
        {
            public void Add(TokenType key, string value)
            {
                base.Add(new KeyValuePair<TokenType, string>(key, value));
            }

            public void Add(TokenType key)
            {
                base.Add(new KeyValuePair<TokenType, string>(key, ""));
            }
        }
    }
}
