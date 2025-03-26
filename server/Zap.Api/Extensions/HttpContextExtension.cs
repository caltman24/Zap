using Newtonsoft.Json;

namespace Zap.Api.Extensions;

public static class HttpContextExtension
{
    public static FormData<T> ParseMultipartForm<T>(this HttpContext context)
    {
        var form = context.Request.Form;
        
        var objectKey = form.Keys.ToArray()[0];
        var isValid = form.TryGetValue(objectKey, out var rawJson);

        if (!isValid) throw new Exception($"Failed to read object value from key \"{objectKey}\" ");

        var objectModel = JsonConvert.DeserializeObject<T>(rawJson);

        if (objectModel is null)
            throw new JsonSerializationException($"Failed to deserialize json object {nameof(objectModel)}");

        var file = form.Files.Count > 0 ? form.Files[0] : null;

        return new FormData<T>(file, objectModel);
    }
}

public record FormData<T>(IFormFile? File, T Data);