using Microsoft.Extensions.Options;
using Resturant_Backend.DTO.payment;
using System.Text.Json;

namespace Resturant_Backend.Services
{
    public class PaymobSettings
    {
        public string ApiKey { get; set; }
        public string IntegrationId { get; set; }
        public string IframeId { get; set; }
        public string HmacSecret { get; set; }
    }

    public class PaymobService
    {
        private readonly HttpClient _http;
        private readonly PaymobSettings _settings;

        public PaymobService(HttpClient http, IOptions<PaymobSettings> settings)
        {
            _http = http;
            _settings = settings.Value;
        }

        public async Task<string> GetAuthTokenAsync()
        {
            var response = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = _settings.ApiKey });

            var responseBody = await response.Content.ReadAsStringAsync();
            if(!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Paymob auth failed ({(int)response.StatusCode}): {responseBody}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return json.GetProperty("token").GetString();
        }

        public async Task<int> RegisterOrderAsync(string authToken, int localOrderId, decimal amountCents)
        {
            var payload = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)amountCents,
                currency = "EGP",
                merchant_order_id = localOrderId.ToString()
            };
            var response = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/ecommerce/orders", payload);

            var responseBody = await response.Content.ReadAsStringAsync();
            if(!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Paymob order registration failed ({(int)response.StatusCode}): {responseBody}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return json.GetProperty("id").GetInt32();
        }

        public async Task<string> GetPaymentKeyAsync(string authToken, int paymobOrderId,
            decimal amountCents, BillingData billing)
        {
            var payload = new
            {
                auth_token = authToken,
                amount_cents = (int)amountCents,
                expiration = 3600,
                order_id = paymobOrderId,
                billing_data = billing,
                currency = "EGP",
                integration_id = _settings.IntegrationId
            };
            var response = await _http.PostAsJsonAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys", payload);

            var responseBody = await response.Content.ReadAsStringAsync();
            if(!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Paymob payment key request failed ({(int)response.StatusCode}): {responseBody}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(responseBody);
            return json.GetProperty("token").GetString();
        }
    }
}