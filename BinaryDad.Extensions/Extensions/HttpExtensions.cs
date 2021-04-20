using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BinaryDad.Extensions
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Send a POST request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TBody"></typeparam>
        /// <param name="client"></param>
        /// <param name="requestUrl"></param>
        /// <param name="model"></param>
        /// <param name="useDataContractJsonSerializer">Specifies whether to serialize using <see cref="DataContractJsonSerializer"/> for WCF/REST methods</param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient client, string requestUrl, object model, bool useDataContractJsonSerializer = false)
        {
            var content = new StringContent(model.Serialize(), Encoding.UTF8, "application/json");

            return client.PostAsync(requestUrl, content);
        }

        /// <summary>
        /// Serialize the HTTP content to type <typeparamref name="TResponse"/> as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="httpContent"></param>
        /// <param name="useDataContractJsonSerializer">Specifies whether to deserialize using <see cref="DataContractJsonSerializer"/> for WCF/REST methods</param>
        /// <returns></returns>
        public static async Task<TResponse> ReadAsAsync<TResponse>(this HttpContent httpContent, bool useDataContractJsonSerializer = false)
        {
            var result = await httpContent.ReadAsStringAsync();

            return result.Deserialize<TResponse>();
        }
    }
}
