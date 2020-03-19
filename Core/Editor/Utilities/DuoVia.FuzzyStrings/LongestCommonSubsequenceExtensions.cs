/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Derived from http://www.codeproject.com/KB/recipes/lcs.aspx 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace DuoVia.FuzzyStrings
{
	internal static class LongestCommonSubsequenceExtensions
	{
		/// <summary>
		/// Longest Common Subsequence. A good value is greater than 0.33.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="comparedTo"></param>
		/// <param name="caseSensitive"></param>
		/// <returns>Returns a Tuple of the sub sequence string and the match coeficient.</returns>
		public static Tuple<string, double> LongestCommonSubsequence(this string input, string comparedTo, bool caseSensitive = false)
		{
			if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo)) return new Tuple<string, double>(string.Empty, 0.0d);
			if (!caseSensitive)
			{
				input = input.ToLower();
				comparedTo = comparedTo.ToLower();
			}

			int inputLen = input.Length;
			int comparedToLen = comparedTo.Length;

			int[,] lcs = new int[inputLen + 1, comparedToLen + 1];
			LcsDirection[,] tracks = new LcsDirection[inputLen + 1, comparedToLen + 1];
			int[,] w = new int[inputLen + 1, comparedToLen + 1];
			int i, j;

			for (i = 0; i <= inputLen; ++i)
			{
				lcs[i, 0] = 0;
				tracks[i, 0] = LcsDirection.North;

			}
			for (j = 0; j <= comparedToLen; ++j)
			{
				lcs[0, j] = 0;
				tracks[0, j] = LcsDirection.West;
			}

			for (i = 1; i <= inputLen; ++i)
			{
				for (j = 1; j <= comparedToLen; ++j)
				{
					if (input[i - 1].Equals(comparedTo[j - 1]))
					{
						int k = w[i - 1, j - 1];
						//lcs[i,j] = lcs[i-1,j-1] + 1;
						lcs[i, j] = lcs[i - 1, j - 1] + Square(k + 1) - Square(k);
						tracks[i, j] = LcsDirection.NorthWest;
						w[i, j] = k + 1;
					}
					else
					{
						lcs[i, j] = lcs[i - 1, j - 1];
						tracks[i, j] = LcsDirection.None;
					}

					if (lcs[i - 1, j] >= lcs[i, j])
					{
						lcs[i, j] = lcs[i - 1, j];
						tracks[i, j] = LcsDirection.North;
						w[i, j] = 0;
					}

					if (lcs[i, j - 1] >= lcs[i, j])
					{
						lcs[i, j] = lcs[i, j - 1];
						tracks[i, j] = LcsDirection.West;
						w[i, j] = 0;
					}
				}
			}

			i = inputLen;
			j = comparedToLen;

			string subseq = "";
			double p = lcs[i, j];

			//trace the backtracking matrix.
			while (i > 0 || j > 0)
			{
				if (tracks[i, j] == LcsDirection.NorthWest)
				{
					i--;
					j--;
					subseq = input[i] + subseq;
					//Trace.WriteLine(i + " " + input1[i] + " " + j);
				}

				else if (tracks[i, j] == LcsDirection.North)
				{
					i--;
				}

				else if (tracks[i, j] == LcsDirection.West)
				{
					j--;
				}
			}

			double coef = p / (inputLen * comparedToLen);

			Tuple<string, double> retval = new Tuple<string, double>(subseq, coef);
			return retval;
		}

		private static int Square(int p)
		{
			return p * p;
		}
	}

	internal enum LcsDirection
	{
		None,
		North,
		West,
		NorthWest
	}
}
