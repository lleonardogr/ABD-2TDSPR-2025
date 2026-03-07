using System.Text.Json.Serialization;

namespace _2TDSPR25;

[JsonConverter(typeof(JsonStringEnumConverter<DayOfWeekAsString>))]
public enum DayOfWeekAsString
{
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
}