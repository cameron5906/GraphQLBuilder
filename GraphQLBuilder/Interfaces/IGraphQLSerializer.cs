using GraphQLBuilder.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Interfaces
{
    public interface IGraphQLSerializer
    {
        string GenerateQuery<T>(Interfaces.IQueryableOf<T> query);
        string GenerateProperty<T>(Interfaces.IQueryable query, GraphQLProperty property, int indentationLevel);
        string GenerateArguments<T>(Interfaces.IQueryable query, string propertyName);
        string Indent(int number);
    }
}
