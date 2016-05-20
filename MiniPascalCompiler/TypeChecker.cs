using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    class TypeChecker
    {
        private Dictionary<Operator, ExprType> intTypeBindings;
        private Dictionary<Operator, ExprType> realTypeBindings;
        private Dictionary<Operator, ExprType> boolTypeBindings;
        private Dictionary<Operator, ExprType> stringTypeBindings;
        private Dictionary<ExprType, Dictionary<Operator, ExprType>> typeBindings;

        public TypeChecker()
        {
            intTypeBindings = new Dictionary<Operator, ExprType>()
            {
                { Operator.Plus, ExprType.Int },
                { Operator.Minus, ExprType.Int },
                { Operator.Divide, ExprType.Real },
                { Operator.Modulus, ExprType.Int },
                { Operator.Times, ExprType.Int },
                { Operator.Equals, ExprType.Bool },
                { Operator.NotEquals, ExprType.Bool },
                { Operator.Less, ExprType.Bool },
                { Operator.LessOrEquals, ExprType.Bool },
                { Operator.More, ExprType.Bool },
                { Operator.MoreOrEquals, ExprType.Bool }
            };
            realTypeBindings = new Dictionary<Operator, ExprType>()
            {
                { Operator.Plus, ExprType.Real },
                { Operator.Minus, ExprType.Real },
                { Operator.Divide, ExprType.Real },
                { Operator.Times, ExprType.Real },
                { Operator.Equals, ExprType.Bool },
                { Operator.NotEquals, ExprType.Bool },
                { Operator.Less, ExprType.Bool },
                { Operator.LessOrEquals, ExprType.Bool },
                { Operator.More, ExprType.Bool },
                { Operator.MoreOrEquals, ExprType.Bool }
            };
            boolTypeBindings = new Dictionary<Operator, ExprType>()
            {
                { Operator.Equals, ExprType.Bool },
                { Operator.NotEquals, ExprType.Bool },
                { Operator.And, ExprType.Bool },
                { Operator.Or, ExprType.Bool },
                { Operator.Not, ExprType.Bool },
                { Operator.Less, ExprType.Bool },
                { Operator.LessOrEquals, ExprType.Bool },
                { Operator.More, ExprType.Bool },
                { Operator.MoreOrEquals, ExprType.Bool }
            };
            stringTypeBindings = new Dictionary<Operator, ExprType>()
            {
                { Operator.Plus, ExprType.String },
                { Operator.Equals, ExprType.Bool },
                { Operator.NotEquals, ExprType.Bool },
                { Operator.Less, ExprType.Bool },
                { Operator.LessOrEquals, ExprType.Bool },
                { Operator.More, ExprType.Bool },
                { Operator.MoreOrEquals, ExprType.Bool }
            };
            typeBindings = new Dictionary<ExprType, Dictionary<Operator, ExprType>>()
            {
                { ExprType.Int, intTypeBindings },
                { ExprType.Real, realTypeBindings },
                { ExprType.Bool, boolTypeBindings },
                { ExprType.String, stringTypeBindings }
            };
        }

        public ExprType FindOpProductionType(TypeInfo type1, TypeInfo type2, Operator op)
        {
            if (type1.BasicType == ExprType.Void || type2.BasicType == ExprType.Void ||
                type1.IsArray || type2.IsArray)
            {
                return ExprType.Void;
            }

            ExprType common = FindCommonType(type1.BasicType, type2.BasicType);
            if (common != ExprType.Void)
            {
                var opBindings = typeBindings[common];
                if (opBindings.ContainsKey(op))
                {
                    return opBindings[op];
                }
            }
            return ExprType.Void;

        }

        public ExprType FindOpProductionType(TypeInfo type, Operator op)
        {
            if (type.BasicType == ExprType.Void || type.IsArray)
            {
                return ExprType.Void;
            }

            var opBindings = typeBindings[type.BasicType];
            if (opBindings.ContainsKey(op))
            {
                return opBindings[op];
            }
            return ExprType.Void;
        }

        public bool TypeCheckAssign(TypeInfo varType, TypeInfo valueType)
        {
            if (varType.IsArray || valueType.IsArray)
            {
                return varType.SameAs(valueType);
            }

            ExprType common = FindCommonType(varType.BasicType, valueType.BasicType);
            if (common != ExprType.Void)
            {
                if (varType.BasicType == common)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public ExprType FindCommonType(ExprType type1, ExprType type2)
        {
            if (type1 == type2)
            {
                return type1;
            }
            else if ((type1 == ExprType.Int && type2 == ExprType.Real) || 
                     (type1 == ExprType.Real && type2 == ExprType.Int))
            {
                return ExprType.Real;
            }
            else
            {
                return ExprType.Void;
            }
        }
    }
}
