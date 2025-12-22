using Core.DTOs.AiService;
using Core.Services.Protos;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ResponseMessageConverter : JsonConverter<ResponseMessage>
{
    public override ResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 1. لو القيمة اللي جاية هي Object بجد { }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // هنستخدم الـ Serializer العادي يكمل شغله هنا
            return JsonSerializer.Deserialize<ResponseMessage>(ref reader, options);
        }

        // 2. لو القيمة اللي جاية هي مجرد String (سبب المشكلة)
        if (reader.TokenType == JsonTokenType.String)
        {
            return new ResponseMessage
            {
                Answer = reader.GetString(), // هنحط النص اللي جاي في خانة الـ Answer
                Sql_Query = null
            };
        }

        // 3. لو القيمة null أو أي حاجة تانية
        return new ResponseMessage();
    }

    public override void Write(Utf8JsonWriter writer, ResponseMessage value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}