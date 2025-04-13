using System.Text.Json.Nodes;

namespace BerlioWeb.Models
{
    public class RecaptchaService
    {
        public static async Task<bool>verifyReCaptchaV3(string response, string secret, string verificationUrl)
        {
            using(var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(response),"response");
                content.Add(new StringContent(secret), "secret");
                var result = await client.PostAsync(verificationUrl,content);
                //Console.WriteLine(result);
                if (result.IsSuccessStatusCode)
                {
                    var strresponse = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(strresponse);
                    var jsonresponse = JsonNode.Parse(strresponse);
                    if (jsonresponse != null)
                    {
                        var success = ((bool?)jsonresponse["success"]);
                        var score = (double?)jsonresponse["score"];
                        Console.WriteLine(score);
                        if (score >= 0.5 && success==true)
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}
