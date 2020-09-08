using GraphQLBuilder.Attributes;
using GraphQLBuilder.Enums;
using GraphQLBuilder.Exceptions;
using GraphQLBuilder.Interfaces;
using GraphQLBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQLBuilder.Serialization
{
    internal sealed class GraphQLSerializer : IGraphQLSerializer
    {
        /// <summary>
        /// Generates a full GraphQL query from the provided GraphQLQueryable
        /// </summary>
        /// <typeparam name="T">The model type to serialize</typeparam>
        /// <param name="query">The structured GraphQLQueryable</param>
        /// <returns>An instance of model T</returns>
        public string GenerateQuery<T>(Interfaces.IQueryableOf<T> query)
        {
            var allQueries = new Interfaces.IQueryable[] { query }.Concat(query.GetSiblings());
            var queryStringBuilder = new StringBuilder();
            int rootIndentation = 1;

            if (query.GetOperationName() != null)
            {
                queryStringBuilder.AppendLine($"query {query.GetOperationName()} {{");
            } else if(allQueries.Count() > 1)
            {
                queryStringBuilder.AppendLine("{");
            } else
            {
                rootIndentation = 0;
            }

            if (allQueries.Count() > 1)
            {
                //Search for multiple queries that share the same schema type name and do not have different aliases
                var ambiguousFields = allQueries
                    .GroupBy(x => x.GetTypeName())
                    .Where(group =>
                        group.SelectMany(q => q.GetAlias() ?? "")
                        .Distinct()
                        .Count() < group.Count()
                    )
                    .ToArray();

                //If any were found, throw an exception. In GraphQL, you must alias field queries that are the same
                if (ambiguousFields.Length > 0)
                {
                    throw new AmbiguousFieldsException(ambiguousFields[0].Select(x => x.GetTypeName()).ToArray());
                }
            }

            //Loop through all of the IQueryable instances. This may be more than one if a With() was used.
            foreach (var subQuery in allQueries)
            {
                //Convert the known GraphQL properties of this type to GraphQLProperty models
                var propFieldMapping = subQuery.GetProperties()
                    .Select(x =>
                    {
                        var propertyInfo = typeof(T).GetProperty(x.Key);
                        return new GraphQLProperty(x.Key, x.Value, propertyInfo);
                    })
                    .ToList();

                var queryType = subQuery.GetTypeName();

                queryStringBuilder.Append(Indent(rootIndentation));
                
                //If an alias was provided for this query, insert it
                if(subQuery.GetAlias() != null)
                {
                    queryStringBuilder.Append($"{subQuery.GetAlias()}: ");
                }
                
                //Add the query type name along with any arguments that may be attached
                queryStringBuilder.Append(queryType);
                queryStringBuilder.AppendLine($"{GenerateArguments<T>(subQuery, string.Empty)} {{");

                //Loop through each property of the type and generate markup for it
                foreach (var property in propFieldMapping)
                {
                    queryStringBuilder.Append(GenerateProperty<T>(subQuery, property, rootIndentation + 1));
                }

                queryStringBuilder.AppendLine($"{Indent(rootIndentation)}}}");
            }

            if(query.GetOperationName() != null || allQueries.Count() > 1)
            {
                queryStringBuilder.AppendLine("}");
            }

            return queryStringBuilder.ToString();
        }

        /// <summary>
        /// Generates a snippet for a schema type to be used in query serialization
        /// </summary>
        /// <typeparam name="T">The model to serialize</typeparam>
        /// <param name="query">The queryable chain</param>
        /// <param name="property">The property to serialize</param>
        /// <param name="indentationLevel">The indentation level, used for formatting</param>
        /// <returns>A string snippet to be used when building the complete query</returns>
        public string GenerateProperty<T>(Interfaces.IQueryable query, GraphQLProperty property, int indentationLevel)
        {
            var sb = new StringBuilder();
            
            switch(property.Type)
            {
                case PropertyType.GraphQLType:
                    sb.AppendLine($"{Indent(indentationLevel)}{property.FieldName}{GenerateArguments<T>(query, property.OriginalPropertyName)} {{");
                    foreach(var subProperty in property.Properties)
                    {
                        sb.Append(GenerateProperty<T>(query, subProperty, indentationLevel + 1));
                    }
                    sb.AppendLine($"{Indent(indentationLevel)}}}");
                    break;
                case PropertyType.String:
                case PropertyType.Number:
                case PropertyType.Enum:
                    sb.AppendLine($"{Indent(indentationLevel)}{property.FieldName}{GenerateArguments<T>(query, property.OriginalPropertyName)}");
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a text snippet of arguments to be used in a query
        /// </summary>
        /// <typeparam name="T">The model to serialize</typeparam>
        /// <param name="query">The queryable chain</param>
        /// <param name="propertyName">The property to derive arguments from the queryable with</param>
        /// <returns>A string snippet that is appended onto a line in the query, or blank if no arguments were found</returns>
        public string GenerateArguments<T>(Interfaces.IQueryable query, string propertyName)
        {
            var arguments = query.GetArguments(propertyName);
            if (arguments == null) return "";

            var sb = new StringBuilder();
            sb.Append("(");

            foreach(var argument in query.GetArguments(propertyName))
            {
                if (argument.Type == ArgumentType.Unknown) continue;

                sb.Append($"{argument.ArgumentName}: ");

                switch(argument.Type)
                {
                    case ArgumentType.String:
                        sb.Append($"\"{argument.Value}\"");
                        break;
                    case ArgumentType.Number:
                        sb.Append(argument.Value);
                        break;
                    case ArgumentType.Enum:
                        sb.Append(Enum.GetName(argument.Value.GetType(), argument.Value));
                        break;
                }

                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Utility function which indents by X amount of spaces
        /// </summary>
        /// <param name="number">The number of indentation levels (like tab characters)</param>
        /// <returns>A string of spaces representing a tab</returns>
        public string Indent(int number)
        {
            return string.Concat(Enumerable.Repeat(" ", number * 4));
        }
    }
}
