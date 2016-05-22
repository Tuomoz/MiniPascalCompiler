using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    class CILHelper
    {
        private Dictionary<Operator, Action<ILGenerator>> NumberActions;
        private Dictionary<Operator, Action<ILGenerator>> StringActions;
        private Dictionary<Operator, Action<ILGenerator>> BooleanActions;
        private Dictionary<ExprType, Dictionary<Operator, Action<ILGenerator>>> ExprActions;

        public CILHelper()
        {
            Type[] twoStrings = new Type[] { typeof(string), typeof(string) };
            NumberActions = new Dictionary<Operator, Action<ILGenerator>>()
            {
                { Operator.Plus, il => il.Emit(OpCodes.Add) },
                { Operator.Minus, il => il.Emit(OpCodes.Sub) },
                { Operator.Times, il => il.Emit(OpCodes.Mul) },
                { Operator.Divide, il => il.Emit(OpCodes.Div) },
                { Operator.Modulus, il => il.Emit(OpCodes.Rem) },
                { Operator.Equals, il => il.Emit(OpCodes.Ceq) },
                { Operator.NotEquals, il => { il.Emit(OpCodes.Ceq); EmitNot(il); } },
                { Operator.Less, il => il.Emit(OpCodes.Clt) },
                { Operator.LessOrEquals, il => { il.Emit(OpCodes.Cgt); EmitNot(il); } },
                { Operator.More, il => il.Emit(OpCodes.Cgt) },
                { Operator.MoreOrEquals, il => { il.Emit(OpCodes.Clt); EmitNot(il); } },
            };
            StringActions = new Dictionary<Operator, Action<ILGenerator>>()
            {
                { Operator.Plus, il => il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", twoStrings)) },
                { Operator.Equals, il => il.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", twoStrings )) },
                { Operator.NotEquals, il => { il.Emit(OpCodes.Call, typeof(string).GetMethod("Equals", twoStrings)); EmitNot(il); } },
                { Operator.Less, il =>
                    {
                        il.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", twoStrings));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Clt);
                    }
                },
                { Operator.LessOrEquals, il =>
                    {
                        il.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", twoStrings));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Cgt);
                        EmitNot(il);
                    }
                },
                { Operator.More, il =>
                    {
                        il.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", twoStrings));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Cgt);
                    }
                },
                { Operator.MoreOrEquals, il =>
                    {
                        il.Emit(OpCodes.Call, typeof(string).GetMethod("Compare", twoStrings));
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Clt);
                        EmitNot(il);
                    }
                },
            };
            BooleanActions = new Dictionary<Operator, Action<ILGenerator>>()
            {
                { Operator.And, il => il.Emit(OpCodes.And) },
                { Operator.Or, il => il.Emit(OpCodes.Or) },
                { Operator.Not, il => EmitNot(il) },
                { Operator.Equals, il => il.Emit(OpCodes.Ceq) },
                { Operator.NotEquals, il => { il.Emit(OpCodes.Ceq); EmitNot(il); } },
                { Operator.Less, il => il.Emit(OpCodes.Clt) },
                { Operator.LessOrEquals, il => { il.Emit(OpCodes.Cgt); EmitNot(il); } },
                { Operator.More, il => il.Emit(OpCodes.Cgt) },
                { Operator.MoreOrEquals, il => { il.Emit(OpCodes.Clt); EmitNot(il); } },
            };
            ExprActions = new Dictionary<ExprType, Dictionary<Operator, Action<ILGenerator>>>()
            {
                { ExprType.Int, NumberActions },
                { ExprType.Real, NumberActions},
                { ExprType.String, StringActions},
                { ExprType.Bool, BooleanActions},
            };
        }

        public void EmitExprOperation(ILGenerator il, Operator op, TypeInfo type)
        {
            ExprActions[type.BasicType][op](il);
        }

        public void EmitNot(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }
}
