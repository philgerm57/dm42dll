using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DM42
{
    public class FormattingEdit
    {


        public static string[] FormatDoc(string doc, bool lineNumber =true, bool abbrev= true, string eol="\r\n")
        {

			int codeLineNo = 0;

			List<string> lText = new List<String>();

           string  lResult = Regex.Replace(doc, eol, "\r");
            string lLine = ""; 

			if (!Regex.IsMatch(doc, "([{}])"))
            {
				lResult = "{ }\r" + lResult; 
            }
					

            lText = lResult.Split('\r').ToList<string>(); 

			

            for ( int lIdx = 0; lIdx < lText.Count; lIdx++)
            {
                lLine = lText[lIdx];
                if (lLine.Trim() != "")
                {
					// 1. Remove line numbers

					if (Regex.Match(lLine, @"^\s*\d+\s+(@|#|//)(.*)").Success)
					{
						// ne rien faire

					}
					else
					{
						if (Regex.Match(lLine, @"(^\s*\d+)\s+(\d+)(.+)").Success)
						{
							lLine = Regex.Replace(lLine, @"(^\s*\d+)\s+(\d+)(.+)", "$2$3"); //01 33  -> 33
						}
						if (Regex.Match(lLine, @"(^\s*\d+)\s+(\w+)").Success)
						{
							lLine = Regex.Replace(lLine, @"(^\s*\d+)\s+(\w+)", "$2"); //01 ABC -> ABC
						}

						if (Regex.Match(lLine, @"(^\s*\d+)\s+(\W)").Success)
						{
							lLine = Regex.Replace(lLine, @"(^\s*\d+)\s+(\W)", "$2"); //-> +
						}
						if (Regex.Match(lLine, @"(^\s*\d+)\s+(\{.*)").Success)
						{
							lLine = Regex.Replace(lLine, @"(^\s*\d+)\s+(\{.*)", "$2"); //01 { 1 -> { 1
						}

						if (Regex.Match(lLine, @"(^\s*\d+)(▸|▶|>)(LBL.+)").Success)
						{
							lLine = Regex.Replace(lLine, @"(^\s*\d+)(▸|▶|>)(LBL.+)", "$3"); //01>LBL  -> LBL
						}
					}


                    //2 Remplacement  SYSTEMATIQUE 

                    lLine = Regex.Replace(lLine, "“", "\""); 
                    lLine = Regex.Replace(lLine, "”", "\"");
                    lLine = Regex.Replace(lLine, @"^\*", @"×");
                    lLine = Regex.Replace(lLine, @" ^x ", @"×");
                    lLine = Regex.Replace(lLine, @"–", @"'-");
                    lLine = Regex.Replace(lLine, @"\[LF\]", @"'␊");

					// 3. Remove abbreviation \Sigma, \GS, +/-, ...

					/* see https://www.swissmicros.com/dm42/decoder/ see 2. tab */

					if (abbrev)
					{
						lLine = Regex.Replace(lLine, @"(^\*)RCLx", "$1RCL×");
						lLine = Regex.Replace(lLine, @"(^\s*)RCL\*", "$1RCL×");
						lLine = Regex.Replace(lLine, @"(^\s*)RCL/", "$1RCL÷");

						lLine = Regex.Replace(lLine, @"(^\s*)STOx", "$1STO×");
						lLine = Regex.Replace(lLine, @"(^\s*)STO\*", "$1STO×");
						lLine = Regex.Replace(lLine, @"(^\s*)STO/", "$1STO÷");

						lLine = Regex.Replace(lLine, @"(^\s*)BASEx", "$1BASE×");
						lLine = Regex.Replace(lLine, @"(^\s*)BASE\*", "$1BASE×");
						lLine = Regex.Replace(lLine, @"(^\s*)BASE/", "$1BASE÷");
						lLine = Regex.Replace(lLine, @"(^\s*)BASE+\/-", @"$1BASE±");

						lLine = Regex.Replace(lLine, @"(^\s*)(R(\\|)\^)", "$1R↑");
						lLine = Regex.Replace(lLine, @"(^\s*)(R(\\|)v)\b", "$1R↓");

						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)(Sigma|SUM|GS)+", "$1Σ+");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)(Sigma|SUM|GS)-", "$1Σ-");
						lLine = Regex.Replace(lLine, @"(^\s*)ALL(\\|)(Sigma|SUM|GS)", "$1ALLΣ");
						lLine = Regex.Replace(lLine, @"(^\s*)CL(\\|)(Sigma|SUM|GS)", "$1CLΣ");
						lLine = Regex.Replace(lLine, @"(^\s*)LIN(\\|)(Sigma|SUM|GS)", "$1LINΣ");
						lLine = Regex.Replace(lLine, @"(^\s*)PR(\\|)(Sigma|SUM|GS)", "$1PRΣ");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)(Sigma|SUM|GS)REG", "$1ΣREG");

						lLine = Regex.Replace(lLine, @"(^\s *)(\\|)->HR", "$1→HR");
						lLine = Regex.Replace(lLine, @"(^\s *)(\\|)->HMS", "$1→HMS");

						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->DEC", "$1→DEC");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->OCT", "$1→OCT");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->RAD", "$1→RAD");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->HMS", "$1→HMS");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->REC", "$1→REC");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->DEG", "$1→DEG");
						lLine = Regex.Replace(lLine, @"(^\s*)(\\|)->POL", "$1→POL");

						lLine = Regex.Replace(lLine, @"(^\s*)X!=0?", "$1X≠0?");
						lLine = Regex.Replace(lLine, @"(^\s*)X\\=0?", "$1X≠0?");
						lLine = Regex.Replace(lLine, @"(^\s*)X#Y?", "$1X≠Y?");
						lLine = Regex.Replace(lLine, @"(^\s*)X#0?", "$1X≠0?");
						lLine = Regex.Replace(lLine, @"(^\s*)X<=0?", "$1X≤0?");
						lLine = Regex.Replace(lLine, @"(^\s*)X>=0?", "$1X≥0?");
						lLine = Regex.Replace(lLine, @"(^\s*)X\!=Y?", "$1X≠Y");
						lLine = Regex.Replace(lLine, @"(^\s*)X<=Y?", "$1X≤Y?");
						lLine = Regex.Replace(lLine, @"(^\s*)X>=Y?", "$1X≥Y?");

						lLine = Regex.Replace(lLine, @"(^\s*)10\^(x|X)", "10↑X");
						lLine = Regex.Replace(lLine, @"(^\s*)(x|X)\^2", "$1X↑2");
						lLine = Regex.Replace(lLine, @"(^\s*)Y\^(x|X)", "$1Y↑X");
						lLine = Regex.Replace(lLine, @"(^\s*)(e|E)\^(x|X)", "$1E↑X");
						lLine = Regex.Replace(lLine, @"(^\s*)(e|E)\^(x|X)-1", "$1E↑X-1");

						lLine = Regex.Replace(lLine, @"(^\s*)<-\b", "$1←");
						lLine = Regex.Replace(lLine, @"(^\s*)\^\b", "$1↑");
						lLine = Regex.Replace(lLine, @"(^\s*)v\b", "$1↓");
						lLine = Regex.Replace(lLine, @"(^\s*)->\b", "$1→");

						lLine = Regex.Replace(lLine, @"(^\s*)/$", "$1÷");

						lLine = Regex.Replace(lLine, @"(^\s*)\|-", "$1⊢");
						lLine = Regex.Replace(lLine, @"(^\s*)├", "$1⊢");//├

						lLine = Regex.Replace(lLine, @"(\d+)\s*(e E)(.*)", "$1ᴇ$3");
					}

					// 4. Reduce whitspace
					//   if (config.removeTooLongSpaces)
					//   {


					if (!Regex.IsMatch(lLine,@""".*"""))
					{
						// All without double quotes
						lLine = Regex.Replace(lLine, @"\s{2,}", " ");
					}
					else
					{
						// with double quotes ...
						// reduce whitespace from line beginning to first double quotes
						if (Regex.IsMatch(lLine,@"^(\w+)(\s+)"""))
						{
							lLine = Regex.Replace(lLine, @"^(\w+)(\s+)""", @"$1 """);
						}
					
					}

					if (Regex.IsMatch(lLine,@"\s+IND\s+"))
					{
						lLine = Regex.Replace(lLine, @"\s+IND\s+", " IND ");
					}

					if (Regex.IsMatch(lLine, @"\s+TO\s+"))
					{
						lLine = Regex.Replace(lLine, @"\s+TO\s+", " TO ");
					}


					if (Regex.IsMatch(lLine, "(LBL|GTO|XEQ) \"([A-J,a-e])\""))
					{
						lLine = Regex.Replace(lLine, "(LBL|GTO|XEQ) \"([A-J,a-e])\"", "$1 $2");
					}

					// 5. Trim
					lLine.Trim();


					// 6. Insert/Refresh line numbers, when using line numbers
					if (lineNumber)
					{
						// si pas une ligne de commentaire 

						if (!Regex.IsMatch(lLine, @"^\s*(@|#|//)"))
						{

							if (Regex.IsMatch(lLine, @"^{.*}"))
							{
								codeLineNo = 0;
							}
							else
							{
								codeLineNo++;
							}

							// prepend line number
							lLine = (codeLineNo < 10 ? "0" + codeLineNo.ToString() : codeLineNo.ToString()) + " " + lLine;

						}
					}

					lText[lIdx] = lLine;

				}
				
			} // for 

		

			return lText.ToArray(); 
        }

    }
}


//**************************************************************


