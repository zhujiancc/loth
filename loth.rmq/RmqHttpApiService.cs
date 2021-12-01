using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

namespace loth.rmq
{
	public class RmqHttpApiService
	{
		public RmqHttpApiService(RmqUrlEnum uriEnum, int port = 15355)
		{
			ConnectionFactory innerFactoryObject = RmqConnectPoolFactory.GetClientPool(uriEnum, 5, 3).GetInnerFactoryObject();
			RmqHttpApiService.userName = innerFactoryObject.UserName;
			RmqHttpApiService.password = innerFactoryObject.Password;
			RmqHttpApiService.host = innerFactoryObject.HostName + ":" + port.ToString();
			RmqHttpApiService.vhost = innerFactoryObject.VirtualHost;
		}

		public string GetNodesDetail()
		{
			string url = string.Format("http://{0}/api/nodes", RmqHttpApiService.host);
			string data = RmqHttpApiService.GetData(url, "get", string.Empty);
			JToken jtoken = JArray.Parse(data)[0];
			return jtoken.ToString();
		}

		public int GetMessageCount(string queueName)
		{
			string url = string.Format("http://{0}/api/queues/{1}/{2}", RmqHttpApiService.host, RmqHttpApiService.vhost, queueName);
			string data = RmqHttpApiService.GetData(url, "get", string.Empty);
			JObject jobject = JObject.Parse(data);
			return jobject.GetValue("messages").Value<int>();
		}

		public string[] GetQueueMessages(string queueName, int takeCount)
		{
			string url = string.Format("http://{0}/api/queues/{1}/{2}/get", RmqHttpApiService.host, RmqHttpApiService.vhost, queueName);
			string requestDatas = JsonConvert.SerializeObject(new Dictionary<string, string>
			{
				{
					"vhost",
					RmqHttpApiService.vhost
				},
				{
					"name",
					queueName
				},
				{
					"requeue",
					"true"
				},
				{
					"encoding",
					"auto"
				},
				{
					"count",
					takeCount.ToString()
				}
			});
			string data = RmqHttpApiService.GetData(url, "post", requestDatas);
			JArray source = JArray.Parse(data);
			return (from m in source
					select m.SelectToken("payload").ToString()).ToArray<string>();
		}

		public static string GetData(string url, string httpMethod, string requestDatas)
		{
			HttpWebRequest httpWebRequest = RmqHttpApiService.createRequest(url, requestDatas, httpMethod);
			string result = string.Empty;
			using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
			{
				using (Stream responseStream = httpWebResponse.GetResponseStream())
				{
					StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
					result = streamReader.ReadToEnd();
				}
			}
			return result;
		}

		private static HttpWebRequest createRequest(string url, string requestDatas, string httpMethod)
		{
			bool flag = httpMethod.ToLower() == "post";
			HttpWebRequest httpWebRequest;
			if (flag)
			{
				httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			}
			else
			{
				httpWebRequest = (HttpWebRequest)WebRequest.Create(url + ((requestDatas == "") ? "" : "?") + requestDatas);
			}
			httpWebRequest.Method = httpMethod;
			httpWebRequest.Credentials = new NetworkCredential(RmqHttpApiService.userName, RmqHttpApiService.password);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Timeout = 3000;
			bool flag2 = httpMethod.ToLower() == "post";
			if (flag2)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(requestDatas);
				httpWebRequest.ContentLength = (long)bytes.Length;
				using (Stream requestStream = httpWebRequest.GetRequestStream())
				{
					requestStream.Write(bytes, 0, bytes.Length);
				}
			}
			return httpWebRequest;
		}

		private static string userName;

		private static string password;

		private static string host;

		private static string vhost;
	}
}
