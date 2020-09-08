using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLBuilder.Exceptions
{
    public class AmbiguousFieldsException : Exception
    {
        public AmbiguousFieldsException(string[] fieldNames)
            :base($"The following fields in the query are amibuous and need aliases: {string.Join(", ", fieldNames)}")
        {

        }
    }
}
