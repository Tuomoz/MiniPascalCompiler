using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public class Scanner
    {
        private SourceReader Source;
        private ErrorHandler ErrorHandler;
        private Dictionary<string, TokenType> SymbolTokens = new Dictionary<string, TokenType>();
        private Dictionary<char, char> EscapeCharacters = new Dictionary<char, char>();
        private Dictionary<string, TokenType> KeywordTokens = new Dictionary<string, TokenType>();

        public Scanner(SourceReader source, ErrorHandler errorHandler)
        {
            Source = source;
            ErrorHandler = errorHandler;
            SymbolTokens.Add("(", TokenType.LParen);
            SymbolTokens.Add(")", TokenType.RParen);
            SymbolTokens.Add("[", TokenType.LBracket);
            SymbolTokens.Add("]", TokenType.RBracket);
            SymbolTokens.Add("+", TokenType.Plus);
            SymbolTokens.Add("-", TokenType.Minus);
            SymbolTokens.Add("/", TokenType.OpDivide);
            SymbolTokens.Add("*", TokenType.OpMultiply);
            SymbolTokens.Add("%", TokenType.OpModulus);
            SymbolTokens.Add("<", TokenType.OpLess);
            SymbolTokens.Add(">", TokenType.OpMore);
            SymbolTokens.Add("<=", TokenType.OpLessOrEquals);
            SymbolTokens.Add(">=", TokenType.OpMoreOrEquals);
            SymbolTokens.Add("<>", TokenType.OpNotEquals);
            SymbolTokens.Add("=", TokenType.OpEquals);
            SymbolTokens.Add(".", TokenType.OpDot);
            SymbolTokens.Add(";", TokenType.LineTerm);
            SymbolTokens.Add(":", TokenType.Colon);
            SymbolTokens.Add(":=", TokenType.OpAssignment);
            SymbolTokens.Add(",", TokenType.Comma);
            KeywordTokens.Add("var", TokenType.KwVar);
            KeywordTokens.Add("while", TokenType.KwWhile);
            KeywordTokens.Add("end", TokenType.KwEnd);
            KeywordTokens.Add("in", TokenType.KwIn);
            KeywordTokens.Add("do", TokenType.KwDo);
            KeywordTokens.Add("assert", TokenType.KwAssert);
            KeywordTokens.Add("program", TokenType.KwProgram);
            KeywordTokens.Add("procedure", TokenType.KwProcedure);
            KeywordTokens.Add("function", TokenType.KwFunction);
            KeywordTokens.Add("array", TokenType.KwArray);
            KeywordTokens.Add("of", TokenType.KwOf);
            KeywordTokens.Add("begin", TokenType.KwBegin);
            KeywordTokens.Add("return", TokenType.KwReturn);
            KeywordTokens.Add("then", TokenType.KwThen);
            KeywordTokens.Add("else", TokenType.KwElse);
            KeywordTokens.Add("not", TokenType.OpNot);
            KeywordTokens.Add("or", TokenType.OpOr);
            KeywordTokens.Add("and", TokenType.OpAnd);
            EscapeCharacters.Add('"', '"');
            EscapeCharacters.Add('\'', '\'');
            EscapeCharacters.Add('n', '\n');
            EscapeCharacters.Add('t', '\t');
            EscapeCharacters.Add('\\', '\\');
        }

        public IEnumerable<Token> GetTokens()
        {
            Token nextToken = GetNextToken();
            while (nextToken != null)
            {
                yield return nextToken;
                nextToken = GetNextToken();
            }
        }

        public Token GetNextToken()
        {
            Source.ReadNext();
            SkipWhitespace();
            SkipComments();
            SkipWhitespace();
            if (!Source.CurrentChar.HasValue)
                return new Token(TokenType.EOF, Source.CurrentLine, Source.CurrentColumn);

            int newTokenLine = Source.CurrentLine, newTokenColumn = Source.CurrentColumn;

            if (SymbolTokens.ContainsKey(Source.CurrentAndPeek))
            {
                TokenType newTokenType = SymbolTokens[Source.CurrentAndPeek];
                Source.ReadNext();
                return new Token(newTokenType, newTokenLine, newTokenColumn);
            }
            else if (SymbolTokens.ContainsKey(Source.CurrentChar.ToString()))
            {
                return new Token(SymbolTokens[Source.CurrentChar.ToString()], newTokenLine, newTokenColumn);
            }
            else if (char.IsNumber(Source.CurrentChar.Value))
            {
                string tokenContent = Source.ReadWhile(peeked => char.IsDigit(peeked));
                return new Token(TokenType.IntLiteral, newTokenLine, newTokenColumn, tokenContent);
            }
            else if (char.IsLetter(Source.CurrentChar.Value))
            {
                string tokenContent = Source.ReadWhile(peeked => char.IsLetterOrDigit(peeked) || peeked == '_');
                if (KeywordTokens.ContainsKey(tokenContent))
                {
                    return new Token(KeywordTokens[tokenContent], newTokenLine, newTokenColumn);
                }
                else
                {
                    return new Token(TokenType.Identifier, newTokenLine, newTokenColumn, tokenContent);
                }
            }
            else if (Source.CurrentChar == '"')
            {
                string stringToken = BuildStringLiteral();
                return new Token(TokenType.StringLiteral, newTokenLine, newTokenColumn, stringToken);
            }
            else
            {
                ErrorHandler.AddError(
                    string.Format("Unknown token '{0}' at line {1} column {2}",
                    Source.CurrentChar, Source.CurrentLine, Source.CurrentColumn),
                    ErrorType.LexicalError);
                return GetNextToken();
            }
        }

        private string BuildStringLiteral()
        {
            System.Text.StringBuilder TokenContentBuilder = new System.Text.StringBuilder();
            while (Source.Peek() != '"' && Source.Peek() != '\n' && Source.Peek().HasValue)
            {
                if (Source.ReadNext() == '\\')
                {
                    Source.ReadNext();
                    try
                    {
                        TokenContentBuilder.Append(EscapeCharacters[Source.CurrentChar.Value]);
                    }
                    catch (KeyNotFoundException)
                    {
                        ErrorHandler.AddError(
                            string.Format("Unrecognized escape sequence '\\{0}' at line {1} column {2}",
                            Source.CurrentChar, Source.CurrentLine, Source.CurrentColumn),
                            ErrorType.LexicalError);
                    }
                }
                else
                {
                    TokenContentBuilder.Append(Source.CurrentChar.Value);
                }
            }
            if (Source.Peek() == '"')
            {
                Source.ReadNext();
            }
            else
            {
                Source.ReadNext();
                ErrorHandler.AddError(
                    string.Format("EOL while scanning string literal at line {0} column {1}",
                    Source.CurrentLine, Source.CurrentColumn),
                    ErrorType.LexicalError);
            }
            return TokenContentBuilder.ToString();
        }

        private void SkipWhitespace()
        {
            while (Source.CurrentChar.HasValue && char.IsWhiteSpace(Source.CurrentChar.Value))
                Source.ReadNext();
        }

        private void SkipComments()
        {
            while (Source.CurrentAndPeek == "//" || Source.CurrentAndPeek == "/*")
            {
                if (Source.Peek() == '/')
                {
                    while (Source.CurrentChar.HasValue && Source.CurrentChar != '\n')
                    {
                        Source.ReadNext();
                    }
                }
                else
                {
                    int commentDepth = 1;
                    int commentBeginLine = Source.CurrentLine, commentBeginColumn = Source.CurrentColumn;
                    while (Source.CurrentChar.HasValue && commentDepth > 0)
                    {
                        Source.ReadNext();
                        if (Source.CurrentChar == '/' && Source.Peek() == '*')
                        {
                            commentDepth++;
                            Source.ReadNext();
                        }
                        else if (Source.CurrentChar == '*' && Source.Peek() == '/')
                        {
                            commentDepth--;
                            Source.ReadNext();
                        }
                    }
                    Source.ReadNext();
                    if (commentDepth > 0)
                    {
                        ErrorHandler.AddError(
                            string.Format("EOF while scanning comment beginning at line {0} column {1}",
                            commentBeginLine, commentBeginColumn),
                            ErrorType.LexicalError);
                    }
                }
                SkipWhitespace();
            }
        }
    }

    public class SourceReader
    {
        private struct BufferedChar
        {
            public char StoredChar;
            public int StoredCharColumn, StoredCharLine;

            public BufferedChar(char storedChar, int storedCharColumn, int storedCharLine)
            {
                StoredChar = storedChar;
                StoredCharColumn = storedCharColumn;
                StoredCharLine = storedCharLine;
            }
        }

        private System.IO.TextReader SourceStream;
        private int ReaderColumn = 0, ReaderLine = 1;
        private Queue<BufferedChar> CharBuffer = new Queue<BufferedChar>();

        public char? CurrentChar { get; private set; }
        public int CurrentColumn { get; private set; } = 0;
        public int CurrentLine { get; private set; } = 0;

        public string CurrentAndPeek
        {
            get { return string.Concat(CurrentChar, Peek()); }
        }

        public SourceReader(System.IO.TextReader sourceStream)
        {
            SourceStream = sourceStream;
        }

        public char? ReadNext()
        {
            if (CharBuffer.Count > 0)
            {
                BufferedChar buffered = CharBuffer.Dequeue();
                CurrentChar = buffered.StoredChar;
                CurrentColumn = buffered.StoredCharColumn;
                CurrentLine = buffered.StoredCharLine;
            }
            else
            {
                CurrentChar = ReadNextFromSource();
                CurrentColumn = ReaderColumn;
                CurrentLine = ReaderLine;
            }
            return CurrentChar;
        }

        public string ReadWhile(System.Func<char, bool> testFunc)
        {
            System.Text.StringBuilder content = new System.Text.StringBuilder();
            content.Append(CurrentChar);
            while (Peek().HasValue && testFunc(Peek().Value))
            {
                content.Append(ReadNext());
            }
            return content.ToString();
        }

        public char? Peek(int offset = 0)
        {
            if (CharBuffer.Count > offset)
            {
                return CharBuffer.ElementAt(offset).StoredChar;
            }

            offset -= CharBuffer.Count;
            char? nextChar = null;
            for (int i = 0; i <= offset; i++)
            {
                nextChar = ReadNextFromSource();
                if (nextChar.HasValue)
                {
                    CharBuffer.Enqueue(new BufferedChar(nextChar.Value, ReaderColumn, ReaderLine));
                }
                else
                {
                    break;
                }
            }
            return nextChar;
        }

        private char? ReadNextFromSource()
        {
            if (CurrentChar.HasValue && CurrentChar.Value == '\n')
            {
                ReaderColumn = 0;
                ReaderLine++;
            }
            int nextChar = SourceStream.Read();
            if (nextChar == '\r')
            {
                nextChar = SourceStream.Read();
            }
            if (nextChar != -1)
            {
                ReaderColumn++;
                return (char)nextChar;
            }
            return null;
        }
    }

    public enum TokenType
    {
        Identifier, IntLiteral, LParen, RParen, Plus, Minus, KwVar,
        OpAssignment, OpRange, StringLiteral, KwWhile, KwEnd, KwIn, KwDo,
        KwAssert, OpMultiply, OpDivide, OpLess, OpEquals, OpAnd, OpNot,
        LineTerm, Colon, EOF, KwProgram, KwProcedure, KwFunction, KwArray,
        KwOf, KwBegin, KwReturn, KwThen, KwElse, OpOr, Comma, LBracket,
        RBracket, OpModulus, OpDot, OpMore, OpLessOrEquals, OpMoreOrEquals,
        OpNotEquals, RealLiteral
    };

    public class Token
    {
        public TokenType Type { get; }
        public string Content { get; }
        public int Column { get; }
        public int Line { get; }

        public Token(TokenType type, int line, int column)
        {
            Type = type;
            Column = column;
            Line = line;
        }

        public Token(TokenType type, int line, int column, string content)
        {
            Type = type;
            Column = column;
            Line = line;
            Content = content;
        }

        public override string ToString()
        {
            if (Content != null)
                return string.Format("{0}<{1},{2}>: {3}", Type, Line, Column, Content);
            return string.Format("{0}<{1},{2}>", Type, Line, Column);
        }
    }
}
