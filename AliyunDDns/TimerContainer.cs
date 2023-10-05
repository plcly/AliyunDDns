using AlibabaCloud.OpenApiClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;

namespace AliyunDDns
{
    public class TimerContainer
    {
        private System.Timers.Timer _timer;
        private bool _isInitialized;
        private AlibabaCloud.OpenApiClient.Client _client;
        private string _subDomain;
        private string _hostName;
        private string _doMain;
        private List<string> _getIpApiAddresses;
        public TimerContainer()
        {
            InitApiClient();
            _timer = new System.Timers.Timer();
            _timer.Interval = 5 * 60 * 1000;
            _timer.Elapsed += SetDDns;
        }

        public AlibabaCloud.OpenApiClient.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = accessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Alidns
            config.Endpoint = "alidns.cn-hangzhou.aliyuncs.com";
            return new AlibabaCloud.OpenApiClient.Client(config);
        }

        private void InitApiClient()
        {
            var id = ConfigurationManager.AppSettings["AccessKeyID"] ?? string.Empty;
            var secret = ConfigurationManager.AppSettings["AccessKeySecret"] ?? string.Empty;
            _hostName = ConfigurationManager.AppSettings["HostName"] ?? string.Empty;
            _doMain = ConfigurationManager.AppSettings["Domain"] ?? string.Empty;
            if (string.IsNullOrEmpty(id) 
                || string.IsNullOrEmpty(secret) 
                || string.IsNullOrEmpty(_hostName)
                || string.IsNullOrEmpty(_doMain))
            {
                Console.WriteLine($"{DateTime.Now}:Please set app.config.");
                return;
            }
            _subDomain = $"{_hostName}.{_doMain}";
            Console.WriteLine($"{DateTime.Now}: Current subDomain is :{_subDomain}");
            var ips = ConfigurationManager.AppSettings["IPApis"] ?? string.Empty;
            if (!string.IsNullOrEmpty(ips))
            {
                _getIpApiAddresses = ips.Split(',').ToList();
            }
            _client = CreateClient(id, secret);
            _isInitialized = true;
        }

        public void Start()
        {
            try
            {
                if (_isInitialized)
                {
                    SetDDns(null, null);
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}:{ex.Message}");
                _timer.Stop();
            }

        }

