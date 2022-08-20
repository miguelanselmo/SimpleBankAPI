using System.ComponentModel;
using System.Runtime.Serialization;
using System.Reflection;

namespace SimpleBankAPI.Core.Enums;

public enum Currency
{
    [EnumMember(Value = "USD")]
    USD,
    [EnumMember(Value = "EUR")]
    EUR
}

