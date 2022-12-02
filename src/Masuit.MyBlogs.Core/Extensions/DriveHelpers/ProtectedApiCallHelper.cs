using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Masuit.MyBlogs.Core.Extensions.DriveHelpers;

/// <summary>
/// Helper class to call a protected API and process its result
/// </summary>
public sealed class ProtectedApiCallHelper
{
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="httpClient">HttpClient used to call the protected API</param>
	public ProtectedApiCallHelper(HttpClient httpClient)
	{
		HttpClient = httpClient;
	}

	private HttpClient HttpClient { get; }

	/// <summary>
	/// Calls the protected Web API and processes the result
	/// </summary>
	/// <param name="webApiUrl">Url of the Web API to call (supposed to return Json)</param>
	/// <param name="accessToken">Access token used as a bearer security token to call the Web API</param>
	/// <param name="processResult">Callback used to process the result of the call to the Web API</param>
	public async Task CallWebApiAndProcessResultASync(string webApiUrl, string accessToken, Action<JObject> processResult, Method method = Method.Get, HttpContent sendContent = null)
	{
		if (!string.IsNullOrEmpty(accessToken))
		{
			var defaultRequetHeaders = HttpClient.DefaultRequestHeaders;
			if (defaultRequetHeaders.Accept.All(m => m.MediaType != "application/json"))
			{
				HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
			defaultRequetHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
			var response = method switch
			{
				Method.Get => await HttpClient.GetAsync(webApiUrl),
				Method.Post => await HttpClient.PostAsync(webApiUrl, sendContent),
				Method.Put => await HttpClient.PutAsync(webApiUrl, sendContent),
				_ => new HttpResponseMessage()
			};
			if (response.IsSuccessStatusCode)
			{
				string json = await response.Content.ReadAsStringAsync();
				JObject result = JsonConvert.DeserializeObject(json) as JObject;
				processResult(result);
			}
			else
			{
				string content = await response.Content.ReadAsStringAsync();
				JObject result = JsonConvert.DeserializeObject(content) as JObject;
				throw new Exception(result.Property("error").Value["message"].ToString());
			}
		}
		else
		{
			throw new Exception("未提供Token");
		}
	}

	public enum Method
	{
		Post,
		Get,
		Put
	}
}