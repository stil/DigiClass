using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.MNIST.Helpers
{
    internal static class StringExtensions
    {
        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }
            if (partLength <= 0)
            {
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));
            }

            for (var i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }

        public static void AsDigitAscii(this Vector<double> vector)
        {
            Console.WriteLine(
                string.Join("\n",
                    string.Join(
                        "",
                        vector.Select(i => Math.Abs(i - 1) < 0.01 ? '+' : '.')
                    ).SplitInParts(28))
            );
        }
    }
}