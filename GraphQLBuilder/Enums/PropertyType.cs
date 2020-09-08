using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Enums
{
    /// <summary>
    /// A valid list of field types that can be serialized
    /// </summary>
    public enum PropertyType
    {
        String,
        Number,
        Enum,
        Object,
        GraphQLType,
        Unknown
    }
}
