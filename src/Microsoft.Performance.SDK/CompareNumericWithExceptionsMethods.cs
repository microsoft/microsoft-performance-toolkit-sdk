// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;

namespace Microsoft.Performance.SDK
{
    internal static class CompareWithNumericExceptionMethods
    {
        public static int _wcsicmpWithNumericExceptions(string string1, string string2)
        {
            unsafe
            {
                fixed (char* _string1 = string1)
                fixed (char* _string2 = string2)
                {
                    return _wcsicmpWithNumericExceptions(_string1, _string2);
                }
            }
        }

        public static int _wcscmpWithNumericExceptions(string string1, string string2)
        {
            unsafe
            {
                fixed (char* _string1 = string1)
                fixed (char* _string2 = string2)
                {
                    return _wcscmpWithNumericExceptions(_string1, _string2);
                }
            }
        }

        private const int firstLessThanSecond = -1;
        private const int firstEqualsSecond = 0;
        private const int secondLessThanFirst = 1;
        private const char NULL = (char)0;

        private unsafe static bool isHexString(char* string1)
        {
            return (string1[0] == '0') && (string1[1] == 'x');
        }

        private unsafe static bool IsGuid(char* string1)
        {
            return IsNextNCharTraits<HexCharTraits>(8, ref string1) &&
                   IsNextChar('-', ref string1) &&
                   IsNextNCharTraits<HexCharTraits>(4, ref string1) &&
                   IsNextChar('-', ref string1) &&
                   IsNextNCharTraits<HexCharTraits>(4, ref string1) &&
                   IsNextChar('-', ref string1) &&
                   IsNextNCharTraits<HexCharTraits>(4, ref string1) &&
                   IsNextChar('-', ref string1) &&
                   IsNextNCharTraits<HexCharTraits>(12, ref string1) &&
                   !iswxdigit(*string1);
        }

        private unsafe static bool IsNextChar(char c, ref char* string1)
        {
            if (*string1 == c)
            {
                ++string1;
                return true;
            }
            return false;
        }

        private unsafe static bool IsNextNCharTraits<TCharTraits>(int n, ref char* string1)
            where TCharTraits : ICharTraits2, new()
        {
            TCharTraits charTraits = new TCharTraits();

            while (n > 0)
            {
                if (!charTraits.IsDigit(*string1))
                {
                    return false;
                }
                ++string1;
                --n;
            }

            return true;
        }

        private unsafe static void EatChars(ref char* string1, char c)
        {
            while (*string1 == c)
            {
                ++string1;
            }
        }

        private static bool iswxdigit(char c)
        {
            return isdigit(c) || isBetween(c, 'a', 'f') || isBetween(c, 'A', 'F');
        }

        private static bool isBetween(char c, char left, char right)
        {
            return (left <= c) && (c <= right);
        }

        private static bool isdigit(char c)
        {
            return isBetween(c, '0', '9');
        }

        private interface ICharTraits2
        {
            bool IsDigit(char c);
        }

        private struct HexCharTraits
            : ICharTraits2
        {
            public bool IsDigit(char c)
            {
                return iswxdigit(c);
            }
        }

        private struct DigitCharTraits
            : ICharTraits2
        {
            public bool IsDigit(char c)
            {
                return isdigit(c);
            }
        };


        private static unsafe int _wcsicmpWithNumericExceptions(char* string1, char* string2)
        {
            return _wcstcmpWithNumericExceptions<CaseInsenstiveCharTraits>(string1, string2);
        }

        //
        // This method compares input strings as case insensitive at first,
        //  and then as case sensitive.
        // This affects all upper sort algorithms which use this for comparing.
        // For example, the sort result of {"A", "Z", "a"} is {"A", "a", "Z"}
        //  as ascending sequence.
        //

        private static unsafe int _wcscmpWithNumericExceptions(char* string1, char* string2)
        {
            int icmpresult = _wcsicmpWithNumericExceptions(string1, string2);

            if (icmpresult != 0)
                return icmpresult;
            else
                return _wcstcmpWithNumericExceptions<CaseSenstiveCharTraits>(string1, string2);
        }

