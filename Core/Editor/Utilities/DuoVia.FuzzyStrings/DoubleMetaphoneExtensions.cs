/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Derived from http://doublemetaphone.googlecode.com/svn/tags/1/DoubleMetaphone.cs 
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
/* Origianal Work Copyright Notice included here as required: 

        Copyright (c) 2008 Anthony Tong Lee

        Permission is hereby granted, free of charge, to any person
        obtaining a copy of this software and associated documentation
        files (the "Software"), to deal in the Software without
        restriction, including without limitation the rights to use,
        copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the
        Software is furnished to do so, subject to the following
        conditions:

        The above copyright notice and this permission notice shall be
        included in all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
        EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
        OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
        NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
        HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
        WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
        FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
        OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;

namespace DuoVia.FuzzyStrings
{
    /// <summary>
    /// DoubleMetaphone string extension
    /// </summary>
    /// <remarks>
    /// Original C++ implementation:
    ///		"Double Metaphone (c) 1998, 1999 by Lawrence Philips"
    ///		http://www.ddj.com/cpp/184401251?pgno=1
    /// </remarks>
    internal static class DoubleMetaphoneExtensions
    {
        public static string ToDoubleMetaphone(this string input)
        {
            MetaphoneData metaphoneData = new MetaphoneData();
            int current = 0;

            if (input.Length < 1)
            {
                return input;
            }
            int last = input.Length - 1; //zero based index

            string workingString = input.ToUpperInvariant() + "     ";

            bool isSlavoGermanic = (input.IndexOf(charW) > -1)
                || (input.IndexOf(charK) > -1)
                || (input.IndexOf(strCZ, StringComparison.OrdinalIgnoreCase) > -1)
                || (input.IndexOf(strWITZ, StringComparison.OrdinalIgnoreCase) > -1);

            //skip these when at start of word
            if (workingString.StartsWith(StringComparison.OrdinalIgnoreCase, strGN, strKN, strPN, strWR, strPS))
            {
                current += 1;
            }

            //Initial 'X' is pronounced 'Z' e.g. 'Xavier'
            if (workingString[0] == charX)
            {
                metaphoneData.Add(strS); //'Z' maps to 'S'
                current += 1;
            }

            while ((metaphoneData.PrimaryLength < 4) || (metaphoneData.SecondaryLength < 4))
            {
                if (current >= input.Length)
                {
                    break;
                }

                switch (workingString[current])
                {
                    case charA:
                    case charE:
                    case charI:
                    case charO:
                    case charU:
                    case charY:
                        if (current == 0)
                        {
                            //all init vowels now map to 'A'
                            metaphoneData.Add("A");
                        }
                        current += 1;
                        break;

                    case charB:
                        //"-mb", e.g", "dumb", already skipped over...
                        metaphoneData.Add("P");

                        if (workingString[current + 1] == charB)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charAdash:
                        metaphoneData.Add(strS);
                        current += 1;
                        break;

                    case charC:
                        //various germanic
                        if ((current > 1)
                            && !IsVowel(workingString[current - 2])
                            && StringAt(workingString, (current - 1), strACH)
                            && ((workingString[current + 2] != charI)
                            && ((workingString[current + 2] != charE)
                            || StringAt(workingString, (current - 2), strBACHER, strMACHER))))
                        {
                            metaphoneData.Add(strK);
                            current += 2;
                            break;
                        }

                        //special case 'caesar'
                        if ((current == 0) && StringAt(workingString, current, strCAESAR))
                        {
                            metaphoneData.Add(strS);
                            current += 2;
                            break;
                        }

                        //italian 'chianti'
                        if (StringAt(workingString, current, strCHIA))
                        {
                            metaphoneData.Add(strK);
                            current += 2;
                            break;
                        }

                        if (StringAt(workingString, current, strCH))
                        {
                            //find 'michael'
                            if ((current > 0) && StringAt(workingString, current, strCHAE))
                            {
                                metaphoneData.Add(strK, strX);
                                current += 2;
                                break;
                            }

                            //greek roots e.g. 'chemistry', 'chorus'
                            if ((current == 0)
                                && (StringAt(workingString, (current + 1), strHARAC, strHARIS)
                                || StringAt(workingString, (current + 1), strHOR, strHYM, strHIA, strHEM))
                                && !StringAt(workingString, 0, strCHORE))
                            {
                                metaphoneData.Add(strK);
                                current += 2;
                                break;
                            }

                            //germanic, greek, or otherwise 'ch' for 'kh' sound
                            if ((StringAt(workingString, 0, strVANsp, strVONsp)
                                || StringAt(workingString, 0, strSCH)) // 'architect but not 'arch', 'orchestra', 'orchid'
                                || StringAt(workingString, (current - 2), strORCHES, strARCHIT, strORCHID)
                                || StringAt(workingString, (current + 2), strT, strS)
                                    || ((StringAt(workingString, (current - 1), strA, strO, strU, strE)
                                    || (current == 0)) //e.g., 'wachtler', 'wechsler', but not 'tichner'
                                        && StringAt(workingString, (current + 2), strL, strR, strN, strM, strB, strH, strF, strV, strW, sp)))
                            {
                                metaphoneData.Add(strK);
                            }
                            else
                            {
                                if (current > 0)
                                {
                                    if (StringAt(workingString, 0, strMC))
                                    {
                                        //e.g., "McHugh"
                                        metaphoneData.Add(strK);
                                    }
                                    else
                                    {
                                        metaphoneData.Add(strX, strK);
                                    }
                                }
                                else
                                {
                                    metaphoneData.Add(strX);
                                }
                            }
                            current += 2;
                            break;
                        }
                        //e.g, 'czerny'
                        if (StringAt(workingString, current, strCZ) && !StringAt(workingString, (current - 2), strWICZ))
                        {
                            metaphoneData.Add(strS, strX);
                            current += 2;
                            break;
                        }

                        //e.g., 'focaccia'
                        if (StringAt(workingString, (current + 1), strCIA))
                        {
                            metaphoneData.Add(strX);
                            current += 3;
                            break;
                        }

                        //double 'C', but not if e.g. 'McClellan'
                        if (StringAt(workingString, current, strCC) && !((current == 1) && (workingString[0] == charM)))
                        {
                            //'bellocchio' but not 'bacchus'
                            if (StringAt(workingString, (current + 2), strI, strE, strH)
                                && !StringAt(workingString, (current + 2), strHU))
                            {
                                //'accident', 'accede' 'succeed'
                                if (((current == 1) && (workingString[current - 1] == charA))
                                    || StringAt(workingString, (current - 1), strUCCEE, strUCCES))
                                {
                                    metaphoneData.Add(strKS);
                                }
                                //'bacci', 'bertucci', other italian
                                else
                                {
                                    metaphoneData.Add(strX);
                                }
                                current += 3;
                                break;
                            }
                            else
                            {
                                //Pierce's rule
                                metaphoneData.Add(strK);
                                current += 2;
                                break;
                            }
                        }

                        if (StringAt(workingString, current, strCK, strCG, strCQ))
                        {
                            metaphoneData.Add(strK);
                            current += 2;
                            break;
                        }

                        if (StringAt(workingString, current, strCI, strCE, strCY))
                        {
                            //italian vs. english
                            if (StringAt(workingString, current, strCIO, strCIE, strCIA))
                            {
                                metaphoneData.Add(strS, strX);
                            }
                            else
                            {
                                metaphoneData.Add(strS);
                            }
                            current += 2;
                            break;
                        }

                        //else
                        metaphoneData.Add(strK);

                        //name sent in 'mac caffrey', 'mac gregor
                        if (StringAt(workingString, (current + 1), strspC, strspQ, strspG))
                        {
                            current += 3;
                        }
                        else if (StringAt(workingString, (current + 1), strC, strK, strQ)
                            && !StringAt(workingString, (current + 1), strCE, strCI))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charD:
                        if (StringAt(workingString, current, strDG))
                        {
                            if (StringAt(workingString, (current + 2), strI, strE, strY))
                            {
                                //e.g. 'edge'
                                metaphoneData.Add(strJ);
                                current += 3;
                                break;
                            }
                            else
                            {
                                //e.g. 'edgar'
                                metaphoneData.Add(strTK);
                                current += 2;
                                break;
                            }
                        }

                        if (StringAt(workingString, current, strDT, strDD))
                        {
                            metaphoneData.Add(strT);
                            current += 2;
                            break;
                        }

                        //else
                        metaphoneData.Add(strT);
                        current += 1;
                        break;

                    case charF:
                        if (workingString[current + 1] == charF)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strF);
                        break;

                    case charG:
                        if (workingString[current + 1] == charH)
                        {
                            if ((current > 0) && !IsVowel(workingString[current - 1]))
                            {
                                metaphoneData.Add(strK);
                                current += 2;
                                break;
                            }

                            if (current < 3)
                            {
                                //'ghislane', ghiradelli
                                if (current == 0)
                                {
                                    if (workingString[current + 2] == charI)
                                    {
                                        metaphoneData.Add(strJ);
                                    }
                                    else
                                    {
                                        metaphoneData.Add(strK);
                                    }
                                    current += 2;
                                    break;
                                }
                            }
                            //Parker's rule (with some further refinements) - e.g., 'hugh'
                            if (((current > 1) && StringAt(workingString, (current - 2), strB, strH, strD)) //e.g., 'bough'
                                || ((current > 2) && StringAt(workingString, (current - 3), strB, strH, strD)) //e.g., 'broughton'
                                    || ((current > 3) && StringAt(workingString, (current - 4), strB, strH)))
                            {
                                current += 2;
                                break;
                            }
                            else
                            {
                                //e.g., 'laugh', 'McLaughlin', 'cough', 'gough', 'rough', 'tough'
                                if ((current > 2) && (workingString[current - 1] == charU)
                                    && StringAt(workingString, (current - 3), strC, strG, strL, strR, strT))
                                {
                                    metaphoneData.Add(strF);
                                }
                                else if ((current > 0) && workingString[current - 1] != charI)
                                {
                                    metaphoneData.Add(strK);
                                }

                                current += 2;
                                break;
                            }
                        }

                        if (workingString[current + 1] == charN)
                        {
                            if ((current == 1) && IsVowel(workingString[0]) && !isSlavoGermanic)
                            {
                                metaphoneData.Add(strKN, strN);
                            }
                            else
                                //not e.g. 'cagney'
                                if (!StringAt(workingString, (current + 2), strEY)
                                    && (workingString[current + 1] != charY) && !isSlavoGermanic)
                            {
                                metaphoneData.Add(strN, strKN);
                            }
                            else
                            {
                                metaphoneData.Add(strKN);
                            }
                            current += 2;
                            break;
                        }

                        //'tagliaro'
                        if (StringAt(workingString, (current + 1), strLI) && !isSlavoGermanic)
                        {
                            metaphoneData.Add(strKL, strL);
                            current += 2;
                            break;
                        }

                        //-ges-,-gep-,-gel-, -gie- at beginning
                        if ((current == 0)
                            && ((workingString[current + 1] == charY)
                            || StringAt(workingString, (current + 1), strES, strEP, strEB, strEL, strEY, strIB, strIL, strIN, strIE, strEI, strER)))
                        {
                            metaphoneData.Add(strK, strJ);
                            current += 2;
                            break;
                        }

                        // -ger-,  -gy-
                        if ((StringAt(workingString, (current + 1), strER)
                            || (workingString[current + 1] == charY))
                            && !StringAt(workingString, 0, strDANGER, strRANGER, strMANGER)
                            && !StringAt(workingString, (current - 1), strE, strI)
                            && !StringAt(workingString, (current - 1), strRGY, strOGY))
                        {
                            metaphoneData.Add(strK, strJ);
                            current += 2;
                            break;
                        }

                        // italian e.g, 'biaggi'
                        if (StringAt(workingString, (current + 1), strE, strI, strY)
                            || StringAt(workingString, (current - 1), strAGGI, strOGGI))
                        {
                            //obvious germanic
                            if ((StringAt(workingString, 0, strVANsp, strVONsp)
                                || StringAt(workingString, 0, strSCH))
                                || StringAt(workingString, (current + 1), strET))
                            {
                                metaphoneData.Add(strK);
                            }
                            else
                                //always soft if french ending
                                if (StringAt(workingString, (current + 1), strIERsp))
                            {
                                metaphoneData.Add(strJ);
                            }
                            else
                            {
                                metaphoneData.Add(strJ, strK);
                            }
                            current += 2;
                            break;
                        }

                        if (workingString[current + 1] == charG)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strK);
                        break;

                    case 'H':
                        //only keep if first & before vowel or btw. 2 vowels
                        if (((current == 0) || IsVowel(workingString[current - 1])) && IsVowel(workingString[current + 1]))
                        {
                            metaphoneData.Add(strH);
                            current += 2;
                        }
                        else //also takes care of 'HH'
                        {
                            current += 1;
                        }
                        break;

                    case 'J':
                        //obvious spanish, 'jose', 'san jacinto'
                        if (StringAt(workingString, current, strJOSE) || StringAt(workingString, 0, strSANsp))
                        {
                            if (((current == 0) && (workingString[current + 4] == ' ')) || StringAt(workingString, 0, strSANsp))
                            {
                                metaphoneData.Add(strH);
                            }
                            else
                            {
                                metaphoneData.Add(strJ, strH);
                            }
                            current += 1;
                            break;
                        }

                        if ((current == 0) && !StringAt(workingString, current, strJOSE))
                        {
                            metaphoneData.Add(strJ, strA); //Yankelovich/Jankelowicz
                        }
                        else
                            //spanish pron. of e.g. 'bajador'
                            if (current > 0
                                && IsVowel(workingString[current - 1])
                                && !isSlavoGermanic && ((workingString[current + 1] == charA)
                                || (workingString[current + 1] == charO)))
                        {
                            metaphoneData.Add(strJ, strH);
                        }
                        else if (current == last)
                        {
                            metaphoneData.Add(strJ, sp);
                        }
                        else if (!StringAt(workingString, (current + 1), strL, strT, strK, strS, strN, strM, strB, strZ)
                            && !StringAt(workingString, (current - 1), strS, strK, strL))
                        {
                            metaphoneData.Add(strJ);
                        }

                        if (workingString[current + 1] == charJ) //it could happen!
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charK:
                        if (workingString[current + 1] == charK)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strK);
                        break;

                    case charL:
                        if (workingString[current + 1] == charL)
                        {
                            //spanish e.g. 'cabrillo', 'gallegos'
                            if (((current == (input.Length - 3))
                                && StringAt(workingString, (current - 1), strILLO, strILLA, strALLE))
                                || ((StringAt(workingString, (last - 1), strAS, strOS)
                                || StringAt(workingString, last, strA, strO))
                                && StringAt(workingString, (current - 1), strALLE)))
                            {
                                metaphoneData.Add(strL, sp);
                                current += 2;
                                break;
                            }
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add("L");
                        break;

                    case charM:
                        if ((StringAt(workingString, (current - 1), strUMB)
                            && (((current + 1) == last)
                            || StringAt(workingString, (current + 2), strER))) //'dumb','thumb'
                            || (workingString[current + 1] == charM))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add("M");
                        break;

                    case charN:
                        if (workingString[current + 1] == charN)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strN);
                        break;

                    case charOdash:
                        current += 1;
                        metaphoneData.Add(strN);
                        break;

                    case charP:
                        if (workingString[current + 1] == charH)
                        {
                            metaphoneData.Add(strF);
                            current += 2;
                            break;
                        }

                        //also account for "campbell", "raspberry"
                        if (StringAt(workingString, (current + 1), strP, strB))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strP);
                        break;

                    case charQ:
                        if (workingString[current + 1] == charQ)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strK);
                        break;

                    case charR:
                        //french e.g. 'rogier', but exclude 'hochmeier'
                        if ((current == last) && !isSlavoGermanic
                            && StringAt(workingString, (current - 2), strIE)
                            && !StringAt(workingString, (current - 4), strME, strMA))
                        {
                            metaphoneData.Add(string.Empty, strR);
                        }
                        else
                        {
                            metaphoneData.Add(strR);
                        }

                        if (workingString[current + 1] == charR)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charS:
                        //special cases 'island', 'isle', 'carlisle', 'carlysle'
                        if (StringAt(workingString, (current - 1), strISL, strYSL))
                        {
                            current += 1;
                            break;
                        }

                        //special case 'sugar-'
                        if ((current == 0) && StringAt(workingString, current, strSUGAR))
                        {
                            metaphoneData.Add(strX, strS);
                            current += 1;
                            break;
                        }

                        if (StringAt(workingString, current, strSH))
                        {
                            //germanic
                            if (StringAt(workingString, (current + 1), strHEIM, strHOEK, strHOLM, strHOLZ))
                            {
                                metaphoneData.Add(strS);
                            }
                            else
                            {
                                metaphoneData.Add(strX);
                            }
                            current += 2;
                            break;
                        }

                        //italian & armenian
                        if (StringAt(workingString, current, strSIO, strSIA) || StringAt(workingString, current, strSIAN))
                        {
                            if (!isSlavoGermanic)
                            {
                                metaphoneData.Add(strS, strX);
                            }
                            else
                            {
                                metaphoneData.Add(strS);
                            }
                            current += 3;
                            break;
                        }

                        //german & anglicisations, e.g. 'smith' match 'schmidt', 'snider' match 'schneider'
                        //also, -sz- in slavic language altho in hungarian it is pronounced 's'
                        if (((current == 0)
                            && StringAt(workingString, (current + 1), strM, strN, strL, strW))
                            || StringAt(workingString, (current + 1), strZ))
                        {
                            metaphoneData.Add(strS, strX);
                            if (StringAt(workingString, (current + 1), strZ))
                            {
                                current += 2;
                            }
                            else
                            {
                                current += 1;
                            }
                            break;
                        }

                        if (StringAt(workingString, current, strSC))
                        {
                            //Schlesinger's rule
                            if (workingString[current + 2] == charH)
                            {
                                //dutch origin, e.g. 'school', 'schooner'
                                if (StringAt(workingString, (current + 3), strOO, strER, strEN, strUY, strED, strEM))
                                {
                                    //'schermerhorn', 'schenker'
                                    if (StringAt(workingString, (current + 3), strER, strEN))
                                    {
                                        metaphoneData.Add(strX, strSK);
                                    }
                                    else
                                    {
                                        metaphoneData.Add(strSK);
                                    }
                                    current += 3;
                                    break;
                                }
                                else
                                {
                                    if ((current == 0) && !IsVowel(workingString[3]) && (workingString[3] != charW))
                                    {
                                        metaphoneData.Add(strX, strS);
                                    }
                                    else
                                    {
                                        metaphoneData.Add(strX);
                                    }
                                    current += 3;
                                    break;
                                }
                            }

                            if (StringAt(workingString, (current + 2), strI, strE, strY))
                            {
                                metaphoneData.Add(strS);
                                current += 3;
                                break;
                            }
                            //else
                            metaphoneData.Add(strSK);
                            current += 3;
                            break;
                        }

                        //french e.g. 'resnais', 'artois'
                        if ((current == last) && StringAt(workingString, (current - 2), strAI, strOI))
                        {
                            metaphoneData.Add(string.Empty, strS);
                        }
                        else
                        {
                            metaphoneData.Add(strS);
                        }

                        if (StringAt(workingString, (current + 1), strS, strZ))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charT:
                        if (StringAt(workingString, current, strTION))
                        {
                            metaphoneData.Add(strX);
                            current += 3;
                            break;
                        }

                        if (StringAt(workingString, current, strTIA, strTCH))
                        {
                            metaphoneData.Add(strX);
                            current += 3;
                            break;
                        }

                        if (StringAt(workingString, current, strTH) || StringAt(workingString, current, strTTH))
                        {
                            //special case 'thomas', 'thames' or germanic
                            if (StringAt(workingString, (current + 2), strOM, strAM)
                                || StringAt(workingString, 0, strVANsp, strVONsp) || StringAt(workingString, 0, strSCH))
                            {
                                metaphoneData.Add(strT);
                            }
                            else
                            {
                                metaphoneData.Add(strO, strT);
                            }
                            current += 2;
                            break;
                        }

                        if (StringAt(workingString, (current + 1), strT, strD))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strT);
                        break;

                    case charV:
                        if (workingString[current + 1] == charV)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        metaphoneData.Add(strF);
                        break;

                    case charW:
                        //can also be in middle of word
                        if (StringAt(workingString, current, strWR))
                        {
                            metaphoneData.Add(strR);
                            current += 2;
                            break;
                        }

                        if ((current == 0) && (IsVowel(workingString[current + 1])
                            || StringAt(workingString, current, strWH)))
                        {
                            //Wasserman should match Vasserman
                            if (IsVowel(workingString[current + 1]))
                            {
                                metaphoneData.Add(strA, strF);
                            }
                            else
                            {
                                //need Uomo to match Womo
                                metaphoneData.Add(strA);
                            }
                        }

                        //Arnow should match Arnoff
                        if ((current == last && current > 0 && IsVowel(workingString[current - 1]))
                            || StringAt(workingString, (current - 1), strEWSKI, strEWSKY, strOWSKI, strOWSKY)
                            || StringAt(workingString, 0, strSCH))
                        {
                            metaphoneData.Add(string.Empty, strF);
                            current += 1;
                            break;
                        }

                        //polish e.g. 'filipowicz'
                        if (StringAt(workingString, current, strWICZ, strWITZ))
                        {
                            metaphoneData.Add(strTS, strFX);
                            current += 4;
                            break;
                        }

                        //else skip it
                        current += 1;
                        break;

                    case charX:
                        //french e.g. breaux
                        if (!((current == last)
                            && (StringAt(workingString, (current - 3), strIAU, strEAU)
                            || StringAt(workingString, (current - 2), strAU, strOU))))
                        {
                            metaphoneData.Add(strKS);
                        }

                        if (StringAt(workingString, (current + 1), strC, strX))
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    case charZ:
                        //chinese pinyin e.g. 'zhao'
                        if (workingString[current + 1] == charH)
                        {
                            metaphoneData.Add(strJ);
                            current += 2;
                            break;
                        }
                        else if (StringAt(workingString, (current + 1), strZO, strZI, strZA)
                            || (isSlavoGermanic && ((current > 0) && workingString[current - 1] != charT)))
                        {
                            metaphoneData.Add(strS, strTS);
                        }
                        else
                        {
                            metaphoneData.Add(strS);
                        }

                        if (workingString[current + 1] == charZ)
                        {
                            current += 2;
                        }
                        else
                        {
                            current += 1;
                        }
                        break;

                    default:
                        current += 1;
                        break;
                }
            }

            return metaphoneData.ToString();
        }


        static bool IsVowel(this char self)
        {
            return (self == charA) || (self == charE) || (self == charI)
                || (self == charO) || (self == charU) || (self == charY);
        }

        private const char charA = 'A';
        private const char charW = 'W';
        private const char charK = 'K';
        private const string strCZ = "CZ";
        private const string strWITZ = "WITZ";
        private const string strGN = "GN";
        private const string strKN = "KN";
        private const string strPN = "PN";
        private const string strWR = "WR";
        private const string strPS = "PS";
        private const char charX = 'X';
        private const string strS = "S";
        private const char charE = 'E';
        private const char charI = 'I';
        private const char charO = 'O';
        private const char charU = 'U';
        private const char charY = 'Y';
        private const char charB = 'B';
        private const char charAdash = 'Ã';
        private const string strACH = "ACH";
        private const string strBACHER = "BACHER";
        private const string strMACHER = "MACHER";
        private const string strK = "K";
        private const string strCAESAR = "CAESAR";
        private const string strCHIA = "CHIA";
        private const string strCH = "CH";
        private const string strCHAE = "CHAE";
        private const string strX = "X";
        private const string strHARAC = "HARAC";
        private const string strHARIS = "HARIS";
        private const string strHOR = "HOR";
        private const string strHYM = "HYM";
        private const string strHIA = "HIA";
        private const string strHEM = "HEM";
        private const string strCHORE = "CHORE";
        private const string strVANsp = "VAN ";
        private const string strVONsp = "VON ";
        private const string strSCH = "SCH";
        private const string strORCHES = "ORCHES";
        private const string strARCHIT = "ARCHIT";
        private const string strORCHID = "ORCHID";
        private const string strT = "T";
        private const string strA = "A";
        private const string strO = "O";
        private const string strU = "U";
        private const string strE = "E";
        private const string strL = "L";
        private const string strR = "R";
        private const string strN = "N";
        private const string strM = "M";
        private const string strB = "B";
        private const string strH = "H";
        private const string strF = "F";
        private const string strV = "V";
        private const string strW = "W";
        private const string sp = " ";
        private const string strMC = "MC";
        private const string strWICZ = "WICZ";
        private const string strCIA = "CIA";
        private const string strCC = "CC";
        private const char charM = 'M';
        private const string strI = "I";
        private const string strHU = "HU";
        private const string strUCCEE = "UCCEE";
        private const string strUCCES = "UCCES";
        private const string strKS = "KS";
        private const string strCK = "CK";
        private const string strCG = "CG";
        private const string strCQ = "CQ";
        private const string strCI = "CI";
        private const string strCE = "CE";
        private const string strCY = "CY";
        private const string strCIO = "CIO";
        private const string strCIE = "CIE";
        private const string strspC = " C";
        private const string strspQ = " Q";
        private const string strspG = " G";
        private const string strC = "C";
        private const string strQ = "Q";
        private const char charC = 'C';
        private const char charD = 'D';
        private const string strDG = "DG";
        private const string strY = "Y";
        private const string strJ = "J";
        private const string strTK = "TK";
        private const string strDT = "DT";
        private const string strDD = "DD";
        private const char charF = 'F';
        private const char charG = 'G';
        private const char charH = 'H';
        private const string strD = "D";
        private const string strG = "G";
        private const char charN = 'N';
        private const string strEY = "EY";
        private const string strLI = "LI";
        private const string strKL = "KL";
        private const string strES = "ES";
        private const string strEP = "EP";
        private const string strEB = "EB";
        private const string strEL = "EL";
        private const string strIB = "IB";
        private const string strIL = "IL";
        private const string strIN = "IN";
        private const string strIE = "IE";
        private const string strEI = "EI";
        private const string strER = "ER";
        private const string strDANGER = "DANGER";
        private const string strRANGER = "RANGER";
        private const string strMANGER = "MANGER";
        private const string strRGY = "RGY";
        private const string strOGY = "OGY";
        private const string strAGGI = "AGGI";
        private const string strOGGI = "OGGI";
        private const string strIERsp = "IER ";
        private const string strJOSE = "JOSE";
        private const string strSANsp = "SAN ";
        private const string strZ = "Z";
        private const char charJ = 'J';
        private const char charL = 'L';
        private const string strILLO = "ILLO";
        private const string strILLA = "ILLA";
        private const string strALLE = "ALLE";
        private const string strAS = "AS";
        private const string strOS = "OS";
        private const string strUMB = "UMB";
        private const char charOdash = 'Ð';
        private const char charP = 'P';
        private const string strP = "P";
        private const char charQ = 'Q';
        private const string strME = "ME";
        private const string strMA = "MA";
        private const char charR = 'R';
        private const char charS = 'S';
        private const string strISL = "ISL";
        private const string strYSL = "YSL";
        private const string strSUGAR = "SUGAR";
        private const string strSH = "SH";
        private const string strHEIM = "HEIM";
        private const string strHOEK = "HOEK";
        private const string strHOLM = "HOLM";
        private const string strHOLZ = "HOLZ";
        private const string strSIO = "SIO";
        private const string strSIA = "SIA";
        private const string strSIAN = "SIAN";
        private const string strSC = "SC";
        private const string strOO = "OO";
        private const string strEN = "EN";
        private const string strUY = "UY";
        private const string strED = "ED";
        private const string strEM = "EM";
        private const string strSK = "SK";
        private const string strAI = "AI";
        private const string strOI = "OI";
        private const string strTION = "TION";
        private const string strTIA = "TIA";
        private const string strTCH = "TCH";
        private const char charT = 'T';
        private const string strTH = "TH";
        private const string strTTH = "TTH";
        private const string strOM = "OM";
        private const string strAM = "AM";
        private const char charV = 'V';
        private const string strWH = "WH";
        private const string strEWSKI = "EWSKI";
        private const string strEWSKY = "EWSKY";
        private const string strOWSKI = "OWSKI";
        private const string strOWSKY = "OWSKY";
        private const string strFX = "FX";
        private const string strTS = "TS";
        private const string strEAU = "EAU";
        private const string strIAU = "IAU";
        private const string strAU = "AU";
        private const string strOU = "OU";
        private const char charZ = 'Z';
        private const string strZA = "ZA";
        private const string strZI = "ZI";
        private const string strZO = "ZO";
        private const string strET = "ET";


        static bool StartsWith(this string self, StringComparison comparison, params string[] strings)
        {
            foreach (string str in strings)
            {
                if (self.StartsWith(str, comparison))
                {
                    return true;
                }
            }
            return false;
        }

        static bool StringAt(this string self, int startIndex, params string[] strings)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            foreach (string str in strings)
            {
                if (self.IndexOf(str, startIndex, StringComparison.OrdinalIgnoreCase) >= startIndex)
                {
                    return true;
                }
            }
            return false;
        }


        private class MetaphoneData
        {
            readonly StringBuilder _primary = new StringBuilder(5);
            readonly StringBuilder _secondary = new StringBuilder(5);


            #region Properties

            internal bool Alternative { get; set; }
            internal int PrimaryLength
            {
                get
                {
                    return _primary.Length;
                }
            }

            internal int SecondaryLength
            {
                get
                {
                    return _secondary.Length;
                }
            }

            #endregion


            internal void Add(string main)
            {
                if (main != null)
                {
                    _primary.Append(main);
                    _secondary.Append(main);
                }
            }

            internal void Add(string main, string alternative)
            {
                if (main != null)
                {
                    _primary.Append(main);
                }

                if (alternative != null)
                {
                    Alternative = true;
                    if (alternative.Trim().Length > 0)
                    {
                        _secondary.Append(alternative);
                    }
                }
                else
                {
                    if (main != null && main.Trim().Length > 0)
                    {
                        _secondary.Append(main);
                    }
                }
            }

            public override string ToString()
            {
                string ret = (Alternative ? _secondary : _primary).ToString();
                //only give back 4 char metaph
                if (ret.Length > 4)
                {
                    ret = ret.Substring(0, 4);
                }

                return ret;
            }
        }
    }
}
