using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Enums
{
    /// <summary>
    /// A valid list of argument types that can be serialized
    /// </summary>
    public enum ArgumentType
    {
        String,
        Number,
        Enum,
        Object,
        Unknown
    }
}
