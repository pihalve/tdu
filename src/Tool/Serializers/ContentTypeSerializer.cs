using System;
using System.Reflection;
using System.Text;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Pihalve.Tdu.Tool.Serializers
{
    public class ContentTypeSerializer : SerializerBase, ISerializer
    {
        public string Serialize(IEntity entity)
        {
            var output = new StringBuilder();
            output.AppendLine("{");
            var propList = entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in propList)
            {
                if (output.Length > 0)
                {
                    output.Append(",\r\n");
                }
                output.AppendFormat("  \"{0}\": ", prop.Name);
                var propType = prop.PropertyType;
                if (propType == typeof(string))
                {
                    output.AppendFormat("\"{0}\"", prop.GetValue(entity));
                }
                else if (propType == typeof (DateTime))
                {
                    output.AppendFormat("\"{0}\"", ((DateTime)prop.GetValue(entity)).ToString("O"));
                }
                else
                {
                    output.AppendFormat("{0}", prop.GetValue(entity));
                }
            }
            output.AppendLine("\r\n}");
            return output.ToString();
        }

        public bool CanSerialize(IEntity entity)
        {
            return entity is ContentType;
        }
    }
}
