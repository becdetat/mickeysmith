using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace JsonDatabaseTestbed
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object args)
        {
            if (args == null)
            {
                return new Dictionary<string, object>();
            }

            return TypeDescriptor.GetProperties(args).Cast<PropertyDescriptor>()
                .ToDictionary(
                    property => property.Name,
                    property => property.GetValue(args));
        }
    }
}