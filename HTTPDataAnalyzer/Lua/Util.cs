using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPDataAnalyzer
{

    public class StringUtil
    {
        public static int indexOf(byte[] data, int start, int stop, byte[] pattern)
        {
            if (data == null || pattern == null) return -1;

            int[] failure = computeFailure(pattern);

            int j = 0;

            for (int i = start; i < stop; i++)
            {
                while (j > 0 && (pattern[j] != '*' && pattern[j] != data[i]))
                {
                    j = failure[j - 1];
                }
                if (pattern[j] == '*' || pattern[j] == data[i])
                {
                    j++;
                }
                if (j == pattern.Length)
                {
                    return i - pattern.Length + 1;
                }
            }
            return -1;
        }

        /**
         * Computes the failure function using a boot-strapping process,
         * where the pattern is matched against itself.
         */
        private static int[] computeFailure(byte[] pattern)
        {
            int[] failure = new int[pattern.Length];

            int j = 0;
            for (int i = 1; i < pattern.Length; i++)
            {
                while (j > 0 && pattern[j] != pattern[i])
                {
                    j = failure[j - 1];
                }
                if (pattern[j] == pattern[i])
                {
                    j++;
                }
                failure[i] = j;
            }

            return failure;
        }

        public static  List<int> SearchBytePattern(byte[] pattern, byte[] bytes)
        {
            List<int> positions = new List<int>();
            int patternLength = pattern.Length;
            int totalLength = bytes.Length;
            byte firstMatchByte = pattern[0];
            for (int i = 0; i < totalLength; i++)
            {
                if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
                {
                    byte[] match = new byte[patternLength];
                    Array.Copy(bytes, i, match, 0, patternLength);
                    if (match.SequenceEqual<byte>(pattern))
                    {
                        positions.Add(i);
                        i += patternLength - 1;
                    }
                }
            }
            return positions;
        }

    }
}
