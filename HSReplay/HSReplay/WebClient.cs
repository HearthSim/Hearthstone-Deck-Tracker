using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HSReplay.Web;

namespace HSReplay
{
	internal class WebClient
	{
		private string _userAgent;

		public WebClient(string userAgent)
		{
			_userAgent = userAgent;
		}

		private const string Post = "POST";
		private const string Get = "GET";
		private const string Put = "PUT";

		public async Task<HttpWebResponse> GetAsync(string url, params Header[] headers)
			=> await SendWebRequestAsync(CreateRequest(url, Get), null, false, headers);

		public async Task<HttpWebResponse> PostAsync(string url, string data, bool gzip, params Header[] headers)
			=> await SendWebRequestAsync(CreateRequest(url, Post), data, gzip, headers);

		public async Task<HttpWebResponse> PostAsync(string url, string data, bool gzip, ContentType contentType, params Header[] headers)
			=> await SendWebRequestAsync(CreateRequest(url, Post, contentType), data, gzip, headers);

		public async Task<HttpWebResponse> PostJsonAsync(string url, string data, bool gzip, params Header[] headers)
			=> await SendWebRequestAsync(CreateRequest(url, Post, ContentType.Json), data, gzip, headers);

		public async Task<HttpWebResponse> PutAsync(string url, string data, bool gzip, params Header[] headers)
			=> await SendWebRequestAsync(CreateRequest(url, Put), data, gzip, headers);

		private async Task<HttpWebResponse> SendWebRequestAsync(HttpWebRequest request, string data, bool gzip, params Header[] headers)
		{
			foreach(var header in headers)
				request.Headers.Add(header.Name, header.Value);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			if(data == null)
				return (HttpWebResponse)await request.GetResponseAsync();
			using(var stream = await request.GetRequestStreamAsync())
			{
				var encoded = Encoding.UTF8.GetBytes(data);
				if(gzip)
				{
					request.Headers.Add(HttpRequestHeader.ContentEncoding, "gzip");
					using(var zipStream = new GZipStream(stream, CompressionMode.Compress))
						zipStream.Write(encoded, 0, encoded.Length);
				}
				else
					stream.Write(encoded, 0, encoded.Length);
			}
			return (HttpWebResponse)await request.GetResponseAsync();
		}

		private HttpWebRequest CreateRequest(string url, string method, ContentType contentType = ContentType.Text)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = GetContentTypeString(contentType);
			request.Accept = "application/json";
			request.Method = method;
			request.UserAgent = _userAgent;
			return request;
		}

		private string GetContentTypeString(ContentType contentType)
		{
			switch(contentType)
			{
				case ContentType.Text:
					return "text/plain";
				case ContentType.Json:
					return "application/json";
				case ContentType.UrlEncoded:
					return "application/x-www-form-urlencoded; charset=utf-8";
				default:
					return "text/plain";
			}
		}
	}
}

