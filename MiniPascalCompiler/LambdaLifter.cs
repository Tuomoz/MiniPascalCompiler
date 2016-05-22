using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    class LambdaLifter
    {
        private ProgramNode Program;
        //private SymbolTable Symbols;

        public LambdaLifter(ProgramNode program, SymbolTable symbols)
        {
            Program = program;
            //Symbols = symbols;
        }

        public void LiftLambdas()
        {
            Visit(Program.Block);
        }

        private void Visit(BlockStmt block)
        {
            foreach (var statement in block.Statements)
            {
                Visit((dynamic)statement);
            }
        }

        private void Visit(CallableDeclarationStmt statement)
        {
            var symbol = statement.DeclarationSymbol;
            foreach (var freeSym in symbol.FreeVariables)
            {
                if (true)
                {
                    var varSym = freeSym;
                    TypeNode paramType;
                    if (varSym.Type.IsArray)
                    {
                        paramType = new ArrayType(0, 0, varSym.Type.BasicType);
                    }
                    else
                    {
                        paramType = new SimpleType(0, 0, varSym.Type.BasicType);
                    }
                    statement.AddParameter(varSym.Name, paramType, true);
                }
            }
            Visit(statement.ProcedureBlock);
        }

        private void Visit(CallStmt call)
        {
            var symbol = call.DeclarationSymbol;
            foreach (var freeSym in symbol.FreeVariables)
            {
                call.Arguments.Add(new VariableExpr(0, 0, freeSym.Name));
                if (freeSym is VariableSymbol)
                {
                    var varSym = freeSym as VariableSymbol;
                    
                }
            }
        }

        private void Visit(AstNode node) { }
    }
}
