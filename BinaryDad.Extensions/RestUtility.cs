﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace BinaryDad.Extensions
{
    public class RestUtility
    {
        public const int DefaultTimeout = 5000;
        public const int DefaultSleepDelay = 3000;
        public const int DefaultRetries = 3;

        #region Get

        /// <summary>
        /// Invokes a GET request with optional headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static T Get<T>(string url, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            return Send(url, HttpMethod.Get, typeof(T), null, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed).To<T>();
        }

        /// <summary>
        /// Invokes a GET request with optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static string Get(string url, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            // response type is always a string if no returnObjectType is used
            return Send(url, HttpMethod.Get, null, null, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed) as string;
        }

        /// <summary>
        /// Invokes a GET request with optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="returnObjectType"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <param name="sleepDelayMs"></param>
        /// <param name="retries"></param>
        /// <returns></returns>
        public static object Get(string url, Type returnObjectType, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            return Send(url, HttpMethod.Get, returnObjectType, null, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed);
        }

        #endregion

        #region Post

        /// <summary>
        /// Invokes a POST request with optional headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static T Post<T>(string url, object body, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            return Send(url, HttpMethod.Post, typeof(T), body, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed).To<T>();
        }

        /// <summary>
        /// Invokes a POST request with optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static string Post(string url, object body, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            // response type is always a string if no returnObjectType is used
            return Send(url, HttpMethod.Post, null, body, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed) as string;
        }

        /// <summary>
        /// Invokes a POST request with optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="returnObjectType"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <param name="sleepDelayMs"></param>
        /// <param name="retries"></param>
        /// <returns></returns>
        public static object Post(string url, Type returnObjectType, object body, Dictionary<string, string> additionalHeaders, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retries = DefaultRetries)
        {
            return Send(url, HttpMethod.Post, returnObjectType, body, additionalHeaders, timeoutMs, sleepDelayMs, retries);
        }

        #endregion

        #region Send

        /// <summary>
        /// Invokes a request with custom method/verb and optional headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static T Send<T>(string url, HttpMethod method, object body = null, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            return Send(url, method, typeof(T), body, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed).To<T>();
        }

        /// <summary>
        /// Invokes a request with custom method/verb and optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <returns></returns>
        public static string Send(string url, HttpMethod method, object body = null, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            return Send(url, method, null, body, additionalHeaders, timeoutMs, sleepDelayMs, retriesAllowed) as string;
        }

        /// <summary>
        /// Invokes a request with custom method/verb and optional headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="returnObjectType"></param>
        /// <param name="body"></param>
        /// <param name="additionalHeaders"></param>
        /// <param name="timeoutMs">Timeout of the request in milliseconds</param>
        /// <param name="sleepDelayMs"></param>
        /// <param name="retriesAllowed"></param>
        /// <returns></returns>
        public static object Send(string url, HttpMethod method, Type returnObjectType, object body = null, Dictionary<string, string> additionalHeaders = null, int timeoutMs = DefaultTimeout, int sleepDelayMs = DefaultSleepDelay, int retriesAllowed = DefaultRetries)
        {
            var serializedResponse = string.Empty;
            var success = false;

            for (var attempts = 1; attempts <= retriesAllowed && !success; attempts++)
            {
                var request = WebRequest.CreateHttp(url);

                request.Method = method.ToString();
                request.Timeout = timeoutMs;
                request.ContentType = "application/json";
                request.Accept = "application/json, text/javascript, *; q=0.01"; // Accept is a reserved header, so you must modify it rather than add

                // add additional headers
                if (additionalHeaders != null)
                {
                    foreach (var key in additionalHeaders.Keys)
                    {
                        if (additionalHeaders[key] != null)
                        {
                            request.Headers.Add(key, additionalHeaders[key]);
                        }
                        else
                        {
                            request.Headers.Add(key);
                        }
                    }
                }

                try
                {
                    if (body != null)
                    {
                        var serializedBody = body.Serialize();
                        var bytes = System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(serializedBody);

                        request.ContentLength = bytes.Length;

                        using (var writeStream = request.GetRequestStream())
                        {
                            writeStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else if (method == HttpMethod.Post) // POST requires a content length, set to 0 for null body
                    {
                        request.ContentLength = 0;
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode < HttpStatusCode.BadRequest)
                        {
                            // Success	
                            using (var responseStream = response.GetResponseStream())
                            {
                                if (responseStream != null)
                                {
                                    using (var reader = new StreamReader(responseStream))
                                    {
                                        serializedResponse = reader.ReadToEnd();
                                    }
                                }
                            }
                        }
                    }

                    success = true;
                }
                catch
                {
                    // only throw after we have reached our retry limit
                    if (attempts >= retriesAllowed)
                    {
                        throw;
                    }
                }

                // if post failed, pause before another attempt
                if (!success)
                {
                    Thread.Sleep(sleepDelayMs);
                }
                else
                {
                    break;
                }
            }

            if (success && returnObjectType != null)
            {
                return serializedResponse.Deserialize(returnObjectType);
            }
            else
            {
                return serializedResponse;
            }
        }

        #endregion
    }
}