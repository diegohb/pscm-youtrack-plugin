// *************************************************
// MMG.PlasticExtensions.Tests.MemoryCacheTest.cs
// Last Modified: 01/10/2015 3:15 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace ExpirableDictionaryTests
{
    using System;
    using System.Collections.Specialized;
    using NUnit.Framework;

    /// <summary>
    /// just observing the new .NET 4.0 memory cache
    /// </summary>
    [TestFixture]
    public class MemoryCacheTest
    {
        /*[Test]
        public void TestMethod1()
        {
            var config = new NameValueCollection();
            var cache = new MemoryCache("myMemCache", config);
            cache.Add
                (new CacheItem("a", "b"),
                    new CacheItemPolicy
                    {
                        Priority = CacheItemPriority.NotRemovable,
                        SlidingExpiration = TimeSpan.FromMilliseconds(50)
                    });
            Assert.IsTrue(cache.Contains("a"));
            Assert.AreEqual("b", cache["a"]);
        }*/
    }
}