        private interface ICharTraits
        {
            char ConvertChar(char c);
            int CompareConvertedChar(char c1, char c2);
        }

        private struct CaseInsenstiveCharTraits
            : ICharTraits
        {
            public char ConvertChar(char c)
            {
                return char.ToLower(c, CultureInfo.CurrentCulture);
            }

            public int CompareConvertedChar(char c1, char c2)
            {
                return (c1 < c2) ? firstLessThanSecond : secondLessThanFirst;
            }
        }

        private struct CaseSenstiveCharTraits
            : ICharTraits
        {
            public char ConvertChar(char c)
            {
                return c;
            }

            //
            // for case sensitive traits
            // make sure that capitals and lowers of the same 
            // letter sort together
            // capitals of the same letter still come before lowers of the same letter.
            // c1 and c2 are already presumed to be different.
            //                
            public int CompareConvertedChar(char c1, char c2)
            {
                var caseInsenstiveCharTraits = new CaseInsenstiveCharTraits();

                char c1Lowered = caseInsenstiveCharTraits.ConvertChar(c1);
                char c2Lowered = caseInsenstiveCharTraits.ConvertChar(c2);

                bool fFirstCharChangedFromUpperToLower = (c1Lowered != c1);
                bool fSecondCharChangedFromUpperToLower = (c2Lowered != c2);

                if (c1Lowered != c2Lowered)
                {
                    return caseInsenstiveCharTraits.CompareConvertedChar(c1Lowered, c2Lowered);
                }
                else
                {
                    if (fFirstCharChangedFromUpperToLower && !fSecondCharChangedFromUpperToLower)
                    {
                        return firstLessThanSecond;
                    }
                    else
                    {
                        return secondLessThanFirst;
                    }
                }
            }
        };

        private static unsafe int _wcstcmpWithNumericExceptions<TCharTraits>(char* string1, char* string2)
            where TCharTraits : ICharTraits, new()
        {
            if (string1 == string2)
            {
                return firstEqualsSecond;
            }
            else if (string1 == null)
            {
                return firstLessThanSecond;
            }
            else if (string2 == null)
            {
                return secondLessThanFirst;
            }

            var charTraits = new TCharTraits();

            bool fTreatAsNegative = false;
            if ((*string1 == '-') && (*string2 == '-'))
            {
                fTreatAsNegative = true;
                ++string1;
                ++string2;
            }

            //
            // Allow variable length hex strings at the beginning only
            // must be preceded with 0x specifier to qualify for this.
            //
            if (isHexString(string1) && isHexString(string2))
            {
                int cchHexPrefix = 2; // = ARRAYSIZE(L"0x");

                string1 += cchHexPrefix;
                string2 += cchHexPrefix;

                EatChars(ref string1, '0');
                EatChars(ref string2, '0');

                while (iswxdigit(*string1) && iswxdigit(*string2))
                {
                    if (*string1 != *string2)
                    {
                        char firstChar = charTraits.ConvertChar(*string1);
                        char secondChar = charTraits.ConvertChar(*string2);

                        if (firstChar != secondChar)
                        {
                            int result = charTraits.CompareConvertedChar(firstChar, secondChar);

                            CheckVariableRunOfHexAndReverseResultIfNecessary(string1, string2, ref result, false);
                            return fTreatAsNegative ? -result : result;
                        }
                    }
                    ++string1;
                    ++string2;
                }

                fTreatAsNegative = false;
                //continue;
            }

            // put all hex strings before non hex strings.
            // so that the 0 in the 0x of the hex string isn't mistaken for a number later
            if (isHexString(string1) ^ isHexString(string2))
            {
                return isHexString(string1) ? firstLessThanSecond : secondLessThanFirst;
            }

            bool isString1Guid = IsGuid(string1);
            bool isString2Guid = IsGuid(string2);

            if (isString1Guid && isString2Guid)
            {
                while ((*string1 != NULL) && (*string2 != NULL))
                {
                    if (*string1 != *string2)
                    {
                        char firstChar = charTraits.ConvertChar(*string1);
                        char secondChar = charTraits.ConvertChar(*string2);

                        if (firstChar != secondChar)
                        {
                            int result = charTraits.CompareConvertedChar(firstChar, secondChar);
                            return result;
                        }
                    }

                    ++string1;
                    ++string2;
                }
            }
            else if (isString1Guid ^ isString2Guid)
            {
                // guids before non guids.
                return isString1Guid ? firstLessThanSecond : secondLessThanFirst;
            }

            bool atBeginningOfNumber = true;

            while ((*string1 != NULL) && (*string2 != NULL))
            {
                if (*string1 != *string2)
                {
                    char firstChar = charTraits.ConvertChar(*string1);
                    char secondChar = charTraits.ConvertChar(*string2);

                    if (firstChar != secondChar)
                    {
                        int result = charTraits.CompareConvertedChar(firstChar, secondChar);

                        if (AdjustForNumericalRemainder(string1, string2, ref result, atBeginningOfNumber))
                        {
                            return fTreatAsNegative ? -result : result;
                        }

                        return result;
                    }

                    if (fTreatAsNegative && !isdigit(firstChar))
                    {
                        fTreatAsNegative = false;
                    }
                }

                atBeginningOfNumber = !isBetween(*string1, atBeginningOfNumber ? '1' : '0', '9');

                ++string1;
                ++string2;
            }

            return ((*string1 != NULL) && (*string2 == NULL)) ? fTreatAsNegative ? firstLessThanSecond : secondLessThanFirst :
                   ((*string1 == NULL) && (*string2 != NULL)) ? fTreatAsNegative ? secondLessThanFirst : firstLessThanSecond :
                   firstEqualsSecond;
        }

