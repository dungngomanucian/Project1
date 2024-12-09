using Microsoft.Extensions.Options;
using Project1.API;
using System.Text.Json;

namespace Project1.Services
{
    public class ExchangeRateService
    {
        private readonly ExchangeRateApiOptions _options;

        public ExchangeRateService(IOptions<ExchangeRateApiOptions> options)
        {
            _options = options.Value;
        }

        public async Task<double> GetExchangeRateAsync()
        {
            var apiUrl = $"https://api.exchangeratesapi.io/v1/latest?access_key={_options.ApiKey}&base={_options.BaseCurrency}&symbols={_options.TargetCurrency}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Không thể lấy tỷ giá từ API: {response.ReasonPhrase}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var exchangeRateData = JsonSerializer.Deserialize<ExchangeRateResponse>(jsonResponse);

                if (exchangeRateData?.Rates != null && exchangeRateData.Rates.ContainsKey(_options.TargetCurrency))
                {
                    return exchangeRateData.Rates[_options.TargetCurrency];
                }

                throw new Exception("Tỷ giá không tồn tại trong phản hồi từ API.");
            }
        }
    }
}
