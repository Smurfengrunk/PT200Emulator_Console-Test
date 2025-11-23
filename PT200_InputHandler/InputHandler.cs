using System.Text.Json;
using System.Text.Json.Serialization;

namespace PT200_InputHandler
{
    public class PT200_InputHandler
    {
#pragma warning disable CS8604
        public InputMapper inputMapper { get; private set; }
        private string _basePath = new(AppDomain.CurrentDomain.BaseDirectory);

        public PT200_InputHandler()
        {
            string json = File.ReadAllText(Path.Combine(_basePath, "Data", "keymap.json"));
            JsonDocument doc = JsonDocument.Parse(json);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            KeyMap? wrapper = JsonSerializer.Deserialize<KeyMap>(json, options);
            inputMapper = new InputMapper(wrapper);

        }

        public class HexInt32Converter : JsonConverter<int>
        {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    string? s = reader.GetString();
                    if (s != null && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt32(s, 16);
                    return int.Parse(s);
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt32();
                }
                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
            {
                writer.WriteStringValue("0x" + value.ToString("X"));
            }
#pragma warning restore CS8604
        }
    }
}