        static unsafe private bool tAdjustForNumericalRemainder<TCharTraits>(char* string1, char* string2, ref int result, bool atBeginningOfNumber)
            where TCharTraits : ICharTraits2, new()
        {
            var charTraits = new TCharTraits();

            if (atBeginningOfNumber)
            {
                EatChars(ref string1, '0');
                EatChars(ref string2, '0');
            }

            bool isDigit1 = charTraits.IsDigit(*string1);
            bool isDigit2 = charTraits.IsDigit(*string2);

            if (isDigit1 && isDigit2)
            {
                do
                {
                    ++string1;
                    ++string2;
                    isDigit1 = charTraits.IsDigit(*string1);
                    isDigit2 = charTraits.IsDigit(*string2);
                }
                while (isDigit1 && isDigit2);

                if (result == firstLessThanSecond)
                {
                    // check if the numeric run length is longer and invert
                    if (isDigit1 && !isDigit2)
                    {
                        result = secondLessThanFirst;
                    }
                }
                else if (result == secondLessThanFirst)
                {
                    // check if the numeric run length is longer and invert
                    if (!isDigit1 && isDigit2)
                    {
                        result = firstLessThanSecond;
                    }
                }
                return true;
            }
            else if (isDigit1 ^ isDigit2)
            {
                result = isDigit1 ? secondLessThanFirst : firstLessThanSecond;
                return true;
            }
            return false;
        }

        private static unsafe bool AdjustForNumericalRemainder(char* string1, char* string2, ref int result, bool atBeginningOfNumber)
        {
            return tAdjustForNumericalRemainder<DigitCharTraits>(string1, string2, ref result, atBeginningOfNumber);
        }

        private static unsafe bool CheckVariableRunOfHexAndReverseResultIfNecessary(char* string1, char* string2, ref int result, bool atBeginningOfNumber)
        {
            return tAdjustForNumericalRemainder<HexCharTraits>(string1, string2, ref result, atBeginningOfNumber);
        }
    }
}
