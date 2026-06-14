using System.Reflection;
using System.Text;

namespace LookupEngine.Formaters;

/// <summary>
///     Formats reflection metadata into human-readable display strings.
/// </summary>
internal static class ReflexionFormater
{
    internal static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var typeName = type.Name;
        var apostropheIndex = typeName.IndexOf('`');
        if (apostropheIndex > 0) typeName = typeName[..apostropheIndex];
        typeName += "<";
        var genericArguments = type.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; i++)
        {
            typeName += FormatTypeName(genericArguments[i]);
            if (i < genericArguments.Length - 1) typeName += ", ";
        }

        typeName += ">";
        return typeName;
    }

    internal static string FormatTypeFullName(Type type, string typeName)
    {
        return type.Namespace is null ? typeName : $"{type.Namespace}.{typeName}";
    }

    internal static string FormatMemberName(MemberInfo member, ParameterInfo[] parameters)
    {
        if (parameters.Length == 0) return member.Name;

        var builder = new StringBuilder();
        builder.Append(member.Name);
        builder.Append(" (");

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterType = parameters[i].ParameterType;
            if (parameterType.IsByRef)
            {
                builder.Append("ref ");
                var name = FormatTypeName(parameterType).AsSpan();
                builder.Append(name[^1] == '&' ? name[..^1] : name);
            }
            else
            {
                builder.Append(FormatTypeName(parameterType));
            }

            if (i < parameters.Length - 1) builder.Append(", ");
        }

        builder.Append(')');
        return builder.ToString();
    }
}