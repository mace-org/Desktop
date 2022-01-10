using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Desktop.Infrastructures.QWeather
{
    public class QWeatherClient
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public QWeatherClient(string baseUri, string key, string locationId)
        {
            BaseUri = new Uri(baseUri);
            Key = key;
            Location = locationId;
        }

        public QWeatherClient(string baseUri, string key, double latitude, double longitude)
            : this(baseUri, key, $"{longitude},{latitude}")
        {
        }

        public Task<QWNowResult> GetNowAsync()
        {
            return GetAsync<QWNowResult>("now");
        }

        private Uri BaseUri { get; }
        private string Key { get; }
        private string Location { get; }

        private async Task<T> GetAsync<T>(string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = BaseUri;
                var uri = $"{path}?key={Key}&location={Location}";
                using (var stream = await client.GetStreamAsync(uri))
                {
                    return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
                }
            }
        }

    }
}
