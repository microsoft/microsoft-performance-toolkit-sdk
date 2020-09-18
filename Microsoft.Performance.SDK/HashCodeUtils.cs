// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Performance.SDK
{
    /// <summary>
    ///     Provides fast methods for computing hash codes.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils")]
    public static class HashCodeUtils
    {
        //
        // Note: These methods exist so that the JITter can reason about
        // them. Having one method with just params int[] does not allow
        // the best JIT optimizations.
        //

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        public static int CombineHashCodeValues(int hc1, int hc2)
        {
            // This is the method for combining hash codes that the BCL uses
            // (e.g. Array.CombineHashCodes(), Tuple.CombineHashCodes())
            return unchecked(((hc1 << 5) + hc1) ^ hc2);
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2), 
                hc3);
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2), 
                CombineHashCodeValues(hc3, hc4));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4), 
                hc5);
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4), 
                CombineHashCodeValues(hc5, hc6));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4), 
                CombineHashCodeValues(hc5, hc6, hc7));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4), 
                CombineHashCodeValues(hc5, hc6, hc7, hc8));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <param name="hc10">
        ///     The tenth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9, int hc10)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9, hc10));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <param name="hc10">
        ///     The tenth integer.
        /// </param>
        /// <param name="hc11">
        ///     The eleventh integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9, int hc10, int hc11)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9, hc10),
                hc11);
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <param name="hc10">
        ///     The tenth integer.
        /// </param>
        /// <param name="hc11">
        ///     The eleventh integer.
        /// </param>
        /// <param name="hc12">
        ///     The twelfth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9, int hc10, int hc11, int hc12)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9, hc10),
                CombineHashCodeValues(hc11, hc12));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <param name="hc10">
        ///     The tenth integer.
        /// </param>
        /// <param name="hc11">
        ///     The eleventh integer.
        /// </param>
        /// <param name="hc12">
        ///     The twelfth integer.
        /// </param>
        /// <param name="hc13">
        ///     The thirteenth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9, int hc10, int hc11, int hc12, int hc13)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9, hc10),
                CombineHashCodeValues(hc11, hc12, hc13));
        }

        /// <summary>
        ///     Combines the given integers into another integer.
        ///     <para/>
        ///     This uses the method that the BCL uses in its classes
        ///     internally.
        /// </summary>
        /// <param name="hc1">
        ///     The first integer.
        /// </param>
        /// <param name="hc2">
        ///     The second integer.
        /// </param>
        /// <param name="hc3">
        ///     The third integer.
        /// </param>
        /// <param name="hc4">
        ///     The fourth integer.
        /// </param>
        /// <param name="hc5">
        ///     The fifth integer.
        /// </param>
        /// <param name="hc6">
        ///     The sixth integer.
        /// </param>
        /// <param name="hc7">
        ///     The seventh integer.
        /// </param>
        /// <param name="hc8">
        ///     The eight integer.
        /// </param>
        /// <param name="hc9">
        ///     The ninth integer.
        /// </param>
        /// <param name="hc10">
        ///     The tenth integer.
        /// </param>
        /// <param name="hc11">
        ///     The eleventh integer.
        /// </param>
        /// <param name="hc12">
        ///     The twelfth integer.
        /// </param>
        /// <param name="hc13">
        ///     The thirteenth integer.
        /// </param>
        /// <param name="hc14">
        ///     The fourteenth integer.
        /// </param>
        /// <returns>
        ///     A new integer representing a combination of the
        ///     given integers.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray")]
        public static int CombineHashCodeValues(int hc1, int hc2, int hc3, int hc4, int hc5, int hc6, int hc7, int hc8, int hc9, int hc10, int hc11, int hc12, int hc13, int hc14)
        {
            return CombineHashCodeValues(
                CombineHashCodeValues(hc1, hc2, hc3, hc4, hc5),
                CombineHashCodeValues(hc6, hc7, hc8, hc9, hc10),
                CombineHashCodeValues(hc11, hc12, hc13, hc14));
        }
    }
}