        public void SetDDns(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"{DateTime.Now}: Begin Set DDns");
            var ip = GetIP();
            Console.WriteLine($"{DateTime.Now}: Current ip is : {ip}");
            var subDomainResult = DescribeSubDomainRecords();
            if (subDomainResult.TryGetValue("statusCode", out object statusCode)
                && statusCode is int status && status == 200)
            {
                var body = JsonConvert.DeserializeObject<DescribeSubDomainRecordsBody>
                    (JsonConvert.SerializeObject(subDomainResult["body"]));

                if (body.TotalCount > 0)
                {
                    var record = body.DomainRecords.Record[0];
                    Console.WriteLine($"{DateTime.Now}: record's ip is {record.Value}");
                    if (ip != record.Value)
                    {
                        Console.WriteLine($"{DateTime.Now}: ip is not equal, begin update");
                        UpdateSubDoamin(ip, record.RecordId);
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}: No exist record, add record");
                    AddSubDomain(ip);
                }
            }
            Console.WriteLine($"{DateTime.Now}: End Set DDns");
        }

        private void AddSubDomain(string ip)
        {
            AlibabaCloud.OpenApiClient.Models.Params params_ = CreateApiParams("AddDomainRecord");
            // query params
            Dictionary<string, object> queries = new Dictionary<string, object>() { };
            queries["DomainName"] = _doMain;
            queries["RR"] = _hostName;
            queries["Type"] = "A";
            queries["Value"] = ip;
            // runtime options
            AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new AlibabaCloud.TeaUtil.Models.RuntimeOptions();
            AlibabaCloud.OpenApiClient.Models.OpenApiRequest request = new AlibabaCloud.OpenApiClient.Models.OpenApiRequest
            {
                Query = AlibabaCloud.OpenApiUtil.Client.Query(queries),
            };
            // 复制代码运行请自行打印 API 的返回值
            // 返回值为 Map 类型，可从 Map 中获得三类数据：响应体 body、响应头 headers、HTTP 返回的状态码 statusCode。
            _client.CallApi(params_, request, runtime);
        }

        private void UpdateSubDoamin(string ip, string recordId)
        {
            AlibabaCloud.OpenApiClient.Models.Params params_ = CreateApiParams("UpdateDomainRecord");
            // query params
            Dictionary<string, object> queries = new Dictionary<string, object>() { };
            queries["RecordId"] = recordId;
            queries["RR"] = _hostName;
            queries["Type"] = "A";
            queries["Value"] = ip;
            // runtime options
            AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new AlibabaCloud.TeaUtil.Models.RuntimeOptions();
            AlibabaCloud.OpenApiClient.Models.OpenApiRequest request = new AlibabaCloud.OpenApiClient.Models.OpenApiRequest
            {
                Query = AlibabaCloud.OpenApiUtil.Client.Query(queries),
            };
            // 复制代码运行请自行打印 API 的返回值
            // 返回值为 Map 类型，可从 Map 中获得三类数据：响应体 body、响应头 headers、HTTP 返回的状态码 statusCode。
            _client.CallApi(params_, request, runtime);
        }

        private Dictionary<string, object> DescribeSubDomainRecords()
        {
            var subParams = CreateApiParams("DescribeSubDomainRecords");
            Dictionary<string, object> queries = new Dictionary<string, object>() { };
            queries["SubDomain"] = _subDomain;
            // runtime options
            AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new AlibabaCloud.TeaUtil.Models.RuntimeOptions();
            AlibabaCloud.OpenApiClient.Models.OpenApiRequest request = new AlibabaCloud.OpenApiClient.Models.OpenApiRequest
            {
                Query = AlibabaCloud.OpenApiUtil.Client.Query(queries),
            };
            // 复制代码运行请自行打印 API 的返回值
            // 返回值为 Map 类型，可从 Map 中获得三类数据：响应体 body、响应头 headers、HTTP 返回的状态码 statusCode。
            return _client.CallApi(subParams, request, runtime);
        }

        public AlibabaCloud.OpenApiClient.Models.Params CreateApiParams(string action)
        {
            AlibabaCloud.OpenApiClient.Models.Params params_ = new AlibabaCloud.OpenApiClient.Models.Params
            {
                // 接口名称
                Action = action,
                // 接口版本
                Version = "2015-01-09",
                // 接口协议
                Protocol = "HTTPS",
                // 接口 HTTP 方法
                Method = "POST",
                AuthType = "AK",
                Style = "RPC",
                // 接口 PATH
                Pathname = "/",
                // 接口请求体内容格式
                ReqBodyType = "json",
                // 接口响应体内容格式
                BodyType = "json",
            };
            return params_;
        }

        private string GetIP()
        {
            if (_getIpApiAddresses == null || _getIpApiAddresses.Count == 0)
            {
                return GetDefaultIP();
            }
            foreach (var add in _getIpApiAddresses)
            {
                try
                {
                    var ip = GetIP(add);
                    if (!string.IsNullOrEmpty(ip))
                    {
                        return ip;
                    }
                }
                catch (Exception)
                {

                }
            }
            
            return GetDefaultIP();
        }

        private string GetIP(string add)
        {
            using var hc = new HttpClient();
            var re = hc.GetStringAsync(add).Result;
            return re.Replace("\n","");
        }

        private string GetDefaultIP()
        {
            using var hc = new HttpClient();
            hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var result = hc.GetStringAsync("https://www.ip.cn/api/index?type=0").Result;
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            return (string)data["ip"];
        }
    }
}
