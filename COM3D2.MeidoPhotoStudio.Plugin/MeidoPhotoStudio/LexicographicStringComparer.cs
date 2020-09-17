/* 
    Taken from https://gist.github.com/mstum/63a6e3e8cf54e8ae55b6aa28ca6f20c5

    Modified slightly to remove the need for unsafe and changed namespace to plugin namespace
*/
using System;
using System.Collections.Generic;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    /// <summary>
    /// A string comparer that behaves like StrCmpLogicalW
    /// https://msdn.microsoft.com/en-us/library/windows/desktop/bb759947
    /// 
    /// This means:
    /// * case insensitive (ZA == za)
    /// * numbers are treated as numbers (z20 &gt; z3) and assumed positive
    ///     (-100 comes AFTER 10 and 100, because the minus is seen
    ///         as a char, not as part of the number)
    /// * leading zeroes come before anything else (z001 &lt; z01 &lt; z1)
    /// 
    /// Note: Instead of instantiating this, you can also use
    /// <see cref="Comparison(string, string)"/>
    /// if you don't need an <see cref="IComparer{string}"/> but can
    /// use a <see cref="Comparison{string}"/> delegate instead.
    /// </summary>
    /// <remarks>
    /// NOTE: This behaves slightly different than StrCmpLogicalW because
    /// it handles large numbers.
    /// 
    /// At some point, StrCmpLogicalW just gives up trying to parse
    /// something as a number (see the Test cases), while we keep going.
    /// Since we want to sort lexicographily as much as possible,
    /// that difference makes sense.
    /// </remarks>
    public class LexicographicStringComparer : IComparer<string>
    {
        /// <summary>
        /// A <see cref="Comparison{string}"/> delegate.
        /// </summary>
        public static int Comparison(string x, string y)
        {
            // 1 = x > y, -1 = y > x, 0 = x == y
            // Rules: Numbers < Letters. Space < everything
            if (x == y) return 0;
            if (string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y)) return -1;
            if (!string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y)) return 1;
            if (string.IsNullOrEmpty(x) && string.IsNullOrEmpty(y)) return 0; // "" and null are the same for the purposes of this

            var yl = y.Length;
            for (int i = 0; i < x.Length; i++)
            {
                if (yl <= i) return 1;
                var cx = x[i];
                var cy = y[i];

                if (Char.IsWhiteSpace(cx) && !Char.IsWhiteSpace(cy)) return -1;
                if (!Char.IsWhiteSpace(cx) && Char.IsWhiteSpace(cy)) return 1;

                if (IsDigit(cx))
                {
                    if (!IsDigit(cy))
                    {
                        return -1;
                    }

                    // Both are digits, but now we need to look at them as a whole, since
                    // 10 > 2, but 10 > 002 > 02 > 2
                    var numCmp = CompareNumbers(x, y, i, out int numChars);
                    if (numCmp != 0) return numCmp;
                    i += numChars; // We might have looked at more than one char, e.g., "10" is 2 chars
                }
                else if (IsDigit(cy))
                {
                    return 1;
                }
                else
                {
                    // Do this after the digit check
                    // Case insensitive
                    // Normalize to Uppercase:
                    // https://docs.microsoft.com/en-US/visualstudio/code-quality/ca1308-normalize-strings-to-uppercase
                    var cmp = Char.ToUpper(cx).CompareTo(Char.ToUpper(cy));
                    if (cmp != 0) return cmp;
                }
            }

            // Strings are equal to that point, and y is at least as large as x
            if (y.Length > x.Length) return -1;

            return 0;
        }

        /// <summary>
        /// <see cref="IComparer{T}.Compare(T, T)"/>
        /// </summary>
        public int Compare(string x, string y)
            => Comparison(x, y);

        private static int CompareNumbers(string x, string y, int ix, out int numChars)
        {
            var xParsed = ParseNumber(x, ix);
            var yParsed = ParseNumber(y, ix);

            numChars = yParsed.NumCharsRead > xParsed.NumCharsRead
                ? xParsed.NumCharsRead
                : yParsed.NumCharsRead;

            return xParsed.CompareTo(yParsed);
        }

        private static ParsedNumber ParseNumber(string str, int offset)
        {
            var result = 0;
            var numChars = 0;
            var leadingZeroes = 0;
            var numOverflows = 0;
            bool countZeroes = true;

            for (int j = offset; j < str.Length; j++)
            {
                char c = str[j];
                if (IsDigit(c))
                {
                    var cInt = (c - 48); // 48/0x30 is '0'

                    checked
                    {
                        long tmp = (result * 10L) + cInt;
                        if (tmp > int.MaxValue)
                        {
                            numOverflows++;
                            tmp = tmp % int.MaxValue;
                        }
                        result = (int)tmp;
                        numChars++;
                    }

                    if (cInt == 0 && countZeroes)
                    {
                        leadingZeroes++;
                    }
                    else
                    {
                        countZeroes = false;
                    }
                }
                else
                {
                    break;
                }
            }
            return new ParsedNumber(result, numOverflows, leadingZeroes, numChars);
        }

        private static bool IsDigit(char c) => (c >= '0' && c <= '9');

        /// <summary>
        /// Note that the ParsedNumber is not very useful as a number,
        /// but purely as a way to compare two numbers that are stored in a string.
        /// </summary>
        private struct ParsedNumber : IComparable<ParsedNumber>, IComparer<ParsedNumber>
        {
            /// <summary>
            /// The remainder, that is, the part of the number that
            /// didn't overflow int.MaxValue.
            /// </summary>
            public int Remainder;

            /// <summary>
            /// How often did the number overflow int.MaxValue during parsing?
            /// </summary>
            public int Overflows;

            /// <summary>
            /// How many leading zeroes were there in the string during parsing?
            /// "001" has a LeadingZeroesCount of 2.
            /// "100" has a LeadingZeroesCount of 0.
            /// "010" has a LeadingZeroesCount of 1.
            /// 
            /// This is important, because 001 comes before 01 comes before 1.
            /// </summary>
            public int LeadingZeroesCount;

            /// <summary>
            /// How many characters were read from the input during parsing?
            /// </summary>
            public int NumCharsRead;

            public ParsedNumber(int remainder, int overflows, int leadingZeroes, int numChars)
            {
                Remainder = remainder;
                Overflows = overflows;
                LeadingZeroesCount = leadingZeroes;
                NumCharsRead = numChars;
            }

            public int Compare(ParsedNumber x, ParsedNumber y)
            {
                // Note: if numCharsX and Y aren't equal, this doesn't matter
                // as the return value will be either -1 or 1 anyway

                if (x.Overflows > y.Overflows) return 1;
                if (x.Overflows < y.Overflows) return -1;

                // 001 > 01 > 1
                if (x.Remainder == y.Remainder)
                {
                    if (x.LeadingZeroesCount > y.LeadingZeroesCount) return -1;
                    if (x.LeadingZeroesCount < y.LeadingZeroesCount) return 1;
                }

                if (x.Remainder > y.Remainder) return 1;
                if (x.Remainder < y.Remainder) return -1;
                return 0;
            }

            public int CompareTo(ParsedNumber other)
                => Compare(this, other);
        }
    }
}
