using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DigiClass.Methods.Extensions
{
    /// <summary>
    ///     Zawiera rozszerzenia do interfejsu IEnumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        private static readonly Random Rng = new Random();

        public static int MaxIndex<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            var maxIndex = -1;
            var maxValue = default(T); // Immediately overwritten anyway

            var index = 0;
            foreach (var value in sequence)
            {
                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        /// <summary>
        ///     Zwraca sekwencję z wymieszanymi elementami listy wejściowej.
        /// </summary>
        /// <param name="source">Lista wejściowa.</param>
        /// <returns>Wymieszana sekwencja wyjściowa.</returns>
        public static IEnumerable<T> Shuffle<T>(this IList<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.ShuffleIterator();
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IList<T> source)
        {
            var buffer = source;
            for (var i = 0; i < buffer.Count; i++)
            {
                var j = Rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        /// <summary>
        ///     Dzieli sekwencję wejściową na fragmenty o zadanym rozmiarze.
        /// </summary>
        /// <param name="source">Sekwencja wejściowa.</param>
        /// <param name="chunkSize">Rozmiar fragmentu.</param>
        /// <returns>Sekwencja wyjściowa.</returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(
            this IEnumerable<T> source,
            int chunkSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize),
                    "The parameter must be a positive value.");
            }

            return source.ChunkInternal(chunkSize);
        }

        private static IEnumerable<IEnumerable<T>> ChunkInternal<T>(
            this IEnumerable<T> source, int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(source != null);
            Debug.Assert(chunkSize > 0);

            // Get the enumerator.  Dispose of when done.
            using (var enumerator = source.GetEnumerator())
            {
                do
                {
                    // Move to the next element.  If there's nothing left
                    // then get out.
                    if (!enumerator.MoveNext())
                    {
                        yield break;
                    }

                    // Return the chunked sequence.
                    yield return ChunkSequence(enumerator, chunkSize);
                } while (true);
            }
        }

        private static IEnumerable<T> ChunkSequence<T>(IEnumerator<T> enumerator,
            int chunkSize)
        {
            // Validate parameters.
            Debug.Assert(enumerator != null);
            Debug.Assert(chunkSize > 0);

            // The count.
            var count = 0;

            // There is at least one item.  Yield and then continue.
            do
            {
                // Yield the item.
                yield return enumerator.Current;
            } while (++count < chunkSize && enumerator.MoveNext());
        }


        public static void ZipForEach<TLeft, TRight>(this IEnumerable<TLeft> left, IEnumerable<TRight> right,
            Action<TLeft, TRight> action)
        {
            using (var leftEnum = left.GetEnumerator())
            {
                using (var rightEnum = right.GetEnumerator())
                {
                    while (leftEnum.MoveNext() && rightEnum.MoveNext())
                    {
                        action.Invoke(leftEnum.Current, rightEnum.Current);
                    }
                }
            }
        }
    }
}