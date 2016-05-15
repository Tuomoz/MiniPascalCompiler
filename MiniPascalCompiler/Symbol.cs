namespace MiniPascalCompiler
{
    public abstract class Symbol
    {
        public readonly string Name;
        public readonly TypeInfo Type;
        public readonly int Scope;

        public Symbol(string name, ExprType basicType, bool isArrayType, int scope)
        {
            Name = name;
            Type = new TypeInfo(basicType, isArrayType);
            Scope = scope;
        }
    }

    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, ExprType basicType, bool isArrayType, int scope) : base(name, basicType, isArrayType, scope) { }
    }

    public class ProcedureSymbol : Symbol
    {
        public ProcedureSymbol(string name, int scope) : base(name, ExprType.Void, false, scope) { }
    }

    public class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ExprType returnType, bool isArrayType, int scope) : base(name, returnType, isArrayType, scope) { }
    }

    public class ParameterSymbol : Symbol
    {
        public readonly bool IsReference;
        public ParameterSymbol(string name, ExprType returnType, bool isArrayType, bool isReference, int scope) : base(name, returnType, isArrayType, scope)
        {
            IsReference = isReference;
        }
    }

    public struct TypeInfo
    {
        public readonly ExprType BasicType;
        public readonly bool IsArray;

        public TypeInfo(ExprType basicType, bool isArray = false)
        {
            BasicType = basicType;
            IsArray = isArray;
        }

        public bool SameAs(TypeInfo comp)
        {
            return BasicType == comp.BasicType && IsArray == comp.IsArray;
        }

        public override string ToString()
        {
            if (IsArray)
            {
                return BasicType + "[]";
            }
            return BasicType.ToString();
        }
    }
}
