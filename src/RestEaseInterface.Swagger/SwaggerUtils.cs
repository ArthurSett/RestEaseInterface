using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace RestEaseInterface.Swagger;

public static class SwaggerUtils
{
    public static OpenApiSchema GetSchemaForType(this Type type)
    {
        var schema = new OpenApiSchema();
        // Check if the type is a nullable type and set the schema type accordingly
        if (Nullable.GetUnderlyingType(type) != null)
        {
            schema.Type = "null";
            schema.Nullable = true;
            type = Nullable.GetUnderlyingType(type);
        }

        // Set the schema type based on the type of the parameter
        if (type == typeof(bool))
        {
            schema.Type = "boolean";
        }
        else if (type == typeof(byte))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(sbyte))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(short))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(ushort))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(int))
        {
            schema.Type = "integer";
            schema.Format = "int32";
        }
        else if (type == typeof(uint))
        {
            schema.Type = "integer";
            schema.Format = "int64";
        }
        else if (type == typeof(long))
        {
            schema.Type = "integer";
            schema.Format = "int64";
        }
        else if (type == typeof(ulong))
        {
            schema.Type = "integer";
            schema.Format = "int64";
        }
        else if (type == typeof(float))
        {
            schema.Type = "number";
            schema.Format = "float";
        }
        else if (type == typeof(double))
        {
            schema.Type = "number";
            schema.Format = "double";
        }
        else if (type == typeof(decimal))
        {
            schema.Type = "number";
            schema.Format = "decimal";
        }
        else if (type == typeof(DateTime))
        {
            schema.Type = "string";
            schema.Format = "date-time";
        }
        else if (type == typeof(string))
        {
            schema.Type = "string";
        }
        else if (type.IsArray)
        {
            schema.Type = "array";
            schema.Items = GetSchemaForType(type.GetElementType());
        }
        else if (type.IsEnum)
        {
            schema.Type = "enum";
            var enums = Enum.GetNames(type);
            schema.Enum = enums.Select(x => new OpenApiString(x)).ToArray();
        }
        else
        {
            schema.Type = "object";
            schema.Properties = new Dictionary<string, OpenApiSchema>();

            // Get the properties of the object and add them to the schema
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                schema.Properties.Add(property.Name, GetSchemaForType(property.PropertyType));
            }
        }

        return schema;
    }
}