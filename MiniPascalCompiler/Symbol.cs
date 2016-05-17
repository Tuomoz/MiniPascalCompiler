using System.Collections.Generic;

namespace MiniPascalCompiler
{
    public abstract class Symbol
    {
        public readonly string Name;
        public readonly TypeInfo Type;
        public readonly int Scope;

        public Symbol(string name, TypeNode type, int scope)
        {
            Name = name;
            Type = new TypeInfo(type);
            Scope = scope;
        }

        public Symbol(string name, TypeInfo type, int scope)
        {
            Name = name;
            Type = type;
            Scope = scope;
        }
    }

    public abstract class CallableSymbol : Symbol
    {
        public readonly List<ParameterSymbol> Parameters;

        public CallableSymbol(CallableDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope) :
            base(declaration.Identifier, GetCallableType(declaration), scope)
        {
            Parameters = parameters;
        }
        public CallableSymbol(string identifier, TypeInfo type, int scope) :
            base(identifier, type, scope)
        {
            Parameters = new List<ParameterSymbol>();
        }

        private static TypeInfo GetCallableType(CallableDeclarationStmt declaration)
        {
            if (declaration is FunctionDeclarationStmt)
            {
                return new TypeInfo(((FunctionDeclarationStmt)declaration).ReturnType);
            }
            else return TypeInfo.BasicVoid;
        }
    }

    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, TypeNode type, int scope) : base(name, type, scope) { }
        public VariableSymbol(string name, TypeInfo type, int scope) : base(name, type, scope) { }
    }

    public class ProcedureSymbol : CallableSymbol
    {
        public ProcedureSymbol(ProcedureDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope)
            : base(declaration, parameters, scope) { }
        public ProcedureSymbol(string name, int scope) : base(name, TypeInfo.BasicVoid, scope) { }
    }

    public class FunctionSymbol : CallableSymbol
    {
        public FunctionSymbol(FunctionDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope)
            : base(declaration, parameters, scope) { }
    }

    public class ParameterSymbol : Symbol
    {
        public readonly bool IsReference;
        public ParameterSymbol(Parameter parameter, int scope) : base(parameter.Identifier, parameter.Type, scope)
        {
            IsReference = parameter.ReferenceParameter;
        }
    }

    public struct TypeInfo
    {
        public readonly ExprType BasicType;
        public readonly bool IsArray;

        public static readonly TypeInfo BasicInt = new TypeInfo(ExprType.Int);
        public static readonly TypeInfo BasicReal = new TypeInfo(ExprType.Real);
        public static readonly TypeInfo BasicString = new TypeInfo(ExprType.String);
        public static readonly TypeInfo BasicBool = new TypeInfo(ExprType.Bool);
        public static readonly TypeInfo BasicVoid = new TypeInfo(ExprType.Void);

        public TypeInfo(ExprType basicType, bool isArray = false)
        {
            BasicType = basicType;
            IsArray = isArray;
        }

        public TypeInfo(TypeNode type)
        {
            BasicType = type.ExprType;
            IsArray = type is ArrayType;
        }

        public bool SameAs(TypeInfo comp)
        {
            return BasicType == comp.BasicType && IsArray == comp.IsArray;
        }

        public bool SameAs(ExprType basicType)
        {
            return !IsArray && BasicType == basicType;
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
