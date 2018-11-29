using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Skybot.Text.Guard
{
    public static class GuardFunction
    {
        [FunctionName("SkybotTextGuard")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest request)
        {
            string secretKey = request.Query["key"];
            if (string.IsNullOrEmpty(secretKey) || !secretKey.Equals(Settings.SecretKey))
            {
                return new UnauthorizedResult();
            }

            var requestBody = new StreamReader(request.Body).ReadToEnd();
            var smsRequest = JsonConvert.DeserializeObject<SmsRequest>(requestBody);

            if (smsRequest != null)
            {
                await PostSmsRequest(smsRequest);
            }

            return new BadRequestResult();
        }

        private static async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"client_id", Settings.ClientId},
                    {"client_secret", Settings.ClientSecret },
                    {"grant_type", "client_credentials" }
                });

                var response = await client.PostAsync(Settings.AuthorityUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var deserialziedContent = JsonConvert.DeserializeObject<dynamic>(responseContent);

                return deserialziedContent.access_token;
            }
        }

        private static async Task PostSmsRequest(SmsRequest smsRequest)
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    FromNumber = smsRequest.From,
                    ToNumber = smsRequest.To,
                    Message = smsRequest.Body
                }), System.Text.Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                await client.PostAsync(Settings.TextoServiceUri, content);
            }
        }
    }
}
