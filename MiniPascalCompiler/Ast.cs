﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public enum ExprType { Int, Real, String, Bool, Void };
    public enum Operator
    {
        Plus, Minus, Times, Divide, Less, Equals, And, Not,
        Modulus, LessOrEquals, More, MoreOrEquals, NotEquals
    }
    public enum ExprSign { Plus, Minus }

    public abstract class AstNode
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

    public abstract class Statement : AstNode
    {
        public Statement(int line, int column) : base(line, column) { }
        public Statement(Token token) : base(token) { }
    }

    public abstract class SimpleStmt : Statement
    {
        public SimpleStmt(int line, int column) : base(line, column) { }
        public SimpleStmt(Token token) : base(token) { }
    }

    public abstract class StructuredStmt : Statement
    {
        public StructuredStmt(int line, int column) : base(line, column) { }
        public StructuredStmt(Token token) : base(token) { }
    }

    public abstract class TypeNode : AstNode
    {
        public TypeNode(int line, int column) : base(line, column) { }
        public TypeNode(Token token) : base(token) { }
    }

    public abstract class Expression : AstNode
    {
        public static readonly TokenType[] FirstSet = 
        {
            TokenType.Minus, TokenType.Plus, TokenType.Identifier, TokenType.IntLiteral,
            TokenType.RealLiteral, TokenType.StringLiteral, TokenType.LParen, TokenType.OpNot
        };
        public ExprType Type { get; set; } = ExprType.Void;
        public ExprSign Sign { get; set; } = ExprSign.Plus;

        public Expression(int line, int column) : base(line, column) { }
        public Expression(Token token) : base(token) { }
    }

    public interface IVariableExpr { }

    public class ProgramNode : AstNode
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt Block { get; set; }

        public ProgramNode(int line, int column) : base(line, column) { }
        public ProgramNode(Token token) : base(token) { }
    }

    public class SimpleType : TypeNode
    {
        public ExprType Type { get; set; }

        public SimpleType(int line, int column, ExprType type) : base(line, column)
        {
            Type = type;
        }
        public SimpleType(Token token) : base(token) { }
    }

    public class ArrayType : TypeNode
    {
        public ExprType Type { get; set; }
        public int Size { get; set; }

        public ArrayType(int line, int column, ExprType type, int size) : base(line, column)
        {
            Type = type;
            Size = size;
        }
        public ArrayType(Token token) : base(token) { }
    }

    public class ArgumentList : AstNode
    {
        public List<Expression> Arguments { get; set; } = new List<Expression>();

        public ArgumentList(int line, int column): base(line, column) { }
        public ArgumentList(Token token) : base(token) { }
    }

    public class ParameterList : AstNode
    {
        public struct Parameter
        {
            public readonly IdentifierExpr Identifier;
            public readonly TypeNode Type;
            public readonly bool ReferenceParameter;

            public Parameter(IdentifierExpr identifier, TypeNode type, bool referenceParameter)
            {
                Identifier = identifier;
                Type = type;
                ReferenceParameter = referenceParameter;
            }
        }

        public List<Parameter> Parameters { get; private set; } = new List<Parameter>();

        public ParameterList(int line, int column) : base(line, column) { }
        public ParameterList(Token token) : base(token) { }

        public void AddParameter(IdentifierExpr identifier, TypeNode type, bool referenceParameter)
        {
            Parameters.Add(new Parameter(identifier, type, referenceParameter));
        }
    }

    public class BlockStmt : Statement
    {
        public List<Statement> Statements { get; set; } = new List<Statement>();

        public BlockStmt(int line, int column) : base(line, column) { }
        public BlockStmt(Token token) : base(token) { }
    }

    public class VarDeclarationStmt : Statement
    {
        public List<IdentifierExpr> Identifiers { get; set; } = new List<IdentifierExpr>();
        public TypeNode Type { get; set; }

        public VarDeclarationStmt(int line, int column) : base(line, column) { }
        public VarDeclarationStmt(Token token) : base(token) { }
    }

    public class ProcedureDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }

        public ProcedureDeclarationStmt(int line, int column) : base(line, column) { }
        public ProcedureDeclarationStmt(Token token) : base(token) { }
    }

    public class FunctionDeclarationStmt : Statement
    {
        public IdentifierExpr Identifier { get; set; }
        public BlockStmt ProcedureBlock { get; set; }
        public ParameterList Parameters { get; set; }
        public TypeNode ReturnType { get; set; }

        public FunctionDeclarationStmt(int line, int column) : base(line, column) { }
        public FunctionDeclarationStmt(Token token) : base(token) { }
    }

    public class IdentifierExpr : Expression, IVariableExpr
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

    public class AssignmentStmt : SimpleStmt
    {
        public IVariableExpr Identifier { get; set; }
        public Expression AssignmentExpr { get; set; }

        public AssignmentStmt(int line, int column) : base(line, column) { }
        public AssignmentStmt(Token token) : base(token) { }
    }

    public class WhileStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement Body { get; set; }

        public WhileStmt(int line, int column) : base(line, column) { }
        public WhileStmt(Token token) : base(token) { }
    }

    public class IfStmt : StructuredStmt
    {
        public Expression TestExpr { get; set; }
        public Statement TrueStatement { get; set; }
        public Statement FalseStatement { get; set; }

        public IfStmt(int line, int column) : base(line, column) { }
        public IfStmt(Token token) : base(token) { }
    }

    public class CallStmt : SimpleStmt
    {
        public IdentifierExpr ProcedureId { get; set; }
        public ArgumentList Arguments { get; set; }

        public CallStmt(int line, int column) : base(line, column) { }
        public CallStmt(Token token) : base(token) { }
    }

    public class ReturnStmt : SimpleStmt
    {
        public Expression ReturnExpression { get; set; }

        public ReturnStmt(int line, int column) : base(line, column) { }
        public ReturnStmt(Token token) : base(token) { }
    }

    public class AssertStmt : SimpleStmt
    {
        public Expression AssertExpr { get; set; }

        public AssertStmt(int line, int column) : base(line, column) { }
        public AssertStmt(Token token) : base(token) { }
    }

    public class BinaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public BinaryExpr(int line, int column) : base(line, column) { }
        public BinaryExpr(Token token) : base(token) { }

        public void SetOperatorFromToken(Token token)
        {
            switch(token.Type)
            {
                case TokenType.Plus: Op = Operator.Plus; break;
                case TokenType.Minus: Op = Operator.Minus; break;
                case TokenType.OpMultiply: Op = Operator.Times; break;
                case TokenType.OpDivide: Op = Operator.Divide; break;
                case TokenType.OpModulus: Op = Operator.Modulus; break;
                case TokenType.OpLess: Op = Operator.Less; break;
                case TokenType.OpLessOrEquals: Op = Operator.LessOrEquals; break;
                case TokenType.OpMore: Op = Operator.More; break;
                case TokenType.OpMoreOrEquals: Op = Operator.MoreOrEquals; break;
                case TokenType.OpEquals: Op = Operator.Equals; break;
                case TokenType.OpNotEquals: Op = Operator.NotEquals; break;
                case TokenType.OpAnd: Op = Operator.And; break;
            }
        }
    }

    public class UnaryExpr : Expression
    {
        public Operator Op { get; set; }
        public Expression Expr { get; set; }

        public UnaryExpr(int line, int column) : base(line, column) { }
        public UnaryExpr(Token token) : base(token) { }
    }

    public class IntLiteralExpr : Expression
    {
        public int Value { get; set; }

        public IntLiteralExpr(int line, int column, int value) : base(line, column)
        {
            Value = value;
        }
        public IntLiteralExpr(Token token) : base(token)
        {
            Value = int.Parse(token.Content);
        }
    }

    public class StringLiteralExpr : Expression
    {
        public string Value { get; set; }

        public StringLiteralExpr(int line, int column, string value) : base(line, column)
        {
            Value = value;
        }
        public StringLiteralExpr(Token token) : base(token)
        {
            Value = token.Content;
        }
    }

    public class RealLiteralExpr : Expression
    {
        public float Value { get; set; }

        public RealLiteralExpr(int line, int column) : base(line, column) { }
        public RealLiteralExpr(Token token) : base(token)
        {
            Value = float.Parse(token.Content);
        }
    }

    public class CallExpr : Expression
    {
        public IdentifierExpr ProcedureId { get; set; }
        public ArgumentList Arguments { get; set; }

        public CallExpr(int line, int column) : base(line, column) { }
        public CallExpr(Token token) : base(token) { }
    }

    public class MemberAccessExpr : Expression
    {
        public Expression Expr { get; set; }
        public IdentifierExpr MemberId { get; set; }

        public MemberAccessExpr(int line, int column) : base(line, column) { }
        public MemberAccessExpr(Token token) : base(token) { }
    }

    public class ArrayVariableExpr : Expression, IVariableExpr
    {
        public Expression SubscriptExpr { get; set; }
        public IdentifierExpr ArrayIdentifier { get; set; }

        public ArrayVariableExpr(int line, int column) : base(line, column) { }
        public ArrayVariableExpr(Token token) : base(token) { }
    }
}
