using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace SimpleBankAPI.Core.Enums;

public static class EnumHelper
{
    public static string? GetEnumMemberValue<T>(this T value) where T : Enum
    {
        return typeof(T)
            .GetTypeInfo()
            .DeclaredMembers
            .SingleOrDefault(x => x.Name == value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>(false)
            ?.Value;
    }

    public static string GetEnumDescription<T>(this T value) where T : Enum
    {
        var name = value.ToString();
        var memberInfo = value.GetType().GetMember(name)[0];
        var descriptionAttributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);

        if (!descriptionAttributes.Any())
            return name;

        return (descriptionAttributes[0] as DescriptionAttribute).Description;
    }
    
}
