using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IoT_PicturesAPI.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoT_PicturesAPI.Controllers
{
    public class PicturesController : Controller
    {
        private const string apiKey = "#####";
        // https://api.cognitive.microsoft.com/bing/v5.0/images/search[?q][&count][&offset][&mkt][&safeSearch]
        private const string apiEndpointUrl = "https://api.cognitive.microsoft.com/bing/v5.0/images/search";
        private string DeviceConnectionString = "#####";
        

        static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("#####");

        

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
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
            await SendEvent(deviceClient, coreReturn);

            return coreReturn;
        }

        [HttpGet]
        public async Task<string> GetDevices()
        {
            var coreReturn = String.Empty;

            try
            {
                var devices = await _registryManager.GetDevicesAsync(20);
                coreReturn = JsonConvert.SerializeObject(devices);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=======================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("=======================");
            }

            return coreReturn;
        }

        [HttpGet]
        public async Task<string> AddDevice()
        {
            var deviceId = await AddDeviceAsync();
            return JsonConvert.SerializeObject(deviceId);
        }

        static async Task SendEvent(DeviceClient deviceClient, string message)
        {
            try
            {
                var strMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(message));
                var clientDevice = DeviceClient.Create("Picture-iothub.azure-devices.net", new DeviceAuthenticationWithRegistrySymmetricKey("device3ce1783a3d124dd0a62f93c4665a8b72", "AiYKPWELnG28CIwg3OTuaZrS/gJHZ3RLBYJTs4vSDbo="));
                await clientDevice.SendEventAsync(strMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine("==================================================== \n");
                Console.WriteLine(e.ToString());
                throw;
            }
           
        }

        private static async Task<string> AddDeviceAsync()
        {
            var deviceId = Guid.NewGuid().ToString("N");
            var deviceList = new List<Device>();

            var device = new Device();

            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
            }

            deviceList.Add(device);
            return JsonConvert.SerializeObject(deviceList);
        }

    }
}