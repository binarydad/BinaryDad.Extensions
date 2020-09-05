using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace BinaryDad.Extensions
{
    /// <summary>
    /// In-memory cache helper wrapping the <see cref="MemoryCache.Default"/>
    /// </summary>
    public static class CacheHelper
    {
        private const int DefaultCacheDuration = 10; // minutes

        #region Add

        /// <summary>
        /// Adds an item to the cache for a duration (in minutes)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheDuration">The length of the cache duration in minutes</param>
        /// <param name="isSliding">Indicates whether the cache duration is sliding or absolute</param>
        public static void Add(string key, object value, int cacheDuration = DefaultCacheDuration, bool isSliding = false) => Add(key, value, GetCacheItemPolicy(cacheDuration, isSliding));

        /// <summary>
        /// Adds an item to the cache with an absolute expiration
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpiration"></param>
        public static void Add(string key, object value, DateTime absoluteExpiration) => Add(key, value, GetCacheItemPolicy(absoluteExpiration));

        #endregion

        #region Get

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="cacheDuration">Amount of time in minutes to persist the cache if loaded from source</param>
        /// <param name="isSliding">Indicates whether the cache duration is sliding or absolute</param>
        /// <returns></returns>
        public static T Get<T>(string key, Func<T> source, int cacheDuration = DefaultCacheDuration, bool isSliding = false) => Get(key, source, GetCacheItemPolicy(cacheDuration, isSliding));

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="cacheDuration">Amount of time in minutes to persist the cache if loaded from source</param>
        /// <param name="isSliding">Indicates whether the cache duration is sliding or absolute</param>
        /// <returns></returns>
        public static Task<T> GetAsync<T>(string key, Func<Task<T>> source, int cacheDuration = DefaultCacheDuration, bool isSliding = false) => GetAsync(key, source, GetCacheItemPolicy(cacheDuration, isSliding));

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="absoluteExpiration">The date and time when the cache will invalidate</param>
        /// <returns></returns>
        public static T Get<T>(string key, Func<T> source, DateTime absoluteExpiration) => Get(key, source, GetCacheItemPolicy(absoluteExpiration));

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="absoluteExpiration">The date and time when the cache will invalidate</param>
        /// <returns></returns>
        public static Task<T> GetAsync<T>(string key, Func<Task<T>> source, DateTime absoluteExpiration) => GetAsync(key, source, GetCacheItemPolicy(absoluteExpiration));

        /// <summary>
        /// Retrieves an object from the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) => MemoryCache.Default[key].To<T>();

        #endregion

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key) => MemoryCache.Default.Remove(key);

        /// <summary>
        /// Returns true if the value exists in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exists(string key) => MemoryCache.Default[key] != null;

        #region Private Methods

        /// <summary>
        /// Adds an item to the cache with a custom <see cref="CacheItemPolicy"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cacheItemPolicy"></param>
        private static void Add(string key, object value, CacheItemPolicy cacheItemPolicy)
        {
            Remove(key);

            MemoryCache.Default.Add(key, value, cacheItemPolicy);
        }

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        private static T Get<T>(string key, Func<T> source, CacheItemPolicy cacheItemPolicy)
        {
            if (Exists(key))
            {
                return Get<T>(key);
            }

            // if the value does not exist in the cache, automatically add it and return it
            var value = source.Invoke();

            Add(key, value, cacheItemPolicy);

            return value;
        }

        /// <summary>
        /// Retrieves an object from the cache, and if not found, retrieves and sets from a source delegate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        private static async Task<T> GetAsync<T>(string key, Func<Task<T>> source, CacheItemPolicy cacheItemPolicy)
        {
            if (Exists(key))
            {
                return Get<T>(key);
            }

            // if the value does not exist in the cache, automatically add it and return it
            var value = await source.Invoke();

            Add(key, value, cacheItemPolicy);

            return value;
        }

        /// <summary>
        /// Creates a <see cref="CacheItemPolicy"/> given an absolute expiration date/time
        /// </summary>
        /// <param name="absoluteExpiration"></param>
        /// <returns></returns>
        private static CacheItemPolicy GetCacheItemPolicy(DateTime absoluteExpiration)
        {
            return new CacheItemPolicy
            {
                AbsoluteExpiration = absoluteExpiration
            };
        }

        /// <summary>
        /// Creates a <see cref="CacheItemPolicy"/> given a cache duration (in minutes) and if the cache expiration is sliding
        /// </summary>
        /// <param name="cacheDuration"></param>
        /// <param name="isSliding"></param>
        /// <returns></returns>
        private static CacheItemPolicy GetCacheItemPolicy(int cacheDuration, bool isSliding)
        {
            if (isSliding)
            {
                return new CacheItemPolicy
                {
                    SlidingExpiration = new TimeSpan(0, cacheDuration, 0)
                };
            }

            return GetCacheItemPolicy(DateTime.Now.AddMinutes(cacheDuration));
        }

        #endregion
    }
}
