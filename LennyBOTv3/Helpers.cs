using System.Text.Json;
using System.Xml.Serialization;

namespace LennyBOTv3
{
    internal static class Helpers
    {
        /// <summary>
        /// Build safe (escaped) url using string interpolation
        /// </summary>
        /// <param name="url">interpolated url string</param>
        /// <returns>Safe (escaped) url</returns>
        /// <remarks>https://medium.com/@j2jensen/c-6s-string-interpolation-feature-is-actually-great-for-encoding-parameters-ab139471b133</remarks>
        public static string BuildSafeUrl(FormattableString url)
        {
            var invariantParameters = url.GetArguments()
              .Select(a => FormattableString.Invariant($"{a}"));
            var escapedParameters = invariantParameters
              .Select(Uri.EscapeDataString)
              .Cast<object?>()
              .ToArray();

            // check for somewhat valid URL
            // not perfect, but good enough https://stackoverflow.com/questions/7578857/how-to-check-whether-a-string-is-a-valid-http-url#comment80682416_7581824
            var uri = new Uri(string.Format(url.Format, escapedParameters), UriKind.Absolute);

            if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                return uri.AbsoluteUri;

            throw new UriFormatException($"'{url}' is not a valid URL");
        }

        public static async Task<T?> GetFromJsonAsync<T>(FormattableString url) where T : class
        {
            var jsonStr = await GetStringAsync(url);

            if (string.IsNullOrEmpty(jsonStr))
                throw new JsonException($"{url} returned empty string");

            return JsonSerializer.Deserialize<T>(jsonStr);
        }

        public static async Task<T?> GetFromXmlAsync<T>(FormattableString url) where T : class
        {
            var xmlStr = await GetStringAsync(url);

            if (string.IsNullOrEmpty(xmlStr))
                throw new JsonException($"{url} returned empty string");

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xmlStr);
            return serializer.Deserialize(reader) as T;
        }

        public static async Task<string?> GetStringAsync(FormattableString url)
        {
            var safeUrl = BuildSafeUrl(url);
            using var client = new HttpClient();
            var result = await client.GetAsync(safeUrl);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }
    }
}
