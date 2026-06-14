using System.Reflection;
using LookupEngine.Abstractions.Enums;

namespace LookupEngine.Formaters;

/// <summary>
///     Converts reflection metadata into <see cref="MemberAttributes"/> flags.
/// </summary>
internal static class ModifiersFormater
{
    internal static MemberAttributes FormatAttributes(MemberInfo member)
    {
        return member switch
        {
            MethodInfo info => CombineModifiers(MemberAttributes.Method, info.Attributes),
            PropertyInfo info => CombineModifiers(MemberAttributes.Property, info.CanRead ? info.GetMethod!.Attributes : info.SetMethod!.Attributes),
            FieldInfo info => CombineModifiers(MemberAttributes.Field, info.Attributes),
            EventInfo info => CombineModifiers(MemberAttributes.Event, info.AddMethod!.Attributes),
            _ => throw new ArgumentOutOfRangeException(nameof(member))
        };
    }

    private static MemberAttributes CombineModifiers(MemberAttributes attributes, MethodAttributes methodAttributes)
    {
        if ((methodAttributes & MethodAttributes.Static) != 0) attributes |= MemberAttributes.Static;
        if ((methodAttributes & MethodAttributes.Private) != 0) attributes |= MemberAttributes.Private;
        return attributes;
    }

    private static MemberAttributes CombineModifiers(MemberAttributes attributes, FieldAttributes fieldAttributes)
    {
        if ((fieldAttributes & FieldAttributes.Static) != 0) attributes |= MemberAttributes.Static;
        if ((fieldAttributes & FieldAttributes.Private) != 0) attributes |= MemberAttributes.Private;
        return attributes;
    }
}