using NUnit.Framework;
using Moq;
using FluentAssertions;
using MiniPascalCompiler;

namespace CompilerUnitTests
{
    [TestFixture]
    class SemanticAnalyzerTests
    {
        private ErrorHandler errors;

        [SetUp]
        public void Init()
        {
            errors = new ErrorHandler();
        }

        // Testing correct programs that shouldn't produce errors

        [Test]
        public void TestEmptyProgram()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            AssertNoErrors(program);
        }

        [Test]
        public void TestVariableDeclaration()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            program.Block.Statements.Add(declaration);
            AssertNoErrors(program);
        }

        [Test]
        public void TestRedeclarationInInnerScope()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration1 = CreateVarDeclaration("var1");
            var declaration2 = CreateVarDeclaration("var2");
            var innerBlock = new BlockStmt(0, 0);
            innerBlock.Statements.Add(declaration2);
            program.Block.Statements.Add(declaration1);
            program.Block.Statements.Add(innerBlock);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAssignSameType()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAssignToVariableDeclaredInOuterScope()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = new VariableExpr(0, 0, "var1");
            var innerBlock = new BlockStmt(0, 0);
            innerBlock.Statements.Add(assign);
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(innerBlock);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAssignIntInReal()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new SimpleType(0, 0, ExprType.Real));
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAssignInArrayElement()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new ArrayType(0, 0, ExprType.Int));
            var assign = new AssignmentStmt(0, 0);
            var array = new ArrayVariableExpr(0, 0);
            array.ArrayIdentifier = "var1";
            array.SubscriptExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = array;
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAccessArraySize()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new ArrayType(0, 0, ExprType.Int));
            var declaration2 = CreateVarDeclaration("var2");
            var assign = new AssignmentStmt(0, 0);
            var member = new MemberAccessExpr(0, 0);
            member.AccessedExpr = new VariableExpr(0, 0, "var1");
            member.MemberId = "size";
            assign.Variable = new VariableExpr(0, 0, "var2");
            assign.AssignmentExpr = member;
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(declaration2);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestAssignArrayInArray()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration1 = CreateVarDeclaration("var1");
            var declaration2 = CreateVarDeclaration("var2");
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            assign.AssignmentExpr = new VariableExpr(0, 0, "var2");
            program.Block.Statements.Add(declaration1);
            program.Block.Statements.Add(declaration2);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestOverridingPredefinedVariable()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("true");
            program.Block.Statements.Add(declaration);
            AssertNoErrors(program);
        }

        [Test]
        public void TestEmptyReturnInProgram()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            program.Block.Statements.Add(new ReturnStmt(0, 0));
            AssertNoErrors(program);
        }

        [Test]
        public void TestEmptyReturnInProcedure()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var procedure = new ProcedureDeclarationStmt(0, 0);
            procedure.Identifier = "proc";
            procedure.ProcedureBlock = new BlockStmt(0, 0);
            procedure.ProcedureBlock.Statements.Add(new ReturnStmt(0, 0));
            program.Block.Statements.Add(procedure);
            AssertNoErrors(program);
        }

        [Test]
        public void TestBooleanAssert()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assert = new AssertStmt(0, 0);
            assert.AssertExpr = new VariableExpr(0, 0, "true");
            program.Block.Statements.Add(assert);
            AssertNoErrors(program);
        }

        [Test]
        public void TestBooleanIfTest()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var ifStmt = new IfStmt(0, 0);
            ifStmt.TestExpr = new VariableExpr(0, 0, "true");
            ifStmt.TrueStatement = new BlockStmt(0, 0);
            program.Block.Statements.Add(ifStmt);
            AssertNoErrors(program);
        }

        [Test]
        public void TestBooleanWhileCondition()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var whileStmt = new WhileStmt(0, 0);
            whileStmt.TestExpr = new VariableExpr(0, 0, "true");
            whileStmt.Body = new BlockStmt(0, 0);
            program.Block.Statements.Add(whileStmt);
            AssertNoErrors(program);
        }

        [Test]
        public void TestCorrectReturnInFuction()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var funcBlock = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            returnStmt.ReturnExpression = new IntLiteralExpr(0, 0, 0);
            funcBlock.Statements.Add(returnStmt);
            program.Block.Statements.Add(CreateFunction("func", funcBlock));
            AssertNoErrors(program);
        }

        [Test]
        public void TestCorrectCallArguments()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.Arguments.Add(new IntLiteralExpr(0, 0, 1));
            call.ProcedureId = "func";
            program.Block.Statements.Add(CreateFunction("func"));
            program.Block.Statements.Add(call);
            AssertNoErrors(program);
        }

        [Test]
        public void TestCallInExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            var call = new CallExpr(0, 0);
            call.Arguments.Add(new IntLiteralExpr(0, 0, 1));
            call.CalleeId = "func";
            assign.AssignmentExpr = call;
            program.Block.Statements.Add(CreateFunction("func"));
            program.Block.Statements.Add(CreateVarDeclaration("var1"));
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestFunctionParametersAreAccessible()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var funcBlock = new BlockStmt(0, 0);
            var ret = new ReturnStmt(0, 0);
            ret.ReturnExpression = new VariableExpr(0, 0, "arg1");
            var function = CreateFunction("func", funcBlock);
            program.Block.Statements.Add(function);
            AssertNoErrors(program);
        }

        [Test]
        public void TestCorrectBinaryExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new SimpleType(0, 0, ExprType.Bool));
            var expr = new BinaryExpr(0, 0);
            expr.Left = new IntLiteralExpr(0, 0, 1);
            expr.Right = new RealLiteralExpr(0, 0, (float)3.4);
            expr.Op = Operator.Less;
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = expr;
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        [Test]
        public void TestCorrectUnaryExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new SimpleType(0, 0, ExprType.Bool));
            var expr = new UnaryExpr(0, 0);
            expr.Expr = new VariableExpr(0, 0, "true");
            expr.Op = Operator.Not;
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = expr;
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertNoErrors(program);
        }

        // Testing semantic errors

        [Test]
        public void TestAssignBeforeDeclaration()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Undeclared variable 'var1'");
        }

        [Test]
        public void TestAssignToVariableDeclaredInInnerScope()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            var innerBlock = new BlockStmt(0, 0);
            innerBlock.Statements.Add(declaration);
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(innerBlock);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Undeclared variable 'var1'");
        }

        [Test]
        public void TestDeclaringAlreadyDeclared()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration1 = CreateVarDeclaration("var1");
            var declaration2 = CreateVarDeclaration("var1");
            program.Block.Statements.Add(declaration1);
            program.Block.Statements.Add(declaration2);
            AssertErrorContains(program, "'var1' is already declared");
        }

        [Test]
        public void TestAssignmentOfWrongType()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new StringLiteralExpr(0, 0, "moi");
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Can't assign a value of type String in a variable of type Int");
        }

        [Test]
        public void TestAssignmentOfValueToArray()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new ArrayType(0, 0, ExprType.Int));
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Can't assign a value of type Int in a variable of type Int[]");
        }

        [Test]
        public void TestArrayAccessOnNotArray()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1");
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            var arrayAccess = new ArrayVariableExpr(0, 0);
            arrayAccess.ArrayIdentifier = "var1";
            arrayAccess.SubscriptExpr = new IntLiteralExpr(0, 0, 1);
            assign.Variable = arrayAccess;
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Variable 'var1' is not declared as an array");
        }

        [Test]
        public void TestArraySubscriptionWithWrongType()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new ArrayType(0, 0, ExprType.Int));
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = new IntLiteralExpr(0, 0, 1);
            var arrayAccess = new ArrayVariableExpr(0, 0);
            arrayAccess.ArrayIdentifier = "var1";
            arrayAccess.SubscriptExpr = new StringLiteralExpr(0, 0, "asd");
            assign.Variable = arrayAccess;
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Array subscript expression has to be of type Int");
        }

        [Test]
        public void TestAccessSizeOfNotArray()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new ArrayType(0, 0, ExprType.Int));
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            var access = new MemberAccessExpr(0, 0);
            access.AccessedExpr = new IntLiteralExpr(0, 0, 1);
            access.MemberId = "size";
            assign.AssignmentExpr = access;
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Int has no member 'size'");
        }

        [Test]
        public void TestCallBeforeDeclaration()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "func";
            program.Block.Statements.Add(call);
            AssertErrorContains(program, "Undeclared procedure 'func'");
        }

        [Test]
        public void TestCallWithWrongNumberOfArguments()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "func";
            program.Block.Statements.Add(CreateFunction("func"));
            program.Block.Statements.Add(call);
            AssertErrorContains(program, "'func' takes 1 arguments, 0 given");
        }

        [Test]
        public void TestCallWithWrongArgumentType()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "func";
            call.Arguments.Add(new StringLiteralExpr(0, 0, "asd"));
            program.Block.Statements.Add(CreateFunction("func"));
            program.Block.Statements.Add(call);
            AssertErrorContains(program, "'func' argument 1 expects a parameter of type Int, String given");
        }

        [Test]
        public void TestCallingVariable()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("func");
            var call = new CallStmt(0, 0);
            call.ProcedureId = "func";
            call.Arguments.Add(new StringLiteralExpr(0, 0, "asd"));
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(call);
            AssertErrorContains(program, "'func' is not defined as a function or a procedure");
        }

        [Test]
        public void TestCallProcedureInExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            var call = new CallExpr(0, 0);
            call.Arguments.Add(new IntLiteralExpr(0, 0, 1));
            call.CalleeId = "proc";
            assign.AssignmentExpr = call;
            program.Block.Statements.Add(CreateProcedure("proc"));
            program.Block.Statements.Add(CreateVarDeclaration("var1"));
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "'proc' is not defined as a function");
        }

        [Test]
        public void TestCallUndeclaredFunctionInExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assign = new AssignmentStmt(0, 0);
            assign.Variable = new VariableExpr(0, 0, "var1");
            var call = new CallExpr(0, 0);
            call.Arguments.Add(new IntLiteralExpr(0, 0, 1));
            call.CalleeId = "func";
            assign.AssignmentExpr = call;
            program.Block.Statements.Add(CreateVarDeclaration("var1"));
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Undeclared function 'func'");
        }

        [Test]
        public void TestReferenceArgumentWithoutVariable()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var function = CreateFunction("func");
            function.Parameters[0] = new Parameter("arg1", new SimpleType(0, 0, ExprType.Int), true);
            var call = new CallStmt(0, 0);
            call.ProcedureId = "func";
            call.Arguments.Add(new IntLiteralExpr(0, 0, 1));
            program.Block.Statements.Add(function);
            program.Block.Statements.Add(call);
            AssertErrorContains(program, "'func' argument 1 expects a variable of type Int");
        }

        [Test]
        public void TestReturnInFunctionWithoutValue()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            var funcBlock = new BlockStmt(0, 0);
            funcBlock.Statements.Add(returnStmt);
            program.Block.Statements.Add(CreateFunction("func", funcBlock));
            AssertErrorContains(program, "Return statement can't be empty in a function");
        }

        [Test]
        public void TestReturnOfWrongType()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            returnStmt.ReturnExpression = new StringLiteralExpr(0, 0, "asd");
            var funcBlock = new BlockStmt(0, 0);
            funcBlock.Statements.Add(returnStmt);
            program.Block.Statements.Add(CreateFunction("func", funcBlock));
            AssertErrorContains(program, "Can't return a value of type String in a function of type Int");
        }

        [Test]
        public void TestReturnValueInProcedure()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = new ProcedureDeclarationStmt(0, 0);
            declaration.Identifier = "func";
            declaration.ProcedureBlock = new BlockStmt(0, 0);
            var returnStmt = new ReturnStmt(0, 0);
            returnStmt.ReturnExpression = new StringLiteralExpr(0, 0, "asd");
            declaration.ProcedureBlock.Statements.Add(returnStmt);
            program.Block.Statements.Add(declaration);
            AssertErrorContains(program, "Can't return a value in a procedure");
        }

        [Test]
        public void TestAssertWithoutBoolean()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var assert = new AssertStmt(0, 0);
            assert.AssertExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(assert);
            AssertErrorContains(program, "Assertion expression has to be of type Bool");
        }

        [Test]
        public void TestIfTestWithoutBoolean()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var ifStmt = new IfStmt(0, 0);
            ifStmt.TrueStatement = new BlockStmt(0, 0);
            ifStmt.TestExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(ifStmt);
            AssertErrorContains(program, "If test expression has to be of type Bool");
        }

        [Test]
        public void TestWhileConditionWithoutBoolean()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var whileStmt = new WhileStmt(0, 0);
            whileStmt.Body = new BlockStmt(0, 0);
            whileStmt.TestExpr = new IntLiteralExpr(0, 0, 1);
            program.Block.Statements.Add(whileStmt);
            AssertErrorContains(program, "While condition expression has to be of type Bool");
        }

        [Test]
        public void TestIncorrectBinaryExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new SimpleType(0, 0, ExprType.Bool));
            var expr = new BinaryExpr(0, 0);
            expr.Left = new IntLiteralExpr(0, 0, 1);
            expr.Right = new RealLiteralExpr(0, 0, (float)3.4);
            expr.Op = Operator.Modulus;
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = expr;
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Can't apply operator Modulus on types Real and Int");
        }

        [Test]
        public void TestIncorrectUnaryExpression()
        {
            var program = new ProgramNode(0, 0);
            program.Block = new BlockStmt(0, 0);
            var declaration = CreateVarDeclaration("var1", new SimpleType(0, 0, ExprType.Bool));
            var expr = new UnaryExpr(0, 0);
            expr.Expr = new IntLiteralExpr(0, 0, 1);
            expr.Op = Operator.Not;
            var assign = new AssignmentStmt(0, 0);
            assign.AssignmentExpr = expr;
            assign.Variable = new VariableExpr(0, 0, "var1");
            program.Block.Statements.Add(declaration);
            program.Block.Statements.Add(assign);
            AssertErrorContains(program, "Can't apply operator Not on type Int");
        }

        private FunctionDeclarationStmt CreateFunction(string name, BlockStmt block = null)
        {
            var declaration = new FunctionDeclarationStmt(0, 0);
            declaration.Identifier = name;
            declaration.ReturnType = new SimpleType(0, 0, ExprType.Int);
            if (block == null)
                declaration.ProcedureBlock = new BlockStmt(0, 0);
            else
                declaration.ProcedureBlock = block;
            declaration.AddParameter("arg1", new SimpleType(0, 0, ExprType.Int), false);
            return declaration;
        }

        private ProcedureDeclarationStmt CreateProcedure(string name, BlockStmt block = null)
        {
            var declaration = new ProcedureDeclarationStmt(0, 0);
            declaration.Identifier = name;
            if (block == null)
                declaration.ProcedureBlock = new BlockStmt(0, 0);
            else
                declaration.ProcedureBlock = block;
            declaration.AddParameter("arg1", new SimpleType(0, 0, ExprType.Int), false);
            return declaration;
        }

        private static VarDeclarationStmt CreateVarDeclaration(string name, TypeNode type = null)
        {
            if (type == null)
                type = new SimpleType(0, 0, ExprType.Int);
            var declaration = new VarDeclarationStmt(0, 0);
            declaration.Identifiers.Add(name);
            declaration.Type = type;
            return declaration;
        }

        private void AssertNoErrors(ProgramNode program)
        {
            var analyzer = new SemanticAnalyzer(program, errors);
            analyzer.Analyze();
            Assert.That(!errors.HasErrors);
            Assert.That(errors.HasErrors, Is.False);
        }

        private void AssertErrorContains(ProgramNode program, string expectedError)
        {
            var analyzer = new SemanticAnalyzer(program, errors);
            analyzer.Analyze();
            Assert.That(errors.HasErrors);
            var error = errors.GetErrors()[0];
            Assert.That(error.ErrorMessage, Does.Contain(expectedError));
        }
    }
}
