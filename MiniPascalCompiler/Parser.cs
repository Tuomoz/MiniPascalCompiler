using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public class Parser
    {
        private Scanner Scanner;
        private ErrorHandler Errors;
        private Token CurrentToken;
        private Token AcceptedToken;

        private Dictionary<TokenType, Func<Statement>> StatementParsers;
        private Dictionary<string, ExprType> KnownTypes = new Dictionary<string, ExprType>()
        {
            { "int", ExprType.Int },
            { "real", ExprType.Real },
            { "string", ExprType.String },
            { "bool", ExprType.Bool }
        };

        public Parser(Scanner scanner, ErrorHandler errors)
        {
            Scanner = scanner;
            Errors = errors;
            CurrentToken = Scanner.GetNextToken();

            StatementParsers = new Dictionary<TokenType, Func<Statement>>()
            {
                { TokenType.KwVar, ParseVarDeclaration },
                { TokenType.KwProcedure, ParseProcedureDeclaration },
                { TokenType.KwFunction, ParseFunctionDeclaration },
                { TokenType.Identifier, ParseAssignmentOrCall },
                { TokenType.KwReturn, ParseReturnStmt },
                { TokenType.KwAssert, ParseAssertStmt },
                { TokenType.KwIf, ParseIfStatement },
                { TokenType.KwWhile, ParseWhileStatement },
                { TokenType.KwBegin, ParseBlock }
            };
        }

        private FunctionDeclarationStmt ParseFunctionDeclaration()
        {
            Match(TokenType.KwFunction);
            FunctionDeclarationStmt statement = new FunctionDeclarationStmt(AcceptedToken);
            statement.Identifier = ParseIdentifier();
            Match(TokenType.LParen);
            statement.Parameters = ParseParameters();
            Match(TokenType.RParen);
            statement.ReturnType = ParseType();
            Match(TokenType.LineTerm);
            statement.ProcedureBlock = ParseBlock();
            return statement;
        }

        private ParameterList ParseParameters()
        {
            ParameterList parameters = new ParameterList(CurrentToken);
            if (CurrentToken.Type == TokenType.RParen) // No parameters
            {
                return parameters;
            }

            do
            {
                bool referenceParameter = Accept(TokenType.KwVar);
                IdentifierExpr identifier = ParseIdentifier();
                Match(TokenType.Colon);
                TypeNode type = ParseType();
                parameters.AddParameter(identifier, type, referenceParameter);
            }
            while (Accept(TokenType.Comma));
            return parameters;
        }

        private IdentifierExpr ParseIdentifier()
        {
            Match(TokenType.Identifier);
            return new IdentifierExpr(AcceptedToken);
        }

        private ProcedureDeclarationStmt ParseProcedureDeclaration()
        {
            Match(TokenType.KwProcedure);
            ProcedureDeclarationStmt statement = new ProcedureDeclarationStmt(AcceptedToken);
            statement.Identifier = ParseIdentifier();
            Match(TokenType.LParen);
            statement.Parameters = ParseParameters();
            Match(TokenType.RParen);
            Match(TokenType.LineTerm);
            statement.ProcedureBlock = ParseBlock();
            return statement;
        }

        private WhileStmt ParseWhileStatement()
        {
            Match(TokenType.KwWhile);
            WhileStmt statement = new WhileStmt(AcceptedToken);
            statement.TestExpr = ParseExpression();
            statement.Body = ParseStatement();
            return statement;
        }

        private IfStmt ParseIfStatement()
        {
            Match(TokenType.KwIf);
            IfStmt statement = new IfStmt(AcceptedToken);
            statement.TestExpr = ParseExpression();
            Match(TokenType.KwThen);
            statement.TrueStatement = ParseStatement();
            if (Accept(TokenType.KwElse))
            {
                statement.FalseStatement = ParseStatement();
            }
            return statement;
        }

        public ProgramNode Parse()
        {
            ProgramNode program = new ProgramNode(CurrentToken);
            Match(TokenType.KwProgram);
            Match(TokenType.Identifier);
            Match(TokenType.LineTerm);
            program.Block = ParseBlock();
            Match(TokenType.OpDot);
            return program;
        }

        private void NextToken()
        {
            CurrentToken = Scanner.GetNextToken();
        }

        private bool Accept(params TokenType[] excepted)
        {
            if (excepted.Contains(CurrentToken.Type))
            {
                AcceptedToken = CurrentToken;
                NextToken();
                return true;
            }
            return false;
        }

        private Token Match(TokenType excepted)
        {
            if (CurrentToken.Type != excepted)
            {
                throw new Exception();
            }
            AcceptedToken = CurrentToken;
            NextToken();
            return AcceptedToken;
        }

        private BlockStmt ParseBlock()
        {
            Match(TokenType.KwBegin);
            BlockStmt block = new BlockStmt(AcceptedToken);
            while (true)
            {
                block.Statements.Add(ParseStatement());
                if (Accept(TokenType.LineTerm))
                {
                    if (Accept(TokenType.KwEnd))
                    {
                        break;
                    }
                }
                else
                {
                    Match(TokenType.KwEnd);
                    break;
                }
            }
            return block;
        }

        private Statement ParseStatement()
        {
            Func<Statement> parseFunction;
            if (StatementParsers.TryGetValue(CurrentToken.Type, out parseFunction))
            {
                return parseFunction();
            }
            else
                throw new Exception();
        }

        private Statement ParseAssertStmt()
        {
            Match(TokenType.KwAssert);
            AssertStmt assert = new AssertStmt(AcceptedToken);
            assert.AssertExpr = ParseExpression();
            return assert;
        }

        private Statement ParseReturnStmt()
        {
            Match(TokenType.KwReturn);
            ReturnStmt statement = new ReturnStmt(AcceptedToken);
            statement.ReturnExpression = ParseExpression();
            return statement;
        }

        private Statement ParseAssignmentOrCall()
        {
            Match(TokenType.Identifier);
            Token identifier = AcceptedToken;
            if (Accept(TokenType.LParen))
            {
                return ParseCall(identifier);
            }
            else if (Accept(TokenType.OpAssignment))
            {
                return ParseAssignment(identifier);
            }
            else
                throw new Exception();
        }

        private AssignmentStmt ParseAssignment(Token identifier)
        {
            AssignmentStmt assignment = new AssignmentStmt(identifier);
            assignment.Identifier = new IdentifierExpr(identifier);
            assignment.AssignmentExpr = ParseExpression();
            return assignment;
        }

        private CallStmt ParseCall(Token identifier)
        {
            CallStmt call = new CallStmt(identifier);
            call.ProcedureId = new IdentifierExpr(identifier);
            call.Arguments = ParseArguments();
            Match(TokenType.RParen);
            return call;
        }

        private ArgumentList ParseArguments()
        {
            ArgumentList arguments = new ArgumentList(CurrentToken);
            if (CurrentToken.Type == TokenType.RParen) // No arguments
            {
                return arguments;
            }

            do
            {
                arguments.Arguments.Add(ParseExpression());
            }
            while (Accept(TokenType.Comma));
            return arguments;
        }

        private VarDeclarationStmt ParseVarDeclaration()
        {
            Match(TokenType.KwVar);
            VarDeclarationStmt declaration = new VarDeclarationStmt(AcceptedToken);
            do
            {
                Match(TokenType.Identifier);
                declaration.Identifiers.Add(new IdentifierExpr(AcceptedToken));
            }
            while (Accept(TokenType.Comma));
            Match(TokenType.Colon);
            declaration.Type = ParseType();
            return declaration;
        }

        private TypeNode ParseType()
        {
            if (Accept(TokenType.Identifier))
            {
                SimpleType node = new SimpleType(AcceptedToken);
                node.Type = ParseTypeName();
                return node;
            }
            else if (Accept(TokenType.KwArray))
            {
                ArrayType node = new ArrayType(AcceptedToken);
                Match(TokenType.LBracket);
                Match(TokenType.IntLiteral);
                node.Size = int.Parse(AcceptedToken.Content);
                Match(TokenType.RBracket);
                Match(TokenType.KwOf);
                Match(TokenType.Identifier);
                node.Type = ParseTypeName();
                return node;
            }
            else
                throw new Exception();
        }

        private ExprType ParseTypeName()
        {
            ExprType parsedType;
            if (KnownTypes.TryGetValue(AcceptedToken.Content, out parsedType))
            {
                return parsedType;
            }
            else
                throw new Exception();
        }

        private Expression ParseExpression()
        {
            Token firstTermToken = CurrentToken;
            Expression left = ParseSimpleExpression();
            if (Accept(TokenType.OpLess, TokenType.OpLessOrEquals, TokenType.OpMore, 
                TokenType.OpMoreOrEquals, TokenType.OpEquals, TokenType.OpNotEquals))
            {
                BinaryExpr expr = new BinaryExpr(firstTermToken);
                expr.Left = left;
                expr.SetOperatorFromToken(AcceptedToken);
                expr.Right = ParseSimpleExpression();
                return expr;
            }
            return left;
        }

        private Expression ParseSimpleExpression()
        {
            Token firstTermToken = CurrentToken;
            if (Accept(TokenType.Minus, TokenType.Plus))
            {
                Token sign = AcceptedToken;
            }
            Expression left = ParseTerm();
            if (Accept(TokenType.Plus, TokenType.Minus, TokenType.OpOr))
            {
                BinaryExpr expr = new BinaryExpr(firstTermToken);
                expr.SetOperatorFromToken(AcceptedToken);
                expr.Left = left;
                expr.Right = ParseTerm();
                return expr;
            }
            return left;
        }

        private Expression ParseTerm()
        {
            Token firstTermToken = CurrentToken;
            Expression left = ParseFactor();
            if (Accept(TokenType.OpMultiply, TokenType.OpDivide, TokenType.OpModulus, TokenType.OpAnd))
            {
                BinaryExpr expr = new BinaryExpr(firstTermToken);
                expr.SetOperatorFromToken(AcceptedToken);
                expr.Left = left;
                expr.Right = ParseFactor();
                return expr;
            }
            return left;
        }

        private Expression ParseFactor()
        {
            Expression factor;
            if (Accept(TokenType.Identifier))
            {
                factor = ParseCallOrVarExpr();
            }
            else if (Accept(TokenType.IntLiteral))
            {
                factor = new IntLiteralExpr(AcceptedToken);
            }
            else if(Accept(TokenType.StringLiteral))
            {
                factor = new StringLiteralExpr(AcceptedToken);
            }
            else if(Accept(TokenType.RealLiteral))
            {
                factor = new RealLiteralExpr(AcceptedToken);
            }
            else if(Accept(TokenType.LParen))
            {
                Expression expr = ParseExpression();
                Match(TokenType.RParen);
                factor = expr;
            }
            else if(Accept(TokenType.OpNot))
            {
                UnaryExpr expr = new UnaryExpr(AcceptedToken);
                expr.Expr = ParseFactor();
                factor = expr;
            }
            else throw new Exception();

            if (Accept(TokenType.OpDot))
            {
                MemberAccessExpr expr = new MemberAccessExpr(AcceptedToken);
                expr.Expr = factor;
                expr.MemberId = ParseIdentifier();
                return expr;
            }
            return factor;
        }

        private Expression ParseCallOrVarExpr()
        {
            Token identifier = AcceptedToken;
            if (Accept(TokenType.LParen))
            {
                CallExpr call = new CallExpr(identifier);
                call.ProcedureId = new IdentifierExpr(identifier);
                call.Arguments = ParseArguments();
                Match(TokenType.RParen);
                return call;
            }
            else if (Accept(TokenType.LBracket))
            {
                ArrayAccessExpr expr = new ArrayAccessExpr(identifier);
                expr.ArrayIdentifier = new IdentifierExpr(identifier);
                expr.SubscriptExpr = ParseExpression();
                Match(TokenType.RBracket);
                return expr;
            }
            else
            {
                return new IdentifierExpr(identifier);
            }
        }
    }
}
