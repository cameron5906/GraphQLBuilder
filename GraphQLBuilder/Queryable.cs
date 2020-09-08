using GraphQLBuilder.Attributes;
using GraphQLBuilder.Enums;
using GraphQLBuilder.Interfaces;
using GraphQLBuilder.Models;
using GraphQLBuilder.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace GraphQLBuilder
{
    public abstract class Queryable<T> : Interfaces.IQueryableOf<T>
    {
        private string typeName;
        private string alias;
        private string operationName;
        private IList<Interfaces.IQueryable> siblings;
        private IDictionary<string, IList<GraphQLPropertyArgument>> arguments;
        private IDictionary<string, string> propertyNames;

        /// <summary>
        /// Initializes private variables and loads in the properties that exist on Type T that contain GraphQL attributes
        /// </summary>
        public Queryable()
        {
            typeName = typeof(T).Name.ToLower();
            
            if(typeof(T).CustomAttributes.Any(x => x.AttributeType == typeof(GraphQLSchemaTypeAttribute)))
            {
                typeName = typeof(T).GetCustomAttribute<GraphQLSchemaTypeAttribute>().SchemaType;
            }

            siblings = new List<Interfaces.IQueryable>();
            arguments = new Dictionary<string, IList<GraphQLPropertyArgument>>();
            propertyNames = typeof(T)
                .GetProperties()
                .Where(prop =>
                    prop.CustomAttributes.Any(attr =>
                        attr.AttributeType == typeof(GraphQLPropertyAttribute)
                    ) &&
                    !prop.CustomAttributes.Any(attr => 
                        attr.AttributeType == typeof(GraphQLIgnoreAttribute)
                    )
                )
                .ToDictionary(x => x.Name, x => x.GetCustomAttribute<GraphQLPropertyAttribute>().FieldName);
        }

        /// <summary>
        /// Used to map the model name to a GraphQL schema type if the naming convention cannot be implicitly derived
        /// </summary>
        /// <param name="type">The schema type name to use</param>
        public Interfaces.IQueryableOf<T> OfType(string type)
        {
            typeName = type;
            return this;
        }

        public Interfaces.IQueryableOf<T> WithAlias(string _alias)
        {
            alias = _alias;
            return this;
        }

        /// <summary>
        /// Used to create an alias for a query section if there is multiple queries for the same schema type
        /// </summary>
        /// <param name="alias">The name to alias the query with</param>
        public Interfaces.IQueryable With(Interfaces.IQueryable other)
        {
            siblings.Add(other);
            return this;
        }

        /// <summary>
        /// Used to name a GraphQL query operation, good for production use to better understand the purpose of a query.
        /// </summary>
        /// <param name="name">The name of the operation</param>
        public Interfaces.IQueryableOf<T> AsOperation(string name)
        {
            operationName = name;
            return this;
        }

        /// <summary>
        /// Used to map a property on the model which does not have a GraphQLAttribute on it so that it is included in the query.
        /// </summary>
        /// <param name="property">The property to map</param>
        /// <param name="mappedField">The schema name of this property</param>
        public Interfaces.IQueryableOf<T> WithMapping(Expression<Func<T, object>> property, string mappedField)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;

            propertyNames.Add(propertyInfo.Name, mappedField);
            return this;
        }

        /// <summary>
        /// Used to include a query argument on the root schema type
        /// </summary>
        /// <param name="argumentName">The name of the argument</param>
        /// <param name="value">The value to pass to the resolver</param>
        public Interfaces.IQueryableOf<T> WithArgument(string argumentName, object value)
        {
            if (!arguments.ContainsKey(string.Empty))
            {
                arguments.Add(string.Empty, new List<GraphQLPropertyArgument>());
            }

            arguments[string.Empty].Add(new GraphQLPropertyArgument()
            {
                ArgumentName = argumentName,
                Value = value
            });

            return this;
        }

        /// <summary>
        /// Used to include a query argument on a field of a schema type
        /// </summary>
        /// <param name="property">The property that represents a field to apply the argument to</param>
        /// <param name="argumentName">The name of the argument</param>
        /// <param name="value">The value to pass to the resolver</param>
        public Interfaces.IQueryableOf<T> WithArgument(Expression<Func<T, object>> property, string argumentName, object value)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;

            if (!arguments.ContainsKey(propertyInfo.Name))
            {
                arguments.Add(propertyInfo.Name, new List<GraphQLPropertyArgument>());
            }

            arguments[propertyInfo.Name].Add(new GraphQLPropertyArgument()
            {
                ArgumentName = argumentName,
                Value = value
            });

            return this;
        }

        /// <summary>
        /// Used to ignore a property which has a GraphQLAttribute on it to exclude it from a query
        /// </summary>
        /// <param name="property">The property to ignore from serialization</param>
        public Interfaces.IQueryableOf<T> ShouldIgnore(Expression<Func<T, object>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;

            if(propertyNames.ContainsKey(propertyInfo.Name))
            {
                propertyNames.Remove(propertyInfo.Name);
            }
            return this;
        }

        /// <summary>
        /// Retrieves the name of the query operation, if any
        /// </summary>
        /// <returns></returns>
        public string GetOperationName()
        {
            return operationName;
        }

        public string GetAlias()
        {
            return alias;
        }

        /// <summary>
        /// Retrieves the schema type name for the model, specified or inferred
        /// </summary>
        /// <returns></returns>
        public string GetTypeName()
        {
            return typeName;
        }

        /// <summary>
        /// Retrieves a mapping of .NET property name to schema field name
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetProperties()
        {
            return propertyNames;
        }

        /// <summary>
        /// Retrieves arguments specified in the GraphQLQueryable
        /// </summary>
        /// <param name="propertyName">The property to retrieve arguments for. Leave blank to retrieve arguments applied to the root schema type</param>
        /// <returns></returns>
        public IList<GraphQLPropertyArgument> GetArguments(string propertyName)
        {
            return arguments.ContainsKey(propertyName) ? arguments[propertyName] : null;
        }

        public Interfaces.IQueryable[] GetSiblings()
        {
            return siblings.ToArray();
        }

        /// <summary>
        /// Generates a resulting GraphQL Query
        /// </summary>
        public override string ToString()
        {
            return new GraphQLSerializer().GenerateQuery(this);
        }
    }
}
