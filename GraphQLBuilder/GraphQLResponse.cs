using GraphQLBuilder.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder
{
    /// <summary>
    /// Used to take a JSON response from GraphQL and parse it into a .NET domain model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GraphQLResponse<T>
    {
        private string json;

        public GraphQLResponse(string _json)
        {
            json = _json;
        }

        /// <summary>
        /// Parses the response for a single object
        /// </summary>
        /// <returns></returns>
        public T Parse()
        {
            return new GraphQLDeserializer().Deserialize<T>(json);
        }

        /// <summary>
        /// Parses the response for an array of objects
        /// </summary>
        /// <returns></returns>
        public T[] ParseArray()
        {
            return new GraphQLDeserializer().DeserializeArray<T>(json);
        }
    }
}
