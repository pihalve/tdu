using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pihalve.Tdu.Tool.Providers;

namespace Pihalve.Tdu.Tool
{
    public class UmbracoItemConverter : JsonConverter
    {
        private readonly IDictionary<Type, IUmbracoItemProvider> _providers;
        private readonly ILog _logger = LogManager.GetLogger("Tdu");

        public UmbracoItemConverter(IDictionary<Type, IUmbracoItemProvider> providers /*IDictionary<Type, IDictionary<int, Guid>> identifierMappingsList*/)
        {
            _providers = providers;
        }

        public override bool CanConvert(Type objectType)
        {
            // this converter can be applied to any type
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // we currently support only writing of JSON
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            bool useCamelCase = serializer.ContractResolver is CamelCasePropertyNamesContractResolver;

            var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance).ToList();

            writer.WriteStartObject();

            foreach (var property in properties)
            {
                if (IsValueType(property))
                {
                    WriteValueType(writer, property, value, useCamelCase);
                }
                else if (IsCollectionType(property))
                {
                    WriteCollection(writer, property, value, useCamelCase);
                }
                else if (IsObjectType(property))
                {
                    WriteObject(writer, property, value, useCamelCase);
                }
            }

            writer.WriteEndObject();
        }

        private void WriteValueType(JsonWriter writer, PropertyInfo property, object value, bool useCamelCase)
        {
            var propertyName = GetPropertyName(property.Name, useCamelCase);
            writer.WritePropertyName(propertyName);
            
            writer.WriteValue(property.GetValue(value));
        }

        private void WriteCollection(JsonWriter writer, PropertyInfo property, object value, bool useCamelCase)
        {
            var propertyName = GetPropertyName(property.Name, useCamelCase);

            //TODO: temporary ignore certain properties until we find a good solution for handling them
            if (propertyName.ToLower().Contains("propertygroup") ||
                propertyName.ToLower().Contains("propertytype")) return;
            
            writer.WritePropertyName(propertyName);
            
            //writer.WriteValue("[Collection]");
            IEnumerable propertyEnumerableValue;
            if (property.PropertyType.IsInterface)
            {
                propertyEnumerableValue = (IEnumerable)property.GetValue(value, BindingFlags.Instance, null, null, null);
            }
            else
            {
                propertyEnumerableValue = (IEnumerable)property.GetValue(value);
            }

            if (propertyEnumerableValue != null)
            {
                writer.WriteStartArray();
                foreach (var entity in propertyEnumerableValue)
                {
                    writer.WriteValue(GetKey(entity));
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteNull();
            }
        }

        private void WriteObject(JsonWriter writer, PropertyInfo property, object value, bool useCamelCase)
        {
            var propertyName = GetPropertyName(property.Name, useCamelCase);
            writer.WritePropertyName(string.Format("${0}", propertyName));

            object propertyValue;
            if (property.PropertyType.IsInterface)
            {
                propertyValue = property.GetValue(value, BindingFlags.Instance, null, null, null);
            }
            else
            {
                propertyValue = property.GetValue(value);
            }

            if (propertyValue != null)
            {

                writer.WriteValue(GetKey(propertyValue));
            }
            else
            {
                writer.WriteNull();
            }
        }

        private Guid GetKey(object value)
        {
            var id = GetId(value);

            var provider = GetProvider(value.GetType());
            var entity = provider.GetEntity(id);
            if (entity == null)
            {
                ThrowException("Unable to find Umbraco item with ID: '{0}'", id);
            }

            return entity.Key;

            //var identifierMappings = GetIdentifierMappingsByType(value.GetType());
            //if (!identifierMappings.ContainsKey(id))
            //{
            //    ThrowException("Identifier mapping not found for id: {0}", id);
            //}
            //return identifierMappings[id];
        }

        private int GetId(object value)
        {
            int id;
            var identifierProperty = value.GetType().GetProperty("Id", typeof(int));
            if (identifierProperty != null)
            {
                id = (int)identifierProperty.GetValue(value);
            }
            else
            {
                identifierProperty = value.GetType().GetProperty("Id", typeof(Lazy<int>));
                if (identifierProperty == null)
                {
                    ThrowException("Object does not contain 'Id' property");
                }

                var lazyId = (Lazy<int>)identifierProperty.GetValue(value);
                if (lazyId == null)
                {
                    ThrowException("'Id' property of object is invalid");
                }

                id = lazyId.Value;
            }

            return id;
        }

        //private IDictionary<int, Guid> GetIdentifierMappingsByType(Type type)
        //{
        //    if (!_identifierMappingsList.ContainsKey(type))
        //    {
        //        ThrowException("Identifier mappings not found for type: {0}", type.Name);
        //    }

        //    return _identifierMappingsList[type];
        //}

        private IUmbracoItemProvider GetProvider(Type type)
        {
            if (!_providers.ContainsKey(type))
            {
                ThrowException("Umbraco item provider not found for type: {0}", type.Name);
            }

            return _providers[type];
        }

        private static string GetPropertyName(string name, bool useCamelCase)
        {
            if (useCamelCase && name != null)
            {
                if (name.Length > 1)
                {
                    return char.ToLower(name[0]) + name.Substring(1);
                }
                if (name.Length > 0)
                {
                    return char.ToLower(name[0]).ToString();
                }
            }
            return name;
        }

        private static bool IsValueType(PropertyInfo property)
        {
            return 
                property.PropertyType.IsValueType || 
                property.PropertyType == typeof(string);
        }

        private static bool IsCollectionType(PropertyInfo property)
        {
            return property.PropertyType != typeof(string) &&
                (property.PropertyType.IsArray ||
                 typeof (IEnumerable).IsAssignableFrom(property.PropertyType) ||
                 property.PropertyType.GetInterface(typeof (IEnumerable<>).FullName) != null);
        }

        private static bool IsObjectType(PropertyInfo property)
        {
            return property.PropertyType != typeof(string) &&
                (property.PropertyType.IsClass || 
                 property.PropertyType.IsInterface);
        }

        [ContractAnnotation("halt <=")]
        private void ThrowException(string formatMessage, params object[] parameters)
        {
            var message = string.Format(formatMessage, parameters);
            _logger.FatalFormat(message);
            throw new Exception(message);
        }
    }
}
