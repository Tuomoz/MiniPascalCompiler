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
            AddSymbol(new VariableSymbol("true", TypeInfo.BasicBool, 0));
            AddSymbol(new VariableSymbol("false", TypeInfo.BasicBool, 0));
            AddSymbol(new ProcedureSymbol("writeln", false, true, 0));
            AddSymbol(new ProcedureSymbol("read", true, false, 0));
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

        public bool ExistsInCurrentScope(string name)
        {
            List<Symbol> symbolList;
            if (Symbols.TryGetValue(name, out symbolList))
            {
                if (symbolList.Exists(listSymbol => listSymbol.Scope == CurrentScope))
                {
                    return true;
                }
            }
            return false;
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

        public int EnterScope()
        {
            ScopeCounter++;
            ScopeStack.Push(ScopeCounter);
            return ScopeCounter;
        }

        public int LeaveScope()
        {
            return ScopeStack.Pop();
        }

        public void ResetScope()
        {
            ScopeStack.Clear();
            ScopeCounter = 0;
            ScopeStack.Push(0);
        }
    }
}
