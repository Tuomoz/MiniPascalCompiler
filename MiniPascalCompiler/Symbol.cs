using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MiniPascalCompiler
{
    public abstract class Symbol
    {
        public readonly string Name;
        public readonly TypeInfo Type;
        public readonly int Scope;
        public bool Predefined { get { return Scope == 0; } }

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
        public List<Symbol> FreeVariables = new List<Symbol>();

        public CallableSymbol(CallableDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope) :
            base(declaration.Identifier, GetCallableType(declaration), scope)
        {
            Parameters = parameters;
        }
        public CallableSymbol(string identifier, TypeInfo type, bool isReference, bool varargs, int scope) :
            base(identifier, type, scope)
        {
            Parameters = new List<ParameterSymbol>();
            if (varargs)
            {
                Parameters.Add(new ParameterSymbol(identifier, TypeInfo.BasicVoid, isReference, varargs, scope));
            }
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
        public LocalBuilder CILLocal;
        public VariableSymbol(string name, TypeNode type, int scope) : base(name, type, scope) { }
        public VariableSymbol(string name, TypeInfo type, int scope) : base(name, type, scope) { }
    }

    public class GlobalSymbol : Symbol
    {
        public FieldBuilder CILField;
        public GlobalSymbol(string name, TypeNode type, int scope) : base(name, type, scope) { }
        public GlobalSymbol(string name, TypeInfo type, int scope) : base(name, type, scope) { }
    }

    public class ProcedureSymbol : CallableSymbol
    {
        public ProcedureSymbol(ProcedureDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope) :
            base(declaration, parameters, scope) { }
        public ProcedureSymbol(string name, bool isReference, bool varargs, int scope) :
            base(name, TypeInfo.BasicVoid, isReference, varargs, scope) { }
    }

    public class FunctionSymbol : CallableSymbol
    {
        public FunctionSymbol(FunctionDeclarationStmt declaration, List<ParameterSymbol> parameters, int scope)
            : base(declaration, parameters, scope) { }
    }

    public class ParameterSymbol : Symbol
    {
        public readonly bool IsReference;
        public readonly bool Varargs;

        public ParameterSymbol(Parameter parameter, int scope) : base(parameter.Identifier, parameter.Type, scope)
        {
            IsReference = parameter.ReferenceParameter;
            Varargs = false;
        }
        public ParameterSymbol(string identifier, TypeInfo type, bool isReference, bool varargs, int scope) : base(identifier, type, scope)
        {
            IsReference = isReference;
            Varargs = varargs;
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

        public Type CILType
        {
            get
            {
                Type CILType = GetCILType(BasicType);
                return IsArray ? CILType.MakeArrayType() : CILType;
            }
        }

        public TypeInfo(ExprType basicType, bool isArray = false)
        {
            BasicType = basicType;
            IsArray = isArray;
            int i = 2;
        }

        public TypeInfo(TypeNode type)
        {
            BasicType = type.ExprType;
            IsArray = type is ArrayType;
        }

        public static TypeInfo GetInstance(ExprType type)
        {
            switch (type)
            {
                case ExprType.Bool: return BasicBool;
                case ExprType.Int: return BasicInt;
                case ExprType.Real: return BasicReal;
                case ExprType.String: return BasicString;
                case ExprType.Void: return BasicVoid;
                default: throw new System.ArgumentException("Unknown type " + type);
            }
        }

        public static Type GetCILType(ExprType type)
        {
            switch (type)
            {
                case ExprType.Bool: return typeof(bool);
                case ExprType.Int: return typeof(Int32);
                case ExprType.Real: return typeof(double);
                case ExprType.String: return typeof(string);
                default: return null;
            }
        }

        public bool SameAs(TypeInfo comp)
        {
            return BasicType == comp.BasicType && IsArray == comp.IsArray;
        }

        public bool SameAs(ExprType basicType)
        {
            return !IsArray && BasicType == basicType;
        }

        public OpCode GetCILConvertOp()
        {
            switch (BasicType)
            {
                case ExprType.Real: return OpCodes.Conv_R8;
                default: throw new Exception("No conversion available to " + BasicType);
            }
            
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
