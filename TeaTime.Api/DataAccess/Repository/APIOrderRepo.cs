using System.Text;
using System.Text.Json;
using TeaTime.Api.Domains.Order;
using TeaTime.Api.Domains.Store;

namespace TeaTime.Api.DataAccess.Repository
{
    public class APIOrderRepo:IOrdersRepo
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        private static readonly JsonSerializerOptions _propertyNameCaseInsensitive = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public APIOrderRepo(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"]!;

            // 增加 header
            var tokenHeader = configuration["ApiSettings:TokenHeader"]!;
            var secretToken = configuration["ApiSettings:SecretToken"]!;
            httpClient.DefaultRequestHeaders.Add(tokenHeader, secretToken);
        }

        public async Task<IEnumerable<Order?>?> GetOrders(long storeId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/stores/{storeId}/orders");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(content, _propertyNameCaseInsensitive);

            return orders ?? Enumerable.Empty<Order>();
        }

        public async Task<IEnumerable<Order?>> GetStoreOrder(long storeId, long id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/stores/{storeId}/orders/{id}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var orders = JsonSerializer.Deserialize<IEnumerable<Order>>(content, _propertyNameCaseInsensitive);

            return orders ?? Enumerable.Empty<Order>();
        }

        public bool HaveOrders()
        {
            var response =  _httpClient.GetAsync($"{_baseUrl}/stores/1/orders").Result;// 沒有查詢所有訂單的API，先寫死
            return response.IsSuccessStatusCode;
        }

        public async Task<Order?> PostOrder(long storeId, OrderDTO orderDTO)
        {
            var json = JsonSerializer.Serialize(orderDTO);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/stores/{storeId}/orders", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<Order>(responseContent, _propertyNameCaseInsensitive);

            return order;
        }
    }
}
