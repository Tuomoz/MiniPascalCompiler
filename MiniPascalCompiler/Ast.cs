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
    }

    abstract class Statement : AstNode
    {
        public Statement(int line, int column) : base(line, column) { }
    }

    abstract class SimpleStmt : Statement
    {
        public SimpleStmt(int line, int column) : base(line, column) { }
    }

    abstract class StructuredStmt : Statement
    {
        public StructuredStmt(int line, int column) : base(line, column) { }
    }

    abstract class TypeNode : AstNode
    {
        public TypeNode(int line, int column) : base(line, column) { }
    }

    abstract class Expression : AstNode
    {
        public ExprType Type { get; set; } = ExprType.VoidType;
        public object ExprValue;

        public Expression(int line, int column) : base(line, column) { }
    }

    class SimpleType : TypeNode
    {
        public IdentifierExpr TypeName { get; set; }

        public SimpleType(int line, int column) : base(line, column) { }
    }

    class ArrayType : TypeNode
    {
        public IdentifierExpr TypeName { get; set; }
        public int Size { get; set; }

        public ArrayType(int line, int column) : base(line, column) { }
    }

    class ArgumentList : AstNode
    {
        public List<Expression> Arguments { get; set; }

        public ArgumentList(int line, int column): base(line, column)
        {
            Arguments = new List<Expression>();
        }
    }

    class ParameterList : AstNode
    {
        public struct Parameter
        {
            IdentifierExpr Identifier;
            TypeNode Type;
        }

        public List<Parameter> Parameters { get; private set; }

        public ParameterList(int line, int column) : base(line, column)
        {
            Parameters = new List<Parameter>();
        }
    }

    class BlockStmt : AstNode
    {
        public List<Statement> Statements { get; set; }

        public BlockStmt(int line, int column) : base(line, column)
        {
            Statements = new List<Statement>();
        }
    }

    class VarDeclarationStmt : Statement
    {
        public List<IdentifierExpr> Identifiers { get; set; }
        public TypeNode Type { get; set; }

        public VarDeclarationStmt(int line, int column) : base(line, column)
        {
            Identifiers = new List<IdentifierExpr>();
        }
    }

    class ProcedureDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }

        public ProcedureDeclarationStmt(int line, int column) : base(line, column) { }
    }

    class FunctionDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }
        public TypeNode ReturnType { get; set; }

        public FunctionDeclarationStmt(int line, int column) : base(line, column) { }
    }

    class IdentifierExpr : Expression
    {
        public string IdentifierName { get; set; }

        public IdentifierExpr(int line, int column, string identifierName) : base(line, column)
        {
            IdentifierName = identifierName;
        }
    }

    class AssignmentStmt : SimpleStmt
    {
        public IdentifierExpr Identifier { get; set; }
        public Expression AssignmentExpr { get; set; }

        public AssignmentStmt(int line, int column) : base(line, column) { }
    }

    class WhileStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement Body { get; set; }

        public WhileStmt(int line, int column) : base(line, column) { }
    }

    class IfStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement TrueStatement { get; set; }
        public Statement FalseStatement { get; set; }

        public IfStmt(int line, int column) : base(line, column) { }
    }

    class CallStmt : SimpleStmt
    {
        public IdentifierExpr ProcedureId { get; set; }
        public ArgumentList Arguments { get; set; }

        public CallStmt(int line, int column) : base(line, column) { }
    }

    class ReturnStmt : SimpleStmt
    {
        public Expression ReturnExpression { get; set; }

        public ReturnStmt(int line, int column) : base(line, column) { }
    }

    class AssertStmt : SimpleStmt
    {
        public Expression AssertExpr { get; set; }

        public AssertStmt(int line, int column) : base(line, column) { }
    }

    class BinaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public BinaryExpr(int line, int column) : base(line, column) { }
    }

    class UnaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Expr { get; set; }

        public UnaryExpr(int line, int column) : base(line, column) { }
    }

    class IntLiteralExpr : Expression
    {
        public IntLiteralExpr(int line, int column) : base(line, column) { }
    }

    class StringLiteralExpr : Expression
    {
        public StringLiteralExpr(int line, int column) : base(line, column) { }
    }
}
