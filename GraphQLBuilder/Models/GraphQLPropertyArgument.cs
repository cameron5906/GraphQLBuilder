using GraphQLBuilder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQLBuilder.Models
{
    public class GraphQLPropertyArgument
    {
        public ArgumentType Type { get { return GetArgumentType(); } }
        public string ArgumentName { get; set; }
        public object Value { get; set; }

        private ArgumentType GetArgumentType()
        {
            if(Value is string)
            {
                return ArgumentType.String;
            } else if(new Type[] { typeof(double), typeof(float), typeof(short), typeof(int), typeof(long)}.Contains(Value.GetType()))
            {
                return ArgumentType.Number;
            } else if(Value is Enum)
            {
                return ArgumentType.Enum;
            } else if(Value.GetType().IsClass)
            {
                return ArgumentType.Object;
            }

            return ArgumentType.Unknown;
        }
    }
}
