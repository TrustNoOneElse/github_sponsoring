using System.Text;
using System.Text.Json;

namespace GithubSponsorsWebhook.Utils;
public static class JsonDocumentExtension
{
    public static string ToJsonString(this JsonDocument json)
    {
        using (var stream = new MemoryStream())
        {
            Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            json.WriteTo(writer);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}