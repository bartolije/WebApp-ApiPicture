using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IoT_PicturesAPI.Models;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoT_PicturesAPI.Controllers
{
    public class PicturesController : Controller
    {
        private const string apiKey = "6094592a37704b4fa74f02f38ff9b1c3";
        // https://api.cognitive.microsoft.com/bing/v5.0/images/search[?q][&count][&offset][&mkt][&safeSearch]
        private const string apiEndpointUrl = "https://api.cognitive.microsoft.com/bing/v5.0/images/search";
        private string DeviceConnectionString = "HostName=IoTHub-ApiPicture.azure-devices.net;DeviceId=device0821bd2cfcab45ab97548a4b4ada68dc;SharedAccessKey=frGqLIfGUnKqYi5eBku4NL7MaJYKQkbAuIvS51R5Jxc=";

        // GET: Pictures
        public ActionResult Index()
        {
            return View();
        }

        public static T ParseJsonObject<T>(string json) where T : class, new()
        {
            JObject jobject = JObject.Parse(json);
            return JsonConvert.DeserializeObject<T>(jobject.ToString());
        }

        [HttpGet]
        public async Task<string> GetPictures(string userInput)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(userInput);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);


            // https://api.cognitive.microsoft.com/bing/v5.0/images/search[?q][&count][&offset][&mkt][&safeSearch]


            // https://api.cognitive.microsoft.com/bing/v5.0/images/search?q=sloth

            var uri = apiEndpointUrl + "?q=" + queryString + "&count=10";

            HttpResponseMessage response;
            
            response = await client.GetAsync(uri);
            var jsonBody = await response.Content.ReadAsStringAsync();

            APIModels tmp = JsonConvert.DeserializeObject<APIModels>(jsonBody);

            var coreReturn = JsonConvert.SerializeObject(tmp.value);

            // device part
            //DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
            //await SendEvent(deviceClient, coreReturn);

            return coreReturn;
        }
        

        static async Task SendEvent(DeviceClient deviceClient, string message)
        {
            try
            {
                string dataBuffer = String.Empty;

                dataBuffer = JsonConvert.SerializeObject(message);
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));

                await deviceClient.SendEventAsync(eventMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("==================================================== \n");
                Console.WriteLine(e.ToString());
                throw;
            }
           
        }

    }
}