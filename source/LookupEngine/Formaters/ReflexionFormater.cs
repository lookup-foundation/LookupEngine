using System.Reflection;

namespace LookupEngine.Formaters;

/// <summary>
///     Reflexion data formater
/// </summary>
internal static class ReflexionFormater
{
    /// <summary>
    ///     Format the type name for a generic object
    /// </summary>
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

    /// <summary>
    ///     Format the full type name, omitting the namespace separator for types without a namespace
    /// </summary>
    internal static string FormatTypeFullName(Type type, string typeName)
    {
        return type.Namespace is null ? typeName : $"{type.Namespace}.{typeName}";
    }

    /// <summary>
    ///     Format the name of a parametric member
    /// </summary>
    internal static string FormatMemberName(MemberInfo member, ParameterInfo[] parameters)
    {
        if (parameters.Length == 0) return member.Name;

        var formatedParameters = parameters.Select(info =>
        {
            return info.ParameterType.IsByRef switch
            {
                true => $"ref {FormatTypeName(info.ParameterType).Replace("&", string.Empty)}",
                false => FormatTypeName(info.ParameterType)
            };
        });

        return $"{member.Name} ({string.Join(", ", formatedParameters)})";
    }
}