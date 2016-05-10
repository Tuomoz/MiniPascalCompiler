using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public class Parser
    {
        private IScanner Scanner;
        private ErrorHandler Errors;
        private Token CurrentToken;
        private Token AcceptedToken;
        private Queue<Token> TokenBuffer = new Queue<Token>();

        private Dictionary<TokenType, Func<Statement>> StatementParsers;
        private Dictionary<string, ExprType> KnownTypes = new Dictionary<string, ExprType>()
        {
            { "int", ExprType.Int },
            { "real", ExprType.Real },
            { "string", ExprType.String },
            { "bool", ExprType.Bool }
        };

        public Parser(IScanner scanner, ErrorHandler errors)
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

        public ProgramNode Parse()
        {
            ProgramNode program = new ProgramNode(CurrentToken);
            Match(TokenType.KwProgram);
            program.Identifier = ParseIdentifier();
            Match(TokenType.LineTerm);
            program.Block = ParseBlock();
            Match(TokenType.OpDot);
            return program;
        }

        private void NextToken()
        {
            if (TokenBuffer.Count > 0)
            {
                CurrentToken = TokenBuffer.Dequeue();
            }
            else
            {
                CurrentToken = Scanner.GetNextToken();
            }
        }

        private bool MatchPeek(int offset, params TokenType[] excepted)
        {
            Token peeked = CurrentToken;
            if (TokenBuffer.Count > offset)
            {
                peeked = TokenBuffer.ElementAt(offset);
            }
            offset -= TokenBuffer.Count;
            for (int i = 0; i < offset; i++)
            {
                peeked = Scanner.GetNextToken();
                TokenBuffer.Enqueue(peeked);
            }
            return excepted.Contains(peeked.Type);
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

        private FunctionDeclarationStmt ParseFunctionDeclaration()
        {
            Match(TokenType.KwFunction);
            FunctionDeclarationStmt statement = new FunctionDeclarationStmt(AcceptedToken);
            statement.Identifier = ParseIdentifier();
            Match(TokenType.LParen);
            statement.Parameters = ParseParameters();
            Match(TokenType.RParen);
            Match(TokenType.Colon);
            statement.ReturnType = ParseType();
            Match(TokenType.LineTerm);
            statement.ProcedureBlock = ParseBlock();
            return statement;
        }

        private ParameterList ParseParameters()
        {
            if (CurrentToken.Type == TokenType.RParen) // No parameters
            {
                return null;
            }

            ParameterList parameters = new ParameterList(CurrentToken);
            do
            {
                bool referenceParameter = Accept(TokenType.KwVar);
                string identifier = ParseIdentifier();
                Match(TokenType.Colon);
                TypeNode type = ParseType();
                parameters.AddParameter(identifier, type, referenceParameter);
            }
            while (Accept(TokenType.Comma));
            return parameters;
        }

        private string ParseIdentifier()
        {
            Match(TokenType.Identifier);
            return AcceptedToken.Content;
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
            Match(TokenType.KwDo);
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
            if (MatchPeek(0, Expression.FirstSet))
            {
                statement.ReturnExpression = ParseExpression();
            }
            return statement;
        }

        private Statement ParseAssignmentOrCall()
        {
            if (MatchPeek(1, TokenType.LParen))
            {
                return ParseCall();
            }
            else
            {
                return ParseAssignment();
            }
        }

        private AssignmentStmt ParseAssignment()
        {
            AssignmentStmt assignment = new AssignmentStmt(CurrentToken);
            assignment.Variable = ParseVariable();
            Match(TokenType.OpAssignment);
            assignment.AssignmentExpr = ParseExpression();
            return assignment;
        }

        private IVariableExpr ParseVariable()
        {
            Token idToken = Match(TokenType.Identifier);
            string id = AcceptedToken.Content;
            if (Accept(TokenType.LBracket))
            {
                ArrayVariableExpr expr = new ArrayVariableExpr(idToken);
                expr.ArrayIdentifier = id;
                expr.SubscriptExpr = ParseExpression();
                Match(TokenType.RBracket);
                return expr;
            }
            return new VariableExpr(idToken);
        }

        private CallStmt ParseCall()
        {
            Match(TokenType.Identifier);
            CallStmt call = new CallStmt(AcceptedToken);
            call.ProcedureId = AcceptedToken.Content;
            Match(TokenType.LParen);
            call.Arguments = ParseArguments();
            Match(TokenType.RParen);
            return call;
        }

        private ArgumentList ParseArguments()
        {
            if (CurrentToken.Type == TokenType.RParen) // No arguments
            {
                return null;
            }

            ArgumentList arguments = new ArgumentList(CurrentToken);
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
                declaration.Identifiers.Add(ParseIdentifier());
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
            ExprSign termSign = ExprSign.Plus;
            if (Accept(TokenType.Minus, TokenType.Plus))
            {
                termSign = AcceptedToken.Type == TokenType.Minus ? ExprSign.Minus : ExprSign.Plus;
            }
            Expression left = ParseTerm();
            left.Sign = termSign;
            while (Accept(TokenType.Plus, TokenType.Minus, TokenType.OpOr))
            {
                BinaryExpr expr = new BinaryExpr(firstTermToken);
                expr.SetOperatorFromToken(AcceptedToken);
                expr.Left = left;
                expr.Right = ParseTerm();
                left = expr;
            }
            return left;
        }

        private Expression ParseTerm()
        {
            Token firstTermToken = CurrentToken;
            Expression left = ParseFactor();
            while (Accept(TokenType.OpMultiply, TokenType.OpDivide, TokenType.OpModulus, TokenType.OpAnd))
            {
                BinaryExpr expr = new BinaryExpr(firstTermToken);
                expr.SetOperatorFromToken(AcceptedToken);
                expr.Left = left;
                expr.Right = ParseFactor();
                left = expr;
            }
            return left;
        }

        private Expression ParseFactor()
        {
            Expression factor;
            if (CurrentToken.Type == TokenType.Identifier)
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
                expr.Op = Operator.Not;
                expr.Expr = ParseFactor();
                factor = expr;
            }
            else throw new Exception();

            if (Accept(TokenType.OpDot))
            {
                MemberAccessExpr expr = new MemberAccessExpr(AcceptedToken);
                expr.AccessedExpr = factor;
                expr.MemberId = ParseIdentifier();
                return expr;
            }
            return factor;
        }

        private Expression ParseCallOrVarExpr()
        {
            if (MatchPeek(1, TokenType.LParen))
            {
                Match(TokenType.Identifier);
                CallExpr call = new CallExpr(AcceptedToken);
                call.CalleeId = AcceptedToken.Content;
                Match(TokenType.LParen);
                call.Arguments = ParseArguments();
                Match(TokenType.RParen);
                return call;
            }
            return (Expression) ParseVariable();
        }
    }
}
