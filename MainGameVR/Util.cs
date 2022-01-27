using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoikatuVR
{
    internal static class Util
    {
        // Our version of C# doesn't have tuples, wtf.
        public struct ValueTuple<T1, T2>
        {
            public T1 Field1 { get; set; }
            public T2 Field2 { get; set; }

            public ValueTuple(T1 x1, T2 x2)
            {
                Field1 = x1;
                Field2 = x2;
            }
        }

        public class ValueTuple
        {
            public static ValueTuple<T1, T2> Create<T1, T2>(T1 x1, T2 x2)
            {
                return new ValueTuple<T1, T2>(x1, x2);
            }
        }

        /// <summary>
        /// Remove a prefix from the given string.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StripPrefix(string prefix, string str)
        {
            if (str.StartsWith(prefix)) return str.Substring(prefix.Length);
            return null;
        }
    }
}
