using System;

namespace BinaryDad.Extensions
{
    public static class UrlUtility
    {
        /// <summary>
        /// Combines a base URL with a relative URL
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        public static string Combine(string baseUrl, string relativeUrl)
        {
            // ensure domain ends with trailing slash
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl = $"{baseUrl}/";
            }

            var baseUri = new Uri(baseUrl);

            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentException("Base URL must be absolute", nameof(baseUrl));
            }

            // ensure relative path does not have a prefixed slash
            relativeUrl = relativeUrl.TrimStart('/');

            return new Uri(baseUri, relativeUrl).OriginalString;
        }
    }
}
