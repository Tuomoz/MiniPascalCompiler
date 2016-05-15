using System.Collections.Generic;

namespace MiniPascalCompiler
{
    public enum SymbolType { Variable, Function, Procedure }

    public class SymbolTable
    {
        private int ScopeCounter = 0;
        public int CurrentScope { get { return ScopeStack.Peek(); } }
        private Stack<int> ScopeStack = new Stack<int>();
        private Dictionary<string, List<Symbol>> Symbols = new Dictionary<string, List<Symbol>>();

        public SymbolTable()
        {
            ScopeStack.Push(0);
            AddSymbol(new VariableSymbol("true", ExprType.Bool, false, 0));
            AddSymbol(new VariableSymbol("false", ExprType.Bool, false, 0));
            AddSymbol(new ProcedureSymbol("writeln", 0));
            AddSymbol(new ProcedureSymbol("read", 0));
        }

        public bool AddSymbol(Symbol symbol)
        {
            List<Symbol> symbolList;
            if (Symbols.TryGetValue(symbol.Name, out symbolList))
            {
                if (symbolList.Exists(listSymbol => listSymbol.Scope == CurrentScope))
                {
                    return false;
                }
                symbolList.Add(symbol);
            }
            Symbols[symbol.Name] = new List<Symbol>() { symbol };
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
            ScopeCounter++;
            ScopeStack.Push(ScopeCounter);
        }

        public void LeaveScope()
        {
            ScopeStack.Pop();
        }
    }
}
