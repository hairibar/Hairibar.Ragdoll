/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Derived from http://www.codeguru.com/vb/gen/vb_misc/algorithms/article.php/c13137__1/Fuzzy-Matching-Demo-in-Access.htm
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System.Linq;

namespace DuoVia.FuzzyStrings
{
	internal static class DiceCoefficientExtensions
	{
		/// <summary>
		/// Dice Coefficient based on bigrams. <br />
		/// A good value would be 0.33 or above, a value under 0.2 is not a good match, from 0.2 to 0.33 is iffy.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="comparedTo"></param>
		/// <returns></returns>
		public static double DiceCoefficient(this string input, string comparedTo)
		{
			var ngrams = input.ToBiGrams();
			var compareToNgrams = comparedTo.ToBiGrams();
			return ngrams.DiceCoefficient(compareToNgrams);
		}

		/// <summary>
		/// Dice Coefficient used to compare nGrams arrays produced in advance.
		/// </summary>
		/// <param name="nGrams"></param>
		/// <param name="compareToNGrams"></param>
		/// <returns></returns>
		public static double DiceCoefficient(this string[] nGrams, string[] compareToNGrams)
		{
			int matches = 0;
			foreach (var nGram in nGrams)
			{
				if (compareToNGrams.Any(x => x == nGram)) matches++;
			}
			if (matches == 0) return 0.0d;
			double totalBigrams = nGrams.Length + compareToNGrams.Length;
			return (2 * matches) / totalBigrams;
		}

		public static string[] ToBiGrams(this string input)
		{
			// nLength == 2
			//   from Jackson, return %j ja ac ck ks so on n#
			//   from Main, return #m ma ai in n#
			input = SinglePercent + input + SinglePound;
			return ToNGrams(input, 2);
		}

		public static string[] ToTriGrams(this string input)
		{
			// nLength == 3
			//   from Jackson, return %%j %ja jac ack cks kso son on# n##
			//   from Main, return ##m #ma mai ain in# n##
			input = DoublePercent + input + DoublePount;
			return ToNGrams(input, 3);
		}

		private static string[] ToNGrams(string input, int nLength)
		{
			int itemsCount = input.Length - 1;
			string[] ngrams = new string[input.Length - 1];
			for (int i = 0; i < itemsCount; i++) ngrams[i] = input.Substring(i, nLength);
			return ngrams;
		}

		private const string SinglePercent = "%";
		private const string SinglePound = "#";
		private const string DoublePercent = "&&";
		private const string DoublePount = "##";
	}
}
