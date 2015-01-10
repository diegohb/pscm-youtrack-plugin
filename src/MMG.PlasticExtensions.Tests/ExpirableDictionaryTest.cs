// *************************************************
// MMG.PlasticExtensions.Tests.ExpirableDictionaryTest.cs
// Last Modified: 01/10/2015 3:15 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace ExpirableDictionaryTests
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class ExpirableDictionaryTest
    {
        [Test]
        public void DictionaryExpiresStaleItems()
        {
            using (var dictionary = new ExpirableItemDictionary<string, object>())
            {
                dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
                dictionary.Add("a", "b");
                Thread.Sleep(51);
                Assert.IsFalse(dictionary.ContainsKey("a"));
            }
        }

        [Test]
        public void DictionaryDoesNotExpiredNonStaleItems()
        {
            using (var dictionary = new ExpirableItemDictionary<string, object>())
            {
                dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
                dictionary.Add("a", "b");
                Assert.IsTrue(dictionary.ContainsKey("a"));
            }
        }

        [Test]
        public void DictionaryRaisesExpirationEvent()
        {
            using (var dictionary = new ExpirableItemDictionary<string, object>())
            {
                dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
                string key = "a";
                object value = "b";
                dictionary[key] = value;

                object sender = null;
                string eventKey = null;
                object eventValue = null;
                dictionary.ItemExpired += (s, e) =>
                {
                    sender = s;
                    eventKey = e.Key;
                    eventValue = e.Value;
                };
                Thread.Sleep(51);
                dictionary.ClearExpiredItems();
                Assert.AreSame(sender, dictionary);
                Assert.AreEqual(eventKey, key);
                Assert.AreEqual(eventValue, value);
            }
        }

        [Test]
        public void DictionaryAutoExpiresItems()
        {
            using (var dictionary = new ExpirableItemDictionary<string, object>())
            {
                dictionary.DefaultTimeToLive = TimeSpan.FromMilliseconds(50);
                dictionary.AutoClearExpiredItemsFrequency = TimeSpan.FromMilliseconds(150);
                string key = "a";
                object value = "b";
                dictionary[key] = value;

                object sender = null;
                string eventKey = null;
                object eventValue = null;
                dictionary.ItemExpired += (s, e) =>
                {
                    sender = s;
                    eventKey = e.Key;
                    eventValue = e.Value;
                };
                Thread.Sleep(351);
                Assert.AreSame(sender, dictionary);
                Assert.AreEqual(eventKey, key);
                Assert.AreEqual(eventValue, value);
            }
        }
    }
}