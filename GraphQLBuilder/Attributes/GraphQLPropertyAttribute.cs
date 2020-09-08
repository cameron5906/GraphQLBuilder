using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Attributes
{
    public class GraphQLPropertyAttribute : Attribute
    {
        public string FieldName { get; set; }

        public GraphQLPropertyAttribute(string name)
        {
            FieldName = name;
        }
    }
}
