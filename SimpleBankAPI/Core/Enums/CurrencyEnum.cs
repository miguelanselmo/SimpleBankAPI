using System.ComponentModel;
using System.Runtime.Serialization;

namespace SimpleBankAPI.Core.Enums;

public enum CurrencyEnum
{
    [EnumMember(Value = "USD")]
    USD,
    [EnumMember(Value = "EUR")]
    EUR
}

