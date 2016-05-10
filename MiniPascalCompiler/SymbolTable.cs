using System.Collections.Generic;

namespace MiniPascalCompiler
{
    public enum SymbolType { Variable, Function, Procedure }

    public class SymbolTable
    {
        private int NewScopeId = 0;
        private int CurrentScope { get { return ScopeStack.Peek(); } }
        private Stack<int> ScopeStack = new Stack<int>();
        private Dictionary<string, List<Symbol>> Symbols = new Dictionary<string, List<Symbol>>();
        //{
        //    { "true", new Symbol("true", ExprType.Bool, SymbolType.Variable, 0) },
        //    { "false", new Symbol("false", ExprType.Bool, SymbolType.Variable, 0) },
        //    { "read", new Symbol("read", ExprType.Void, SymbolType.Procedure, 0) },
        //    { "writeln", new Symbol("writeln", ExprType.Void, SymbolType.Procedure, 0) }
        //};

        public SymbolTable()
        {
            AddSymbol("true", ExprType.Bool, SymbolType.Variable);
            AddSymbol("false", ExprType.Bool, SymbolType.Variable);
            AddSymbol("writeln", ExprType.Void, SymbolType.Procedure);
            AddSymbol("read", ExprType.Void, SymbolType.Procedure);
        }

        public bool AddSymbol(string name, ExprType exprType, SymbolType symbolType)
        {
            List<Symbol> symbolList;
            if (Symbols.TryGetValue(name, out symbolList))
            {
                if (symbolList.Exists(symbol => symbol.Scope == CurrentScope))
                {
                    return false;
                }
                symbolList.Add(new Symbol(name, exprType, symbolType, CurrentScope));
            }
            Symbols[name] = new List<Symbol>() { new Symbol(name, exprType, symbolType, CurrentScope) };
            return true;
        }

        public Symbol Lookup(string name)
        {
            List<Symbol> symbolList;
            if (Symbols.TryGetValue(name, out symbolList))
            {
                Symbol predefined = null, best = null;
                foreach (var symbol in symbolList)
                {
                    if (symbol.Scope == 0)
                    {
                        predefined = symbol;
                    }
                    else
                    {
                        foreach (int stackedScope in ScopeStack)
                        {
                            if (symbol.Scope == stackedScope)
                            {
                                best = symbol;
                                break;
                            }
                            else if (best != null && symbol.Scope == stackedScope)
                            {
                                break;
                            }
                        }
                    }
                }
                if (best != null)
                {
                    return best;
                }
                else if (predefined != null)
                {
                    return predefined;
                }
            }
            return null;
        }

        public void EnterScope()
        {
            NewScopeId++;
            ScopeStack.Push(NewScopeId);
        }

        public void LeaveScope()
        {
            ScopeStack.Pop();
        }
    }

    public class Symbol
    {
        public readonly string Name;
        public readonly ExprType EvalType;
        public readonly SymbolType SymbolType;
        public readonly int Scope;

        public Symbol(string name, ExprType evalType, SymbolType symbolType, int scope)
        {
            Name = name;
            EvalType = evalType;
            SymbolType = symbolType;
            Scope = scope;
        }
    }
}
