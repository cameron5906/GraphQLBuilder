using GraphQLBuilder.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Interfaces
{
    public interface IQueryable
    {
        IQueryable With(IQueryable other);
        IQueryable[] GetSiblings();

        string GetAlias();
        string GetTypeName();
        string GetOperationName();
        IDictionary<string, string> GetProperties();
        IList<GraphQLPropertyArgument> GetArguments(string propertyName);
    }
}
