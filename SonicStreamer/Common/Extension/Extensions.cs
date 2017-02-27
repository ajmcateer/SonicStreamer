using System;
using System.Collections.Generic;

namespace SonicStreamer.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Mischt die Einträge einer Liste
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            var rng =
                new Random((DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second) * DateTime.Now.Millisecond);
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}