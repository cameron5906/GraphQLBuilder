using GraphQLBuilder.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphQLBuilder
{
    /// <summary>
    /// Used to craft GraphQL queries when provided a .NET domain model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GraphQLQuery<T> : Queryable<T>
    {
        public GraphQLQuery()
        {

        }

        /// <summary>
        /// Finalizes the query and builds the text
        /// </summary>
        /// <returns>A formatted GraphQL query to send to the server</returns>
        public string Build()
        {
            return new GraphQLSerializer().GenerateQuery<T>(this);
        }
    }
}
