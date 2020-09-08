using GraphQLBuilder.Enums;
using GraphQLBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace GraphQLBuilder.Interfaces
{
    public interface IQueryableOf<T> : IQueryable
    {
        IQueryableOf<T> OfType(string name);
        IQueryableOf<T> WithAlias(string alias);
        IQueryableOf<T> AsOperation(string name);
        IQueryableOf<T> WithMapping(Expression<Func<T, object>> property, string mappedField);
        IQueryableOf<T> WithArgument(string argumentName, object value);
        IQueryableOf<T> WithArgument(Expression<Func<T, object>> property, string argumentName, object value);
        IQueryableOf<T> ShouldIgnore(Expression<Func<T, object>> property);
    }
}
