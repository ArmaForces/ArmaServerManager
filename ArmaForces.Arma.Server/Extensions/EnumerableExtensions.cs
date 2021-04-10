﻿using System.Collections.Generic;
using System.Linq;

namespace ArmaForces.Arma.Server.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
        {
            return enumerable
                .Where(x => x != null)
                .Cast<T>();
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : struct
        {
            return enumerable
                .Where(x => x != null)
                .Cast<T>();
        }
    }
}