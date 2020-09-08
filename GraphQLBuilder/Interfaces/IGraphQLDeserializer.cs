using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace GraphQLBuilder.Interfaces
{
    public interface IGraphQLDeserializer
    {
        T Deserialize<T>(string data);
        T[] DeserializeArray<T>(string data);
        object DeserializeObject<T>(Dictionary<string, JsonElement> document);
        void DeserializeMember(JsonElement value, PropertyInfo modelProperty, object model);
    }
}
