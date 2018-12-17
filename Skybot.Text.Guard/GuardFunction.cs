using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            HttpRequest request)
        {
            var key = request.Query["key"];
            if (key.Count == 0 && string.IsNullOrEmpty(key[0]) && !key[0].Equals(Settings.SecretKey))
            {
                return new UnauthorizedResult();
            }
            
            if (request.Form.TryGetValue("From", out var from) &&
                request.Form.TryGetValue("To", out var to) &&
                request.Form.TryGetValue("Body", out var body))
            {
                if (from.Count > 0 && to.Count > 0 && body.Count > 0)
                {
                    var response = await PostSmsRequest(new SmsRequest
                    {
                        From = from[0],
                        To = to[0],
                        Body = body[0]
                    });

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return new TwiMLResult();
                    }
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
