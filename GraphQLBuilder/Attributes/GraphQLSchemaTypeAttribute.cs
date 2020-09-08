using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Attributes
{
    public class GraphQLSchemaTypeAttribute : Attribute
    {
        public string SchemaType { get; set; }

        public GraphQLSchemaTypeAttribute(string name)
        {
            SchemaType = name;
        }
    }
}
