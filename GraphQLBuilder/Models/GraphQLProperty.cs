using GraphQLBuilder.Attributes;
using GraphQLBuilder.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GraphQLBuilder.Models
{
    /// <summary>
    /// Represents a mappable .NET to GraphQL property and its metadata used to serialize
    /// </summary>
    public class GraphQLProperty
    {
        public string OriginalPropertyName { get; set; }
        public string FieldName { get; set; }
        public PropertyType Type { get; set; }

        public GraphQLProperty[] Properties { get; set; }

        /// <summary>
        /// Initializes private variables and determines the Property Type
        /// </summary>
        /// <param name="propertyName">The .NET name of the property</param>
        /// <param name="name">The mapped schema field name of the property</param>
        /// <param name="property">The property information</param>
        public GraphQLProperty(string propertyName, string name, PropertyInfo property)
        {
            OriginalPropertyName = propertyName;
            FieldName = name;

            if (property.PropertyType == typeof(string))
            {
                Type = PropertyType.String;
            } else if (new Type[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double) }.Contains(property.PropertyType)) //TODO: make helper function
            {
                Type = PropertyType.Number;
            } else if(property.PropertyType.IsEnum)
            {
                Type = PropertyType.Enum;
            } else if(property.PropertyType.IsClass)
            {
                var properties = property.PropertyType
                    .GetProperties()
                    .Where(x => 
                        x.CustomAttributes.Any(attr => 
                            attr.AttributeType == typeof(GraphQLPropertyAttribute)
                        )
                    )
                    .ToList();

                if(properties.Count > 0)
                {
                    Type = PropertyType.GraphQLType;
                    Properties = properties
                        .Select(x =>
                        {
                            var fieldName = x.GetCustomAttribute<GraphQLPropertyAttribute>().FieldName;
                            return new GraphQLProperty(x.Name, fieldName, x);
                        })
                        .ToArray();
                } else
                {
                    Type = PropertyType.Object;
                }
            } else
            {
                Type = PropertyType.Unknown;
            }
        }
    }
}
