using LoggerWorkerService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoggerWorkerService
{
    public class ApiContext
    {
        public static string BaseUrl = "http://192.168.10.250:8888/api/";
        private readonly HttpClient _client;

        public ApiContext()
        {
            _client = new HttpClient();
        }

        public async Task<List<TblIlogDbIp>> GetIpsAsync()
        {
            try
            {
                var httpResponse = await _client.GetAsync(BaseUrl + "ip");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed!");
                }

                var content = await httpResponse.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TblIlogDbIp>>(content);

                return result;
            }
            catch (Exception)
            {
                throw new Exception("Failed!");
            }
        }

        public async Task<TblIlogDbLog> PostLogAsync(TblIlogDbLog log)
        {
            try
            {
                var content = JsonConvert.SerializeObject(log);
                var httpResponse = await _client.PostAsync(BaseUrl + "log", new StringContent(content, Encoding.Default, "application/json"));
                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed!");
                }
                var result = JsonConvert.DeserializeObject<TblIlogDbLog>(await httpResponse.Content.ReadAsStringAsync());
                return result;
            }
            catch (Exception)
            {

                throw new Exception("Failed!");
            }
        }
    }
}
