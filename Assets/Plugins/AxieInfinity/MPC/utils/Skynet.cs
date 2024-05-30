using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SM.ID.Utils
{

    public class Paging
    {
        public string cursor { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public string pagingStyle { get; set; }
        public Paging(string cursor = "string", int limit = 200, int offset = 0, string pagingStyle = "offset")
        {
            this.cursor = cursor;
            this.limit = limit;
            this.offset = offset;
            this.pagingStyle = pagingStyle;
        }
    }
    public class Skynet : MonoBehaviour
    {

        readonly string ApiKey;
        readonly string ownerAddress;
        readonly string[] NftContractAddresses;

        readonly string[] Erc20ContractAddresses;
        public Skynet(
            string ApiKey,
            string ownerAddress,
            string[] NftContractAddress,
            string[] Erc20ContractAddresses)
        {
            this.ApiKey = ApiKey;
            this.ownerAddress = ownerAddress;
            this.NftContractAddresses = NftContractAddress;
            this.Erc20ContractAddresses = Erc20ContractAddresses;
        }


        public async Task<object> GetNFTsMetadata(string[] tokenIds)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api-gateway.skymavis.com/skynet/ronin/nfts/search");

                var body = new
                {
                    contractAddresses = NftContractAddresses,
                    ownerAddress = ownerAddress,
                    paging = new Paging(),
                    tokenIds = tokenIds
                };

                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-API-KEY", ApiKey);
                var requestBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject(content);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Get Mutiple NFTs error : " + e.Message);
            }
        }


        public async Task<object> GetTotalNFTs()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api-gateway.skymavis.com/skynet/ronin/nfts/search");

                var body = new
                {
                    contractAddresses = this.NftContractAddresses,
                    // Optional
                    ownerAddress = ownerAddress,
                    paging = new Paging()

                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-API-KEY", ApiKey);
                var requestBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject(content);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Search match NFTs error : " + e.Message);
            }
        }
        public async Task<object> GetERC20TokenBalances()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api-gateway.skymavis.com/skynet/ronin/tokens/balances/search");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-API-KEY", ApiKey);

                var body = new
                {
                    contractAddresses = this.Erc20ContractAddresses,
                    // Optional
                    includes = new string[] { "RON" },
                    ownerAddress = ownerAddress,
                    paging = new Paging(),
                    tokenStandards = new string[] { "ERC20" }
                };

                var requestBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject(content);
                return result; ;
            }
            catch (Exception e)
            {
                throw new Exception("Search match NFTs error : " + e.Message);
            }
        }

        public async Task<string> CallRPC(string contractAddress, string data)
        {
            var client = new HttpClient();
            {
                try
                {
                    var rpcRequest = new
                    {
                        jsonrpc = "2.0",
                        method = "eth_call",
                        @params = new object[]
                        {
                        new
                        {
                            to = contractAddress,
                            input = data
                        },
                        "latest"
                        },
                        id = 1
                    };

                    var rpcRequestJson = JObject.FromObject(rpcRequest).ToString();
                    var content = new StringContent(rpcRequestJson, null, "application/json");

                    var response = await client.PostAsync($"https://api-gateway.skymavis.com/rpc?apikey={ApiKey}", content);

                    // Throw if not a success code.
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseString);

                    if (jsonResponse["error"] != null)
                    {
                        throw new Exception(jsonResponse["error"]["message"].ToString());
                    }

                    return jsonResponse["result"].ToString();
                }
                catch (HttpRequestException e)
                {
                    throw new Exception("Request error: " + e.Message);
                }
                catch (Exception e)
                {
                    throw new Exception("Error: " + e.Message);
                }
            }
        }
    }
}