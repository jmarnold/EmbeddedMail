using System;
using System.Collections.Generic;

namespace EmbeddedMail
{
    internal static class CoreExtensions
    {
         public static void Each<T>(this IEnumerable<T> source, Action<T> callback)
         {
             foreach(var value in source)
             {
                 callback(value);
             }
         }
    }
}