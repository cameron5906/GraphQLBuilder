using GraphQLBuilder.Attributes;
using GraphQLBuilder.Interfaces;
using GraphQLBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace GraphQLBuilder.Serialization
{
    internal sealed class GraphQLDeserializer : IGraphQLDeserializer
    {
        /// <summary>
        /// Provided a domain model type, parses a JSON response into an instance of the provided type
        /// </summary>
        /// <typeparam name="T">The domain model to map to</typeparam>
        /// <param name="data">JSON response from GraphQL API</param>
        /// <returns>An instance of the expected type</returns>
        public T Deserialize<T>(string data)
        {
            var document = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data);
            var documentEntity = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(document[typeof(T).Name.ToLower()].ToString());
            return (T)DeserializeObject<T>(documentEntity);
        }

        /// <summary>
        /// Provided a base type, parses a JSON response into an array of the provided type
        /// </summary>
        /// <typeparam name="T">The base type to map</typeparam>
        /// <param name="data">JSON response from GraphQL API</param>
        /// <returns>An array instance of expected type</returns>
        public T[] DeserializeArray<T>(string data)
        {
            var document = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(data);
            var docArray = document[typeof(T).Name.ToLower()].EnumerateArray();
            var models = new List<T>();
            foreach (var doc in docArray)
            {
                var docDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(doc.ToString());
                models.Add((T)DeserializeObject<T>(docDictionary));
            }

            return (T[])models.ToArray();
        }

        /// <summary>
        /// Deserializes an object from a GraphQL response to the expected type
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="document">The JSON object</param>
        public object DeserializeObject<T>(Dictionary<string, JsonElement> document)
        {
            var model = Activator.CreateInstance(typeof(T));

            var properties = model.GetType()
                .GetProperties()
                .Where(property => property.CustomAttributes.Any(attr => attr.AttributeType == typeof(GraphQLPropertyAttribute)))
                .Select(x =>
                {
                    var fieldName = x.GetCustomAttribute<GraphQLPropertyAttribute>().FieldName;
                    return new GraphQLProperty(x.Name, fieldName, x);
                })
                .ToList();

            foreach (var property in properties)
            {
                if (!document.ContainsKey(property.FieldName)) continue;
                DeserializeMember(document[property.FieldName], model.GetType().GetProperty(property.OriginalPropertyName), model);
            }

            return model;
        }

        /// <summary>
        /// A potentially recursive function that deserializes a property from JSON to its .NET equivalent type.
        /// For properties that are class types, this will recurse and deserialize that type as well if there are any valid GraphQL properties
        /// </summary>
        /// <param name="value">The JSON element to deserialize</param>
        /// <param name="modelProperty">The destination model property</param>
        /// <param name="model">The model that owns the property being deserialized</param>
        public void DeserializeMember(JsonElement value, PropertyInfo modelProperty, object model)
        {
            if (modelProperty.PropertyType == typeof(string))
            {
                modelProperty.SetValue(model, value.GetString());
            }
            else if (modelProperty.PropertyType == typeof(double))
            {
                modelProperty.SetValue(model, value.GetDouble());
            }
            else if (modelProperty.PropertyType == typeof(short))
            {
                modelProperty.SetValue(model, value.GetInt16());
            }
            else if (modelProperty.PropertyType == typeof(int))
            {
                modelProperty.SetValue(model, value.GetInt32());
            }
            else if (modelProperty.PropertyType == typeof(long))
            {
                modelProperty.SetValue(model, value.GetInt64());
            }
            else if (modelProperty.PropertyType.IsEnum)
            {
                modelProperty.SetValue(model, Enum.Parse(modelProperty.PropertyType, value.GetRawText()));
            }
            else if (modelProperty.PropertyType.IsClass)
            {
                var properties = modelProperty.PropertyType
                    .GetProperties()
                    .Where(property => property.CustomAttributes.Any(attr => attr.AttributeType == typeof(GraphQLPropertyAttribute)))
                    .ToList();

                foreach (var property in properties)
                {
                    var fieldName = property.GetCustomAttribute<GraphQLPropertyAttribute>().FieldName;
                    var propModel = Activator.CreateInstance(modelProperty.PropertyType);
                    modelProperty.SetValue(model, propModel);
                    DeserializeMember(value.GetProperty(fieldName), property, propModel);
                }
            }
        }
    }
}
