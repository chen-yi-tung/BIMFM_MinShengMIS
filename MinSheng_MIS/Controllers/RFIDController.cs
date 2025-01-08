using MinSheng_MIS.Models.ViewModels;
using MinSheng_MIS.Models;
using MinSheng_MIS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using J_RFID;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Web.Http.Results;

namespace MinSheng_MIS.Controllers
{
    public class RFIDController : Controller

    {
        private readonly Bimfm_MinSheng_MISEntities _db;
        private readonly RFIDService _rfidService;

        public RFIDController()
        {
            _db = new Bimfm_MinSheng_MISEntities();
            _rfidService = new RFIDService(_db);

            try
            {
                CentralRegistryUrl = DiscoverCentralRegistry();

                if (string.IsNullOrEmpty(CentralRegistryUrl))
                {
                    ViewBag.ErrorMessage = "Failed to discover the Central Registry URL. Please check the network configuration.";
                }
                else
                {
                    Debug.WriteLine($"Central Registry URL discovered: {CentralRegistryUrl}");
                }
                // Initialize RFID API
                rfid = new RFIDAPI();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error initializing RFID SDK: {ex.Message}";
            }
        }

        #region 檢查RFID內碼是否有使用過
        [HttpGet]
        public async Task<ActionResult> CheckIsRFIDInternalCodeDuplicate(string id)
        {
            try
            {
                await _rfidService.CheckRFIDInternalCode(id);
                return Content(JsonConvert.SerializeObject(new JsonResService<string>
                {
                    AccessState = ResState.Success,
                    ErrorMessage = null,
                    Datas = null,
                }), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }
        #endregion

        #region RFID掃描
        private RFIDAPI rfid;
        private static string connectedPort = null; // Store the connected port
        private static bool isReaderConnected = false; // Flag to track if the reader is already connected
        private static string LocalServerIp = null; // Automatically discovered IP
        private static int LocalServerPort = 5000; // Port on which the local server listens
        private static string CentralRegistryUrl = null; // Central registry URL

        #region Query Local Server IP from Central Registry
        private (string Ip, int Port) QueryLocalServerIp(string serverName)
        {
            string lookupUrl = $"{CentralRegistryUrl}/lookup/{serverName}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = httpClient.GetAsync(lookupUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        var serverInfo = JsonConvert.DeserializeObject<dynamic>(responseBody);

                        string ip = serverInfo.ip.ToString();
                        int port = int.Parse(serverInfo.port.ToString());

                        Debug.WriteLine($"Discovered Local Server - IP: {ip}, Port: {port}");
                        return (ip, port);
                    }
                    else
                    {
                        Debug.WriteLine($"Failed to query local server IP. Status: {response.StatusCode}");
                        return (null, 0);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error querying local server IP: {ex.Message}");
                    return (null, 0);
                }
            }
        }

        #endregion

        #region Equipment Management Actions
        public ActionResult Create()
        {
            // Query LocalServerIp and Port from the central registry
            if (string.IsNullOrEmpty(LocalServerIp))
            {
                var serverInfo = QueryLocalServerIp("LocalServer1");

                if (string.IsNullOrEmpty(serverInfo.Ip) || serverInfo.Port == 0)
                {
                    ViewBag.Message = "Failed to find the local server IP from the central registry. Please try again.";
                }
                else
                {
                    LocalServerIp = serverInfo.Ip;
                    LocalServerPort = serverInfo.Port;
                    Debug.WriteLine($"Local Server Discovered: {LocalServerIp}:{LocalServerPort}");
                }
            }

            ViewBag.LocalServerIp = LocalServerIp ?? "Not Found";
            return View();
        }

        [HttpPost]
        public ActionResult Create(string equipmentName)
        {
            // Logic to handle equipment creation
            ViewBag.Message = "Equipment created successfully!";
            ViewBag.LocalServerIp = LocalServerIp;
            return View();
        }
        #endregion

        [HttpGet]
        public ActionResult CheckRFID()
        {
            var res = new JsonResService<string>();
            try
            {
                res = _rfidService.CheckRFID();
                return Content(JsonConvert.SerializeObject(res), "application/json");
            }
            catch (MyCusResException ex)
            {
                return Helper.HandleMyCusResException(this, ex);
            }
            catch (Exception)
            {
                return Helper.HandleException(this);
            }
        }

        
        private string DiscoverCentralRegistry()
        {
            // Attempt to discover the Central Registry dynamically
            try
            {
                // Step 1: Attempt discovery via UDP broadcast
                using (var udpClient = new UdpClient())
                {
                    udpClient.EnableBroadcast = true;
                    udpClient.Client.ReceiveTimeout = 5000; // Set a 5-second timeout

                    var broadcastMessage = Encoding.UTF8.GetBytes("DISCOVER_REGISTRY");
                    udpClient.Send(broadcastMessage, broadcastMessage.Length, new IPEndPoint(IPAddress.Broadcast, 9090)); // Adjust port to match the UDP listener

                    var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var response = udpClient.Receive(ref serverEndpoint);
                    string registryUrl = Encoding.UTF8.GetString(response);

                    Debug.WriteLine($"Discovered Central Registry via UDP: {registryUrl}");
                    return registryUrl; // Return the discovered URL
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UDP discovery failed: {ex.Message}");
            }

            // Step 2: Attempt to resolve via DNS
            try
            {
                string dnsHostname = "central-registry.local"; // Replace with your DNS hostname
                string dnsIp = Dns.GetHostEntry(dnsHostname).AddressList.FirstOrDefault()?.ToString();

                if (!string.IsNullOrEmpty(dnsIp))
                {
                    string dnsUrl = $"http://{dnsIp}:8080";
                    Debug.WriteLine($"Discovered Central Registry via DNS: {dnsUrl}");
                    return dnsUrl;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DNS discovery failed: {ex.Message}");
            }

            // Step 3: Fallback to environment variable
            string fallbackUrl = Environment.GetEnvironmentVariable("CENTRAL_REGISTRY_URL");
            if (!string.IsNullOrEmpty(fallbackUrl))
            {
                Debug.WriteLine($"Using fallback Central Registry URL from environment variable: {fallbackUrl}");
                return fallbackUrl;
            }

            // Step 4: Final hardcoded fallback
            string defaultUrl = "http://192.168.0.148:8080";
            Debug.WriteLine($"Using hardcoded fallback Central Registry URL: {defaultUrl}");
            return defaultUrl;
        }
        #endregion
    }
}