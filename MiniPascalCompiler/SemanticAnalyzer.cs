﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public class SemanticAnalyzer
    {
        private ProgramNode Program;
        private ErrorHandler Errors;
        private SymbolTable Symbols;
        private Stack<TypeInfo> TypeStack = new Stack<TypeInfo>();
        //private Stack<TypeInfo> ReturnTypeStack = new Stack<TypeInfo>();
        private Stack<CallableSymbol> FunctionStack = new Stack<CallableSymbol>();
        private TypeChecker TypeChecker;

        public SemanticAnalyzer(ProgramNode program, ErrorHandler errors)
        {
            Program = program;
            Errors = errors;
            TypeChecker = new TypeChecker();
            //ReturnTypeStack.Push(TypeInfo.BasicVoid);
        }

        public SymbolTable Analyze()
        {
            Symbols = new SymbolTable();
            TypeStack.Clear();
            FunctionStack.Clear();
            Visit(Program);
            return Symbols;
        }

        private void Visit(ProcedureDeclarationStmt declarationStmt)
        {
            if (!Symbols.ExistsInCurrentScope(declarationStmt.Identifier))
            {
                int currentScope = Symbols.CurrentScope;
                int procedureScope = Symbols.EnterScope();
                //ReturnTypeStack.Push(TypeInfo.BasicVoid);
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameter in declarationStmt.Parameters)
                {
                    var parameterSymbol = new ParameterSymbol(parameter, procedureScope);
                    Symbols.AddSymbol(parameterSymbol);
                    parameterSymbols.Add(parameterSymbol);
                }
                var procSymbol = new ProcedureSymbol(declarationStmt, parameterSymbols, currentScope);
                Symbols.AddSymbol(procSymbol);
                FunctionStack.Push(procSymbol);
                declarationStmt.DeclarationSymbol = procSymbol;
                Visit(declarationStmt.ProcedureBlock, false);
                Symbols.LeaveScope();
                //ReturnTypeStack.Pop();
                FunctionStack.Pop();
                
            }
            else
            {
                AddError(string.Format("'{0}' is already declared in current scope", declarationStmt.Identifier), declarationStmt);
            }
        }

        private void Visit(CallStmt callStmt)
        {
            Symbol callSymbol = Symbols.Lookup(callStmt.ProcedureId);
            if (callSymbol == null)
            {
                AddError(string.Format("Undeclared procedure '{0}'", callStmt.ProcedureId), callStmt);
                return;
            }
            if (!(callSymbol is FunctionSymbol) && !(callSymbol is ProcedureSymbol))
            {
                AddError(string.Format("'{0}' is not defined as a function or a procedure", callStmt.ProcedureId), callStmt);
                return;
            }
            callStmt.DeclarationSymbol = callSymbol as CallableSymbol;
            CheckCallParameters(callStmt, callStmt.Arguments, callSymbol as CallableSymbol);
        }

        private void CheckCallParameters(AstNode callNode, List<Expression> arguments, CallableSymbol callable)
        {
            int index = 0;
            if (callable.Parameters.Count != 0 && callable.Parameters[0].Varargs)
            {
                bool referenceParams = callable.Parameters[0].IsReference;
                foreach (Expression argument in arguments)
                {
                    Visit((dynamic)argument);
                    TypeInfo argumentType = TypeStack.Pop();
                    if (IsNotVoid(argumentType))
                    {
                        if (!(argument is VariableExpr) && !(argument is ArrayVariableExpr) && referenceParams)
                        {
                            AddError(string.Format("'{0}' argument {1} expects a variable of type {2}",
                                callable.Name, index + 1, argumentType), callNode);
                        }
                    }
                    index++;
                }
                return;
            }

            if (arguments.Count != callable.Parameters.Count)
            {
                AddError(string.Format("'{0}' takes {1} arguments, {2} given",
                    callable.Name, callable.Parameters.Count, arguments.Count), callNode);
                return;
            }
            foreach (Expression argument in arguments)
            {
                Visit((dynamic)argument);
                TypeInfo argumentType = TypeStack.Pop();
                ParameterSymbol parameter = callable.Parameters[index];
                if (IsNotVoid(argumentType))
                {
                    if (!TypeChecker.TypeCheckAssign(parameter.Type, argumentType))
                    {
                        AddError(string.Format("'{0}' argument {1} expects a parameter of type {2}, {3} given",
                            callable.Name, index + 1, parameter.Type, argumentType), callNode);
                    }
                    if (!(argument is VariableExpr) && !(argument is ArrayVariableExpr) && parameter.IsReference)
                    {
                        AddError(string.Format("'{0}' argument {1} expects a variable of type {2}",
                            callable.Name, index + 1, argumentType), callNode);
                    }
                }
                index++;
            }
        }

        private void Visit(ReturnStmt returnStmt)
        {
            //var expectedType = ReturnTypeStack.Peek();
            var expectedType = FunctionStack.Peek().Type;
            if (returnStmt.ReturnExpression != null && expectedType.BasicType == ExprType.Void)
            {
                AddError("Can't return a value in a procedure", returnStmt);
                return;
            }
            if (returnStmt.ReturnExpression == null)
            {
                if (IsNotVoid(expectedType))
                {
                    AddError("Return statement can't be empty in a function", returnStmt);
                }
                return;
            }
            Visit((dynamic)returnStmt.ReturnExpression);
            var returnType = TypeStack.Pop();
            if (!TypeChecker.TypeCheckAssign(expectedType, returnType))
            {
                AddError(string.Format("Can't return a value of type {0} in a function of type {1}", returnType, expectedType), returnStmt);
            }
        }

        private void Visit(IfStmt ifStmt)
        {
            Visit((dynamic)ifStmt.TestExpr);
            TypeInfo testType = TypeStack.Pop();
            if (IsNotVoid(testType) && testType.BasicType != ExprType.Bool)
            {
                AddError("If test expression has to be of type Bool", ifStmt);
            }
            Visit((dynamic)ifStmt.TrueStatement);
            if (ifStmt.FalseStatement != null)
            {
                Visit((dynamic)ifStmt.FalseStatement);
            }
        }

        private void Visit(BinaryExpr binaryExpr)
        {
            Visit((dynamic) binaryExpr.Left);
            Visit((dynamic)binaryExpr.Right);
            TypeInfo type1 = TypeStack.Pop();
            TypeInfo type2 = TypeStack.Pop();
            if (IsNotVoid(type1) && IsNotVoid(type2))
            {
                TypeInfo opType = TypeChecker.FindOpProductionType(type1, type2, binaryExpr.Op);
                if (opType.SameAs(TypeInfo.BasicVoid))
                {
                    AddError(string.Format("Can't apply operator {0} on types {1} and {2}", binaryExpr.Op, type1, type2), binaryExpr);
                }
                TypeStack.Push(opType);
                binaryExpr.Type = opType;
            }
            else
            {
                TypeStack.Push(TypeInfo.BasicVoid);
            }
        }

        private void Visit(CallExpr callExpr)
        {
            Symbol callSymbol = Symbols.Lookup(callExpr.CalleeId);
            if (callSymbol == null)
            {
                AddError(string.Format("Undeclared function '{0}'", callExpr.CalleeId), callExpr);
                TypeStack.Push(TypeInfo.BasicVoid);
                return;
            }
            if (!(callSymbol is FunctionSymbol))
            {
                AddError(string.Format("'{0}' is not defined as a function", callExpr.CalleeId), callExpr);
                TypeStack.Push(TypeInfo.BasicVoid);
                return;
            }
            CheckCallParameters(callExpr, callExpr.Arguments, callSymbol as CallableSymbol);
            TypeStack.Push(callSymbol.Type);
            callExpr.Type = callSymbol.Type;
            callExpr.DeclarationSymbol = callSymbol as FunctionSymbol;
        }

        private void Visit(ArrayVariableExpr arrayVariableExpr)
        {
            Symbol symbol = Symbols.Lookup(arrayVariableExpr.ArrayIdentifier);
            if (symbol != null)
            {
                arrayVariableExpr.VariableSymbol = symbol;
                Visit((dynamic)arrayVariableExpr.SubscriptExpr);
                TypeInfo subscriptType = TypeStack.Pop();
                if (!symbol.Type.IsArray)
                {
                    AddError(string.Format("Variable '{0}' is not declared as an array", arrayVariableExpr.ArrayIdentifier), arrayVariableExpr);
                    TypeStack.Push(TypeInfo.BasicVoid);
                }
                else if (IsNotVoid(subscriptType) && !subscriptType.SameAs(ExprType.Int))
                {
                    AddError("Array subscript expression has to be of type Int", arrayVariableExpr);
                    TypeStack.Push(TypeInfo.BasicVoid);
                }
                else
                {
                    TypeStack.Push(TypeInfo.GetInstance(symbol.Type.BasicType));
                    arrayVariableExpr.Type = TypeInfo.GetInstance(symbol.Type.BasicType);
                }
                
            }
            else
            {
                AddError(string.Format("Undeclared variable '{0}'", arrayVariableExpr.ArrayIdentifier), arrayVariableExpr);
                TypeStack.Push(TypeInfo.BasicVoid);
            }
        }

        private void Visit(RealLiteralExpr realLiteralExpr)
        {
            TypeStack.Push(TypeInfo.BasicReal);
        }

        private void Visit(MemberAccessExpr memberAccessExpr)
        {
            Visit((dynamic)memberAccessExpr.AccessedExpr);
            TypeInfo accessed = TypeStack.Pop();
            if (IsNotVoid(accessed))
            {
                if (accessed.IsArray && memberAccessExpr.MemberId == "size")
                {
                    TypeStack.Push(TypeInfo.BasicInt);
                    memberAccessExpr.Type = TypeInfo.BasicInt;
                }
                else
                {
                    AddError(string.Format("{0} has no member '{1}'", accessed, memberAccessExpr.MemberId), memberAccessExpr);
                    TypeStack.Push(TypeInfo.BasicVoid);
                }
            }
        }

        private void Visit(StringLiteralExpr stringLiteralExpr)
        {
            TypeStack.Push(TypeInfo.BasicString);
        }

        private void Visit(IntLiteralExpr intLiteralExpr)
        {
            TypeStack.Push(TypeInfo.BasicInt);
        }

        private void Visit(VariableExpr variableExpr)
        {
            Symbol symbol = Symbols.Lookup(variableExpr.Identifier);
            if (symbol != null)
            {
                variableExpr.VariableSymbol = symbol;
                var currentFunction = FunctionStack.Peek();
                if (!currentFunction.Locals.Contains(symbol) && 
                    !currentFunction.Parameters.Contains(symbol) &&
                    !currentFunction.FreeVariables.Contains(symbol))
                {
                    FunctionStack.Peek().FreeVariables.Add(symbol);
                }
                TypeStack.Push(symbol.Type);
                variableExpr.Type = symbol.Type;
            }
            else
            {
                AddError(string.Format("Undeclared variable '{0}'", variableExpr.Identifier), variableExpr);
                TypeStack.Push(TypeInfo.BasicVoid);
            }
        }

        private void Visit(UnaryExpr unaryExpr)
        {
            Visit((dynamic)unaryExpr.Expr);
            TypeInfo type = TypeStack.Pop();
            if (IsNotVoid(type))
            {
                TypeInfo opType = TypeChecker.FindOpProductionType(type, unaryExpr.Op);
                if (opType.SameAs(TypeInfo.BasicVoid))
                {
                    AddError(string.Format("Can't apply operator {0} on type {1}", unaryExpr.Op, type), unaryExpr);
                }
                TypeStack.Push(opType);
                unaryExpr.Type = opType;
            }
        }

        private void Visit(WhileStmt whileStmt)
        {
            Visit((dynamic)whileStmt.TestExpr);
            TypeInfo testType = TypeStack.Pop();
            if (IsNotVoid(testType) && testType.BasicType != ExprType.Bool)
            {
                AddError("While condition expression has to be of type Bool", whileStmt);
            }
            Visit((dynamic)whileStmt.Body);
        }

        private void Visit(AssertStmt assertStmt)
        {
            Visit((dynamic)assertStmt.AssertExpr);
            TypeInfo assertionType = TypeStack.Pop();
            if (IsNotVoid(assertionType) && assertionType.BasicType != ExprType.Bool)
            {
                AddError("Assertion expression has to be of type Bool", assertStmt);
            }
        }

        private void Visit(BlockStmt blockStmt, bool enterScope = true)
        {
            if (enterScope)
                Symbols.EnterScope();
            foreach (dynamic statement in blockStmt.Statements)
            {
                Visit(statement);
            }
            if (enterScope)
                Symbols.LeaveScope();
        }

        private void Visit(FunctionDeclarationStmt declarationStmt) // TODO tarkista että palauttaa oikeasti jotain
        {
            if (!Symbols.ExistsInCurrentScope(declarationStmt.Identifier))
            {
                int currentScope = Symbols.CurrentScope;
                int procedureScope = Symbols.EnterScope();
                //ReturnTypeStack.Push(new TypeInfo(declarationStmt.ReturnType));
                var parameterSymbols = new List<ParameterSymbol>();
                foreach (var parameter in declarationStmt.Parameters)
                {
                    var parameterSymbol = new ParameterSymbol(parameter, procedureScope);
                    Symbols.AddSymbol(parameterSymbol);
                    parameterSymbols.Add(parameterSymbol);
                }
                var funcSymbol = new FunctionSymbol(declarationStmt, parameterSymbols, currentScope);
                Symbols.AddSymbol(funcSymbol);
                FunctionStack.Push(funcSymbol);
                declarationStmt.DeclarationSymbol = funcSymbol;
                Visit(declarationStmt.ProcedureBlock, false);
                Symbols.LeaveScope();
                //ReturnTypeStack.Pop();
                FunctionStack.Pop();
            }
            else
            {
                AddError(string.Format("'{0}' is already declared in current scope", declarationStmt.Identifier), declarationStmt);
            }
        }

        private void Visit(VarDeclarationStmt varDeclarationStmt)
        {
            foreach (string identifier in varDeclarationStmt.Identifiers)
            {
                bool creationSuccess;
                Symbol createdSymbol;
                if (Symbols.CurrentScope == 1)
                {
                    createdSymbol = new GlobalSymbol(identifier, varDeclarationStmt.Type, Symbols.CurrentScope);
                }
                else
                {
                    createdSymbol = new VariableSymbol(identifier, varDeclarationStmt.Type, Symbols.CurrentScope);
                    FunctionStack.Peek().Locals.Add(createdSymbol as VariableSymbol);
                }
                creationSuccess = Symbols.AddSymbol(createdSymbol);
                if (!creationSuccess)
                {
                    AddError(string.Format("'{0}' is already declared in current scope", identifier), varDeclarationStmt);
                }
                varDeclarationStmt.VarSymbols.Add(createdSymbol);
            }
        }

        private void Visit(ProgramNode programNode)
        {
            FunctionStack.Push(new ProcedureSymbol("_Main", false, false, 1));
            Visit(programNode.Block);
        }

        private void Visit(AssignmentStmt assignmentStmt)
        {
            Visit((dynamic) assignmentStmt.Variable);
            TypeInfo varType = TypeStack.Pop();
            Visit((dynamic) assignmentStmt.AssignmentExpr);
            TypeInfo valType = TypeStack.Pop();
            if (IsNotVoid(varType) && IsNotVoid(valType) && !TypeChecker.TypeCheckAssign(varType, valType))
            {
                AddError(string.Format("Can't assign a value of type {0} in a variable of type {1}", valType, varType), assignmentStmt);
            }
        }

        private void AddError(string message, AstNode node)
        {
            Errors.AddError(message, ErrorType.SemanticError, node.Line, node.Column);
        }

        private bool IsNotVoid(TypeInfo type)
        {
            return type.BasicType != ExprType.Void;
        }
    }
}
