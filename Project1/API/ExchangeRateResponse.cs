using System.Text.Json.Serialization;

namespace Project1.API
{
    public class ExchangeRateResponse
    {
        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, double> Rates { get; set; }
    }
}
