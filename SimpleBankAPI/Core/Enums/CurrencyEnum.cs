using System.Runtime.Serialization;

namespace SimpleBankAPI.Core.Enums;

public enum Currency
{
    [EnumMember(Value = "USD")]
    USD,
    [EnumMember(Value = "EUR")]
    EUR
}

