using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public enum ExprType { IntType, StringType, BoolType, VoidType };
    public enum Operator { Plus, Minus, Times, Divide, Less, Equals, And, Not }

    abstract class AstNode
    {
        public readonly int Line, Column;

        public AstNode(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public AstNode(Token token)
        {
            Line = token.Line;
            Column = token.Column;
        }
    }

    abstract class Statement : AstNode
    {
        public Statement(int line, int column) : base(line, column) { }
        public Statement(Token token) : base(token) { }
    }

    abstract class SimpleStmt : Statement
    {
        public SimpleStmt(int line, int column) : base(line, column) { }
        public SimpleStmt(Token token) : base(token) { }
    }

    abstract class StructuredStmt : Statement
    {
        public StructuredStmt(int line, int column) : base(line, column) { }
        public StructuredStmt(Token token) : base(token) { }
    }

    abstract class TypeNode : AstNode
    {
        public TypeNode(int line, int column) : base(line, column) { }
        public TypeNode(Token token) : base(token) { }
    }

    abstract class Expression : AstNode
    {
        public ExprType Type { get; set; } = ExprType.VoidType;
        public object ExprValue;

        public Expression(int line, int column) : base(line, column) { }
        public Expression(Token token) : base(token) { }
    }

    class ProgramNode : AstNode
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt Block { get; set; }

        public ProgramNode(int line, int column) : base(line, column) { }
        public ProgramNode(Token token) : base(token) { }
    }

    class SimpleType : TypeNode
    {
        public IdentifierExpr TypeName { get; set; }

        public SimpleType(int line, int column) : base(line, column) { }
        public SimpleType(Token token) : base(token) { }
    }

    class ArrayType : TypeNode
    {
        public IdentifierExpr TypeName { get; set; }
        public int Size { get; set; }

        public ArrayType(int line, int column) : base(line, column) { }
        public ArrayType(Token token) : base(token) { }
    }

    class ArgumentList : AstNode
    {
        public List<Expression> Arguments { get; set; } = new List<Expression>();

        public ArgumentList(int line, int column): base(line, column) { }
        public ArgumentList(Token token) : base(token) { }
    }

    class ParameterList : AstNode
    {
        public struct Parameter
        {
            IdentifierExpr Identifier;
            TypeNode Type;
        }

        public List<Parameter> Parameters { get; private set; } = new List<Parameter>();

        public ParameterList(int line, int column) : base(line, column) { }
        public ParameterList(Token token) : base(token) { }
    }

    class BlockStmt : AstNode
    {
        public List<Statement> Statements { get; set; } = new List<Statement>();

        public BlockStmt(int line, int column) : base(line, column) { }
        public BlockStmt(Token token) : base(token) { }
    }

    class VarDeclarationStmt : Statement
    {
        public List<IdentifierExpr> Identifiers { get; set; } = new List<IdentifierExpr>();
        public TypeNode Type { get; set; }

        public VarDeclarationStmt(int line, int column) : base(line, column) { }
        public VarDeclarationStmt(Token token) : base(token) { }
    }

    class ProcedureDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }

        public ProcedureDeclarationStmt(int line, int column) : base(line, column) { }
        public ProcedureDeclarationStmt(Token token) : base(token) { }
    }

    class FunctionDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }
        public TypeNode ReturnType { get; set; }

        public FunctionDeclarationStmt(int line, int column) : base(line, column) { }
        public FunctionDeclarationStmt(Token token) : base(token) { }
    }

    class IdentifierExpr : Expression
    {
        public string IdentifierName { get; set; }

        public IdentifierExpr(int line, int column, string identifierName) : base(line, column)
        {
            IdentifierName = identifierName;
        }
        public IdentifierExpr(Token token) : base(token)
        {
            IdentifierName = token.Content;
        }
    }

    class AssignmentStmt : SimpleStmt
    {
        public IdentifierExpr Identifier { get; set; }
        public Expression AssignmentExpr { get; set; }

        public AssignmentStmt(int line, int column) : base(line, column) { }
        public AssignmentStmt(Token token) : base(token) { }
    }

    class WhileStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement Body { get; set; }

        public WhileStmt(int line, int column) : base(line, column) { }
        public WhileStmt(Token token) : base(token) { }
    }

    class IfStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement TrueStatement { get; set; }
        public Statement FalseStatement { get; set; }

        public IfStmt(int line, int column) : base(line, column) { }
        public IfStmt(Token token) : base(token) { }
    }

    class CallStmt : SimpleStmt
    {
        public IdentifierExpr ProcedureId { get; set; }
        public ArgumentList Arguments { get; set; }

        public CallStmt(int line, int column) : base(line, column) { }
        public CallStmt(Token token) : base(token) { }
    }

    class ReturnStmt : SimpleStmt
    {
        public Expression ReturnExpression { get; set; }

        public ReturnStmt(int line, int column) : base(line, column) { }
        public ReturnStmt(Token token) : base(token) { }
    }

    class AssertStmt : SimpleStmt
    {
        public Expression AssertExpr { get; set; }

        public AssertStmt(int line, int column) : base(line, column) { }
        public AssertStmt(Token token) : base(token) { }
    }

    class BinaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public BinaryExpr(int line, int column) : base(line, column) { }
        public BinaryExpr(Token token) : base(token) { }
    }

    class UnaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Expr { get; set; }

        public UnaryExpr(int line, int column) : base(line, column) { }
        public UnaryExpr(Token token) : base(token) { }
    }

    class IntLiteralExpr : Expression
    {
        public IntLiteralExpr(int line, int column) : base(line, column) { }
        public IntLiteralExpr(Token token) : base(token) { }
    }

    class StringLiteralExpr : Expression
    {
        public StringLiteralExpr(int line, int column) : base(line, column) { }
        public StringLiteralExpr(Token token) : base(token) { }
    }
}
