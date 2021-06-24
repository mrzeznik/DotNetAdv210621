using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LevelUpCSharp.Reflection
{
    public static class ObjectExtensions
    {
        private static void SetField<TSource>(this TSource source, string name, object value)
        {
            var field = source.GetType().BaseType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(source, value);
        }

        private static void SetProperty<TSource>(this TSource source, string name, object value)
        {
            var type = typeof(TSource);
            var property = type.GetProperty(name);

            var backingField = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(field =>
                    field.Attributes.HasFlag(FieldAttributes.Private) &&
                    field.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute)) &&
                    (field.DeclaringType == property.DeclaringType) &&
                    field.FieldType.IsAssignableFrom(property.PropertyType) &&
                    field.Name.StartsWith("<" + property.Name + ">", StringComparison.InvariantCulture)
                );
            backingField.SetValue(source, value);
        }
    }
}
