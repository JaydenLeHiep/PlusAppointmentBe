
using System.Text;

using Newtonsoft.Json;

namespace PlusAppointment.Utils.SendingSms
{
    public class SmsTextMagicService
    {
        private readonly HttpClient _httpClient;
        private readonly string _username;
        private readonly string _apiKey;

        public SmsTextMagicService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _username = configuration["TextMagic:Username"] ?? throw new InvalidOperationException();
            _apiKey = configuration["TextMagic:ApiKey"] ?? throw new InvalidOperationException();
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            var url = "https://rest.textmagic.com/api/v2/messages";
            
            _httpClient.DefaultRequestHeaders.Add("X-TM-Username", _username);
            _httpClient.DefaultRequestHeaders.Add("X-TM-Key", _apiKey);
            
            var payload = new
            {
                text = message,
                phones = phoneNumber
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Message sent successfully: " + responseString);
                    return true;
                }
                else
                {
                    var errorString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error sending message: " + errorString);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The SMS was not sent. Error message: {ex.Message}");
                return false;
            }
        }
    }
}
