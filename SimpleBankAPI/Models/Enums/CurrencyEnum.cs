using System.ComponentModel;
using System.Runtime.Serialization;

namespace SimpleBankAPI.Models.Enums;

public enum CurrencyEnum
{
    [EnumMember(Value = "USD")]
    Dollar,
    [EnumMember(Value = "EUR")]
    Euro
}