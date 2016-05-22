using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace MiniPascalCompiler
{
    class CodeGenerator
    {
        private AssemblyName name;
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;
        private TypeBuilder typeBuilder;
        private Stack<MethodBuilder> MethodStack = new Stack<MethodBuilder>();
        private ProgramNode program;
        private SymbolTable symbols;
        private CILHelper CILHelper = new CILHelper();

        private MethodBuilder CurrentMethod { get { return MethodStack.Peek(); } }
        private ILGenerator CurrentMethodIL { get { return CurrentMethod.GetILGenerator(); } }

        public CodeGenerator(ProgramNode program, SymbolTable symbols)
        {
            this.program = program;
            this.symbols = symbols;
            symbols.ResetScope();
        }

        public void Generate()
        {
            Visit(program);
            
            //ILGenerator constructorIL = main.GetILGenerator();
            //var asd = constructorIL.DeclareLocal(typeof(string));
            //asd.SetLocalSymInfo("asdlol");
            //constructorIL.Emit(OpCodes.Ldstr, "hello world");
            //constructorIL.Emit(OpCodes.Stloc_0);
            //constructorIL.Emit(OpCodes.ldf);
            //constructorIL.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            //constructorIL.Emit(OpCodes.Ret);
            Type t = typeBuilder.CreateType();
            moduleBuilder.CreateGlobalFunctions();
            assemblyBuilder.SetEntryPoint(CurrentMethod);
            assemblyBuilder.Save("test.exe");
            MethodInfo info = t.GetMethod("Main");
            object o1 = Activator.CreateInstance(t);
            info.Invoke(null, null);
        }

        private void Visit(ProgramNode program)
        {
            symbols.EnterScope();
            name = new AssemblyName(program.Identifier);
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll", true);
            
            typeBuilder = moduleBuilder.DefineType(name.Name, TypeAttributes.Public);
            var _true = typeBuilder.DefineField("_true", typeof(bool), FieldAttributes.Static);
            var _false = typeBuilder.DefineField("_true", typeof(bool), FieldAttributes.Static);
            symbols.LookupPredefined("true").CILField = _true;
            symbols.LookupPredefined("false").CILField = _false;
            var main = typeBuilder.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static);
            var il = main.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Stsfld, _true);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stsfld, _false);
            MethodStack.Push(main);
            foreach (var statement in program.Block.Statements)
            {
                Visit((dynamic)statement);
            }
            CurrentMethodIL.Emit(OpCodes.Ret);
        }

        private void Visit(BlockStmt block)
        {
            CurrentMethodIL.BeginScope();
            foreach (var statement in block.Statements)
            {
                Visit((dynamic)statement);
            }
            CurrentMethodIL.EndScope();
        }

        private void Visit(VarDeclarationStmt declaration)
        {
            var il = CurrentMethodIL;
            TypeInfo varType = new TypeInfo(declaration.Type);
            if (declaration.Type is ArrayType)
            {
                var arrSize = il.DeclareLocal(typeof(int));
                var arrayType = declaration.Type as ArrayType;
                Visit((dynamic)arrayType.SizeExpr);
                il.Emit(OpCodes.Stloc, arrSize);
                foreach (var varSymbol in declaration.VarSymbols)
                {
                    if (varSymbol is GlobalSymbol)
                    {
                        var newField = typeBuilder.DefineField(varSymbol.Name, varType.CILType, FieldAttributes.Static);
                        ((GlobalSymbol)varSymbol).CILField = newField;
                        il.Emit(OpCodes.Ldloc, arrSize);
                        il.Emit(OpCodes.Newarr, varType.CILType.GetElementType());
                        il.Emit(OpCodes.Stsfld, newField);
                    }
                    else if (varSymbol is VariableSymbol)
                    {
                        var newLocal = il.DeclareLocal(varType.CILType);
                        ((VariableSymbol)varSymbol).CILLocal = newLocal;
                        newLocal.SetLocalSymInfo(varSymbol.Name);
                        il.Emit(OpCodes.Ldloc, arrSize);
                        il.Emit(OpCodes.Newarr, varType.CILType.GetElementType());
                        il.Emit(OpCodes.Stloc, newLocal);
                    }
                }
            }
            else
            {
                foreach (var varSymbol in declaration.VarSymbols)
                {
                    if (varSymbol is GlobalSymbol)
                    {
                        var newField = typeBuilder.DefineField(varSymbol.Name, varType.CILType, FieldAttributes.Static);
                        ((GlobalSymbol)varSymbol).CILField = newField;
                    }
                    else if (varSymbol is VariableSymbol)
                    {
                        var newLocal = il.DeclareLocal(varType.CILType);
                        newLocal.SetLocalSymInfo(varSymbol.Name);
                        ((VariableSymbol)varSymbol).CILLocal = newLocal;
                    }
                    else
                    {
                        throw new Exception("Unexpected symboltype " + varSymbol.GetType());
                    }
                }
            }   
        }

        private void Visit(AssignmentStmt assign)
        {
            var il = CurrentMethodIL;
            var varSymbol = assign.VariableSymbol;
            if (varSymbol.Type.IsArray)
            {
                var arrVar = assign.Variable as ArrayVariableExpr;
                if (varSymbol is GlobalSymbol)
                {
                    var globalVar = varSymbol as GlobalSymbol;
                    il.Emit(OpCodes.Ldsfld, globalVar.CILField);
                    Visit((dynamic)arrVar.SubscriptExpr);
                    Visit((dynamic)assign.AssignmentExpr);
                    il.Emit(OpCodes.Stelem, varSymbol.Type.CILType.GetElementType());
                }
                else if (varSymbol is VariableSymbol)
                {
                    var localVar = varSymbol as VariableSymbol;
                    il.Emit(OpCodes.Ldloc, localVar.CILLocal);
                    Visit((dynamic)arrVar.SubscriptExpr);
                    Visit((dynamic)assign.AssignmentExpr);
                    il.Emit(OpCodes.Stelem, varSymbol.Type.CILType.GetElementType());
                }
                else if (varSymbol is ParameterSymbol)
                {
                    var localVar = varSymbol as ParameterSymbol;
                    il.Emit(OpCodes.Ldarg, localVar.CILParameter.Position - 1);
                    Visit((dynamic)arrVar.SubscriptExpr);
                    Visit((dynamic)assign.AssignmentExpr);
                    il.Emit(OpCodes.Stelem, varSymbol.Type.CILType.GetElementType());
                }
                else
                {
                    throw new Exception("Unexpected symboltype " + varSymbol.GetType());
                }
            }
            else
            {
                Visit((dynamic)assign.AssignmentExpr);
                if (varSymbol is GlobalSymbol)
                {
                    var globalVar = varSymbol as GlobalSymbol;
                    CurrentMethodIL.Emit(OpCodes.Stsfld, globalVar.CILField);
                }
                else if (varSymbol is VariableSymbol)
                {
                    var localVar = varSymbol as VariableSymbol;
                    CurrentMethodIL.Emit(OpCodes.Stloc, localVar.CILLocal);
                }
                else if (varSymbol is ParameterSymbol)
                {
                    var localVar = varSymbol as ParameterSymbol;
                    CurrentMethodIL.Emit(OpCodes.Starg, localVar.CILParameter.Position - 1);
                }
                else
                {
                    throw new Exception("Unexpected symboltype " + varSymbol.GetType());
                }
            }
        }

        private void Visit(VariableExpr variable)
        {
            var il = CurrentMethodIL;
            var varSymbol = variable.VariableSymbol;
            if (varSymbol is GlobalSymbol)
            {
                var globalVar = varSymbol as GlobalSymbol;
                il.Emit(OpCodes.Ldsfld, globalVar.CILField);
            }
            else if (varSymbol is VariableSymbol)
            {
                var localVar = varSymbol as VariableSymbol;
                il.Emit(OpCodes.Ldloc, localVar.CILLocal);
            }
            else if (varSymbol is ParameterSymbol)
            {
                var paramVar = varSymbol as ParameterSymbol;
                il.Emit(OpCodes.Ldarg, paramVar.CILParameter.Position - 1);
            }
            else
            {
                throw new Exception("Unexpected symboltype " + varSymbol.GetType());
            }
            ApplyExprSign(variable);
        }

        private void Visit(ArrayVariableExpr variable)
        {
            var il = CurrentMethodIL;
            var varSymbol = variable.VariableSymbol;
            if (varSymbol is GlobalSymbol)
            {
                var globalVar = varSymbol as GlobalSymbol;
                il.Emit(OpCodes.Ldsfld, globalVar.CILField);
            }
            else if (varSymbol is VariableSymbol)
            {
                var localVar = varSymbol as VariableSymbol;
                il.Emit(OpCodes.Ldloc, localVar.CILLocal);
            }
            else if (varSymbol is ParameterSymbol)
            {
                var paramVar = varSymbol as ParameterSymbol;
                il.Emit(OpCodes.Ldarg, paramVar.CILParameter.Position - 1);
            }
            else
            {
                throw new Exception("Unexpected symboltype " + varSymbol.GetType());
            }
            Visit((dynamic)variable.SubscriptExpr);
            il.Emit(OpCodes.Ldelem, varSymbol.Type.CILType.GetElementType());
            ApplyExprSign(variable);
        }

        private void Visit(BinaryExpr expr)
        {
            var il = CurrentMethodIL;
            Visit((dynamic)expr.Left);
            if (expr.Type.SameAs(TypeInfo.BasicReal) && !expr.Type.SameAs(expr.Left.Type))
            {
                il.Emit(expr.Type.GetCILConvertOp());
            }
            Visit((dynamic)expr.Right);
            if (expr.Type.SameAs(TypeInfo.BasicReal) && !expr.Type.SameAs(expr.Right.Type))
            {
                il.Emit(expr.Type.GetCILConvertOp());
            }
            CILHelper.EmitExprOperation(il, expr.Op, expr.Left.Type);
            ApplyExprSign(expr);
        }

        private void Visit(UnaryExpr expr)
        {
            var il = CurrentMethodIL;
            Visit((dynamic)expr.Expr);
            CILHelper.EmitExprOperation(il, expr.Op, expr.Expr.Type);
        }

        private void Visit(CallableDeclarationStmt declaration)
        {
            var funcSymbol = declaration.DeclarationSymbol;
            Type[] paramTypes = funcSymbol.Parameters.Select(p =>
            {
                if (p.IsReference)
                    return p.Type.CILType.MakeByRefType();
                return p.Type.CILType;
            }).ToArray();

            var func = typeBuilder.DefineMethod(declaration.Identifier, MethodAttributes.Static, funcSymbol.Type.CILType, paramTypes);
            declaration.DeclarationSymbol.CILMethod = func;
            MethodStack.Push(func);
            int index = 1;
            foreach (var parameter in funcSymbol.Parameters)
            {
                var CILParameter = func.DefineParameter(index, ParameterAttributes.None, parameter.Name);
                parameter.CILParameter = CILParameter;
            }
            foreach (var statement in declaration.ProcedureBlock.Statements)
            {
                Visit((dynamic)statement);
            }
            if (declaration is ProcedureDeclarationStmt)
                CurrentMethodIL.Emit(OpCodes.Ret);
            MethodStack.Pop();
        }

        private void Visit(ReturnStmt returnStmt)
        {
            if (returnStmt.ReturnExpression != null)
            {
                Visit((dynamic)returnStmt.ReturnExpression);
            }
            CurrentMethodIL.Emit(OpCodes.Ret);
        }

        private void Visit(CallStmt call)
        {
            var il = CurrentMethodIL;
            Symbol callee = call.DeclarationSymbol;
            if (callee.Predefined && callee.Name == "writeln")
            {
                EmitPrintCall(call);
            }
            else if (callee.Predefined && callee.Name == "read")
            {
                EmitReadCall(call);
            }
            else
            {
                foreach (var argument in call.Arguments)
                {
                    Visit((dynamic)argument);
                }
                il.Emit(OpCodes.Call, call.DeclarationSymbol.CILMethod);
                if (!call.DeclarationSymbol.Type.SameAs(TypeInfo.BasicVoid))
                {
                    il.Emit(OpCodes.Pop);
                }
            }
        }

        private void Visit(CallExpr call)
        {
            var il = CurrentMethodIL;
            foreach (var argument in call.Arguments)
            {
                Visit((dynamic)argument);
            }
            il.Emit(OpCodes.Call, call.DeclarationSymbol.CILMethod);
        }

        private void Visit(IfStmt ifStmt)
        {
            var il = CurrentMethodIL;
            var falseLbl = il.DefineLabel();
            var doneLbl = il.DefineLabel();
            Visit((dynamic)ifStmt.TestExpr);
            il.Emit(OpCodes.Brfalse, falseLbl);
            Visit((dynamic)ifStmt.TrueStatement);
            il.Emit(OpCodes.Br, doneLbl);
            il.MarkLabel(falseLbl);
            if (ifStmt.FalseStatement != null)
            {
                Visit((dynamic)ifStmt.FalseStatement);
            }
            else
            {
                il.Emit(OpCodes.Nop);
            }
            il.MarkLabel(doneLbl);
        }

        private void Visit(WhileStmt whileStmt)
        {
            var il = CurrentMethodIL;
            var bodyLbl = il.DefineLabel();
            var condLbl = il.DefineLabel();
            il.Emit(OpCodes.Br, condLbl);
            il.MarkLabel(bodyLbl);
            Visit((dynamic)whileStmt.Body);
            il.MarkLabel(condLbl);
            Visit((dynamic)whileStmt.TestExpr);
            il.Emit(OpCodes.Brtrue, bodyLbl);
        }

        private void Visit(MemberAccessExpr expr)
        {
            Visit((dynamic)expr.AccessedExpr);
            if (expr.MemberId.Equals("size"))
            {
                CurrentMethodIL.Emit(OpCodes.Ldlen);
                ApplyExprSign(expr);
            }
        }

        private void Visit(IntLiteralExpr intLiteral)
        {
            CurrentMethodIL.Emit(OpCodes.Ldc_I4, intLiteral.Value);
            ApplyExprSign(intLiteral);
        }

        private void Visit(RealLiteralExpr realLiteral)
        {
            CurrentMethodIL.Emit(OpCodes.Ldc_R8, realLiteral.Value);
            ApplyExprSign(realLiteral);
        }

        private void Visit(StringLiteralExpr stringLiteral)
        {
            CurrentMethodIL.Emit(OpCodes.Ldstr, stringLiteral.Value);
        }

        private void ApplyExprSign(Expression expr)
        {
            if (expr.Sign == ExprSign.Minus)
            {
                CurrentMethodIL.Emit(OpCodes.Neg);
            }
        }

        private void EmitPrintCall(CallStmt call)
        {
            var il = CurrentMethodIL;
            if (call.Arguments.Count > 0)
            {
                var paramsArr = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Ldc_I4, call.Arguments.Count);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc, paramsArr);
                int index = 0;
                foreach (var argument in call.Arguments)
                {
                    il.Emit(OpCodes.Ldloc, paramsArr);
                    il.Emit(OpCodes.Ldc_I4, index);
                    Visit((dynamic)argument);
                    il.Emit(OpCodes.Box, argument.Type.CILType);
                    il.Emit(OpCodes.Stelem_Ref);
                    index++;
                }
                il.Emit(OpCodes.Ldstr, " ");
                il.Emit(OpCodes.Ldloc, paramsArr);
                il.Emit(OpCodes.Call, typeof(string).GetMethod("Join", new Type[] { typeof(string), typeof(object[]) }));
                il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            }
            else
            {
                il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", Type.EmptyTypes ));
            }            
        }

        private void EmitReadCall(CallStmt call)
        {
            var il = CurrentMethodIL;
            foreach (VariableExpr argument in call.Arguments)
            {
                il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", Type.EmptyTypes));
                if (argument.Type.SameAs(TypeInfo.BasicInt))
                {
                    il.Emit(OpCodes.Call, typeof(int).GetMethod("Parse", new Type[] { typeof(string) }));
                }
                else if (argument.Type.SameAs(TypeInfo.BasicReal))
                {
                    il.Emit(OpCodes.Call, typeof(double).GetMethod("Parse", new Type[] { typeof(string) }));
                }
                else if (argument.Type.SameAs(TypeInfo.BasicBool))
                {
                    il.Emit(OpCodes.Call, typeof(bool).GetMethod("Parse", new Type[] { typeof(string) }));
                }

                var varSymbol = argument.VariableSymbol;
                if (varSymbol is GlobalSymbol)
                {
                    var globalVar = varSymbol as GlobalSymbol;
                    CurrentMethodIL.Emit(OpCodes.Stsfld, globalVar.CILField);
                }
                else if (varSymbol is VariableSymbol)
                {
                    var localVar = varSymbol as VariableSymbol;
                    CurrentMethodIL.Emit(OpCodes.Stloc, localVar.CILLocal);
                }
            }
        }
    }
}
