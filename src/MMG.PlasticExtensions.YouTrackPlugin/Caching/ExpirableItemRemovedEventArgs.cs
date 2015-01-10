// *************************************************
// MMG.PlasticExtensions.YouTrackPlugin.ExpirableItemRemovedEventArgs.cs
// Last Modified: 01/10/2015 3:02 PM
// Modified By: Bustamante, Diego (bustamd1)
// *************************************************

namespace ExpirableDictionary
{
    using System;

    /// <summary>
    /// Contains the key/value pair of the item that expired and has been removed from the dictionary.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// see http://www.jondavis.net/techblog/post/2010/08/30/Four-Methods-Of-Simple-Caching-In-NET.aspx
    /// </remarks>
    public class ExpirableItemRemovedEventArgs<K, T> : EventArgs
    {
        public K Key { get; set; }
        public T Value { get; set; }
    }
}