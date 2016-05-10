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
        ProgramNode expected;

        [OneTimeSetUp]
        public void Setup()
        {
            AssertionOptions.AssertEquivalencyUsing(options => options.AllowingInfiniteRecursion());
            AssertionOptions.AssertEquivalencyUsing(options => options.RespectingRuntimeTypes());
        }

        [SetUp]
        public void Init()
        {
            expected = new ProgramNode(0, 0);
            expected.Identifier = "testprog";
            expected.Block = new BlockStmt(0, 0);
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
            declr.Identifiers.Add("asd");
            declr.Type = new SimpleType(0, 0, ExprType.Int);
            expected.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifiers.Add("asd");
            declr.Identifiers.Add("lol");
            declr.Identifiers.Add("foo");
            declr.Type = new SimpleType(0, 0, ExprType.Bool);
            expected.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifiers.Add("asd");
            declr.Type = new ArrayType(0, 0, ExprType.Int, 3);
            expected.Block.Statements.Add(declr);
            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifier = "proc";
            declr.AddParameter("par1", new SimpleType(0, 0, ExprType.Real), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "writeln";
            declr.ProcedureBlock.Statements.Add(call);
            expected.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestProcedureDeclarationWithoutParameters()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwProcedure },
                { TokenType.Identifier, "proc" },
                { TokenType.LParen },
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
            declr.Identifier = "proc";
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "writeln";
            declr.ProcedureBlock.Statements.Add(call);
            expected.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifier = "proc";
            declr.AddParameter("par1", new SimpleType(0, 0, ExprType.Real), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            declr.ProcedureBlock.Statements.Add(new ReturnStmt(0, 0));
            expected.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifier = "proc";
            declr.AddParameter("par1", new SimpleType(0, 0, ExprType.Real), false);
            declr.AddParameter("par2", new SimpleType(0, 0, ExprType.String), true);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "writeln";
            declr.ProcedureBlock.Statements.Add(call);
            expected.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(expected);
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
            declr.Identifier = "func";
            declr.ReturnType = new SimpleType(0, 0, ExprType.Bool);
            declr.AddParameter("par1", new SimpleType(0, 0, ExprType.Int), false);
            declr.ProcedureBlock = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            returnStmt.ReturnExpression = new IntLiteralExpr(0, 0, 123);
            declr.ProcedureBlock.Statements.Add(returnStmt);
            expected.Block.Statements.Add(declr);

            program.ShouldBeEquivalentTo(expected);
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
            assign.Variable = new VariableExpr(0, 0, "var1");
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            expected.Block.Statements.Add(assign);

            program.ShouldBeEquivalentTo(expected);
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
            identifier.ArrayIdentifier = "var1";
            identifier.SubscriptExpr = new IntLiteralExpr(0, 0, 5);
            assign.Variable = identifier;
            assign.AssignmentExpr = new StringLiteralExpr(0, 0, "moi");
            expected.Block.Statements.Add(assign);

            program.ShouldBeEquivalentTo(expected);
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
            call.ProcedureId = "func";
            expected.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(expected);
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
            call.Arguments = new List<Expression>();
            call.Arguments.Add(new IntLiteralExpr(0, 0, 2));
            call.ProcedureId = "func";
            expected.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(expected);
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
            call.Arguments = new List<Expression>();
            call.Arguments.Add(new IntLiteralExpr(0, 0, 2));
            call.Arguments.Add(new StringLiteralExpr(0, 0, "arg"));
            call.ProcedureId = "func";
            expected.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(expected);
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
            assert.AssertExpr = new VariableExpr(0, 0, "true");
            expected.Block.Statements.Add(assert);
            program.ShouldBeEquivalentTo(expected);
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
            ifStmt.TestExpr = new VariableExpr(0, 0, "true");
            ifStmt.TrueStatement = new ReturnStmt(0, 0);
            expected.Block.Statements.Add(ifStmt);
            program.ShouldBeEquivalentTo(expected);
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
            ifStmt.TestExpr = new VariableExpr(0, 0, "true");
            ifStmt.TrueStatement = new ReturnStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "writeln";
            ifStmt.FalseStatement = call;
            expected.Block.Statements.Add(ifStmt);
            program.ShouldBeEquivalentTo(expected);
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
            whileStmt.TestExpr = new VariableExpr(0, 0, "true");
            whileStmt.Body = new ReturnStmt(0, 0);
            expected.Block.Statements.Add(whileStmt);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestExpressionMinusSign()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.Minus },
                { TokenType.IntLiteral, "5" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr = new IntLiteralExpr(0, 0, 5);
            expr.Sign = ExprSign.Minus;
            var assignment = new AssignmentStmt(0, 0);
            assignment.Variable = new VariableExpr(0, 0, "x");
            assignment.AssignmentExpr = expr;
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestRealLiteral()
        {
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.RealLiteral, "2.4e3" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr = new IntLiteralExpr(0, 0, 5);
            expr.Sign = ExprSign.Minus;
            var assignment = new AssignmentStmt(0, 0);
            assignment.Variable = new VariableExpr(0, 0, "x");
            assignment.AssignmentExpr = new RealLiteralExpr(0, 0, 2400);
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestMultipleAdditions()
        {
            // 1 + 2 + 3
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.IntLiteral, "1" },
                { TokenType.Plus },
                { TokenType.IntLiteral, "2" },
                { TokenType.Minus },
                { TokenType.IntLiteral, "3" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr1 = new BinaryExpr(0, 0);
            expr1.Left = new IntLiteralExpr(0, 0, 1);
            expr1.Right = new IntLiteralExpr(0, 0, 2);
            expr1.Op = Operator.Plus;
            var expr2 = new BinaryExpr(0, 0);
            expr2.Left = expr1;
            expr2.Right = new IntLiteralExpr(0, 0, 3);
            expr2.Op = Operator.Minus;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = expr2;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestMultipleMultiplications()
        {
            // 1 + 2 + 3
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.IntLiteral, "1" },
                { TokenType.OpMultiply },
                { TokenType.IntLiteral, "2" },
                { TokenType.OpDivide },
                { TokenType.IntLiteral, "3" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr1 = new BinaryExpr(0, 0);
            expr1.Left = new IntLiteralExpr(0, 0, 1);
            expr1.Right = new IntLiteralExpr(0, 0, 2);
            expr1.Op = Operator.Times;
            var expr2 = new BinaryExpr(0, 0);
            expr2.Left = expr1;
            expr2.Right = new IntLiteralExpr(0, 0, 3);
            expr2.Op = Operator.Divide;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = expr2;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestAddAndMultiplicationPrecedence()
        {
            // 1 + 2 * 3
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.IntLiteral, "1" },
                { TokenType.Plus },
                { TokenType.IntLiteral, "2" },
                { TokenType.OpMultiply },
                { TokenType.IntLiteral, "3" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr1 = new BinaryExpr(0, 0);
            expr1.Left = new IntLiteralExpr(0, 0, 2);
            expr1.Right = new IntLiteralExpr(0, 0, 3);
            expr1.Op = Operator.Times;
            var expr2 = new BinaryExpr(0, 0);
            expr2.Left = new IntLiteralExpr(0, 0, 1);
            expr2.Right = expr1;
            expr2.Op = Operator.Plus;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = expr2;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestExpressionInParenthesis()
        {
            // (1 + 2) * 3
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.LParen },
                { TokenType.IntLiteral, "1" },
                { TokenType.Plus },
                { TokenType.IntLiteral, "2" },
                { TokenType.RParen },
                { TokenType.OpMultiply },
                { TokenType.IntLiteral, "3" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr1 = new BinaryExpr(0, 0);
            expr1.Left = new IntLiteralExpr(0, 0, 1);
            expr1.Right = new IntLiteralExpr(0, 0, 2);
            expr1.Op = Operator.Plus;
            var expr2 = new BinaryExpr(0, 0);
            expr2.Left = expr1;
            expr2.Right = new IntLiteralExpr(0, 0, 3);
            expr2.Op = Operator.Times;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = expr2;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestRelationalExpression()
        {
            // 1 + 2 < 3 * 4
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.IntLiteral, "1" },
                { TokenType.Plus },
                { TokenType.IntLiteral, "2" },
                { TokenType.OpLess },
                { TokenType.IntLiteral, "3" },
                { TokenType.OpMultiply },
                { TokenType.IntLiteral, "4" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var leftExpr = new BinaryExpr(0, 0);
            leftExpr.Left = new IntLiteralExpr(0, 0, 1);
            leftExpr.Right = new IntLiteralExpr(0, 0, 2);
            leftExpr.Op = Operator.Plus;
            var rightExpr = new BinaryExpr(0, 0);
            rightExpr.Left = new IntLiteralExpr(0, 0, 3);
            rightExpr.Right = new IntLiteralExpr(0, 0, 4);
            rightExpr.Op = Operator.Times;
            var comp = new BinaryExpr(0, 0);
            comp.Left = leftExpr;
            comp.Right = rightExpr;
            comp.Op = Operator.Less;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = comp;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestCallExpr()
        {
            // func("asd")
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.Identifier, "func" },
                { TokenType.LParen },
                { TokenType.StringLiteral, "asd" },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var call = new CallExpr(0, 0);
            call.Arguments = new List<Expression>();
            call.Arguments.Add(new StringLiteralExpr(0, 0, "asd"));
            call.CalleeId = "func";
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = call;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestNotExpr()
        {
            // not (1 > 2)
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.OpNot },
                { TokenType.LParen },
                { TokenType.IntLiteral, "1" },
                { TokenType.OpMore },
                { TokenType.IntLiteral, "2" },
                { TokenType.RParen }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var comp = new BinaryExpr(0, 0);
            comp.Left = new IntLiteralExpr(0, 0, 1);
            comp.Right = new IntLiteralExpr(0, 0, 2);
            comp.Op = Operator.More;
            var unary = new UnaryExpr(0, 0);
            unary.Op = Operator.Not;
            unary.Expr = comp;
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = unary;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestMemberAccessExpr()
        {
            // not (1 > 2)
            var programSource = new TokenList()
            {
                { TokenType.Identifier, "x" },
                { TokenType. OpAssignment },
                { TokenType.Identifier, "arr" },
                { TokenType.OpDot },
                { TokenType.Identifier, "size" }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var expr = new MemberAccessExpr(0, 0);
            expr.AccessedExpr = new VariableExpr(0, 0, "arr");
            expr.MemberId = "size";
            var assignment = new AssignmentStmt(0, 0);
            assignment.AssignmentExpr = expr;
            assignment.Variable = new VariableExpr(0, 0, "x");
            expected.Block.Statements.Add(assignment);
            program.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void TestMultipleStatements()
        {
            var programSource = new TokenList()
            {
                { TokenType.KwAssert },
                { TokenType.Identifier, "true" },
                { TokenType.LineTerm },
                { TokenType.Identifier, "writeln" },
                { TokenType.LParen },
                { TokenType.RParen },
                { TokenType.LineTerm }
            };
            Parser parser = new Parser(CreateMockScanner(programSource), new ErrorHandler());
            ProgramNode program = parser.Parse();

            var assert = new AssertStmt(0, 0);
            assert.AssertExpr = new VariableExpr(0, 0, "true");
            var call = new CallStmt(0, 0);
            call.ProcedureId = "writeln";
            expected.Block.Statements.Add(assert);
            expected.Block.Statements.Add(call);
            program.ShouldBeEquivalentTo(expected);
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
