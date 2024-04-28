using Newtonsoft.Json;

namespace GeneralLibrary.Base.Kafka.Converters;

public class CustomeTimeSpanConverter : JsonConverter<TimeSpan>
{
    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return TimeSpan.Parse((string)reader.Value ?? string.Empty);
    }
}