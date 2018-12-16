using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Twilio.AspNet.Core;

namespace Skybot.Text.Guard
{
    public static class GuardFunction
    {
        [FunctionName("SkybotTextGuard")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            Twilio.AspNet.Common.SmsRequest request, string key)
        {
            if (string.IsNullOrEmpty(key) || !key.Equals(Settings.SecretKey))
            {
                return new UnauthorizedResult();
            }

            var requestBody = new StreamReader(request.Body).ReadToEnd();
            var smsRequest = JsonConvert.DeserializeObject<SmsRequest>(requestBody);

            if (smsRequest != null)
            {
                var response = await PostSmsRequest(smsRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new TwiMLResult();
                }
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

                var response = await client.PostAsync($"{Settings.AuthorityUri}/connect/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var deserialziedContent = JsonConvert.DeserializeObject<dynamic>(responseContent);

                return deserialziedContent?.access_token;
            }
        }

        private static async Task<HttpResponseMessage> PostSmsRequest(SmsRequest smsRequest)
        {
            var token = await GetToken();
            using (var client = new HttpClient())
            {
                var content = new
                {
                    FromNumber = smsRequest.From,
                    ToNumber = smsRequest.To,
                    Message = smsRequest.Body
                };

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var response = await client.PostAsJsonAsync($"{Settings.TextoServiceUri}/api/text/receive", content);

                return response;
            }
        }
    }
}
