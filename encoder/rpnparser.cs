using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DM42.encoder
{
    public class RpnParser
    {
        int debug = 0; // debug level 0=nothing, 1=minimal, 2=verbose

        public List<RawProgram> programs= new List<RawProgram>();
        public int prgmLineNo = 0; // by parser auto incremented number
        public int prgmIndex = -1;
        List<string> document;
        bool useLineNumbers = false; 

        public void parse()
        {
            if (document.Count>0)
             {
                    int  docLineCount = document.Count;

                    RawProgram  program = null;

                    for (int  docLine = 0; docLine < docLineCount; docLine++)
                    {
                        string  line = document[docLine];

                            //{ ... }-line detected -> Prgm Start
                            Match  match =  Regex.Match(line,@"{.*}");
                        if (match.Success)
                        {
                            this.prgmIndex++;
                            program = new RawProgram(docLine);
                            this.programs.Add(program);
                        }

                        if (program!= null)
                        {
                            RawLine  rawLine = parseLine(docLine, line);

                            // no parser error ...
                            if (!rawLine.hasError() )
                            {
                                if (!rawLine.ignored)
                                {
                                    this.pushLine(rawLine);
                                }
                            }
                            else
                            {
                                // parse error
                                this.pushLine(rawLine);
                            }
                         }
                    }
             }
        }

        public RawLine parseLine(int docLine, string line)
        {
            string progErrorText = "";
            RawLine rawLine = new RawLine();

            //save original code

            if (this.ignoredLine(line))
            {
                rawLine.ignored = true;

                // reset line number when {} header line
                if (useLineNumbers)
                {
                    // premiere ligne indiquant la taille en octet du programme  { n-Byte Prgm } 
                    if (Regex.IsMatch(line, "^00 {.*}"))
                    {
                        this.prgmLineNo = 0;
                    }

                }
            }
            else
            {
                // increment
                this.prgmLineNo++;

                // read codeLineNo from code line
                if (useLineNumbers)
                {
                    Match match = Regex.Match(line, @"(^\d+)(▸|▶|>|s+)");
                    if (match.Success)
                    {
                        rawLine.codeLineNo = int.Parse(match.Value.Remove(match.Value.Length - 1, 1)); // enlever le dernier caractere

                    }
                }

                //#region prepare line
                rawLine.docCode = line; // ligne de code d'origine 
                rawLine.workCode = this.formatLine(line); // ligne de code a convertir !! probleme avec 123 # EZEEEZE 123 est pris comme un numero de ligne  

                if (Regex.IsMatch(rawLine.workCode, @"^\s*(⊢|)("".*"")"))
                {
                    // Is it a string "abc", ⊢"cde" ?
                    this.readString(ref rawLine);
                }
                else if (Regex.IsMatch(rawLine.workCode, @"^\s*-?\d+(\.\d+|)((ᴇ|e|E)-?\d{1,3}|)\s*$"))
                {
                    // Is it a number ?
                    this.readFloatNumber(ref rawLine);

                }
                else if (Encoder42.rpnMap.ContainsKey(rawLine.token))
                {
                    // Is it a rpn command ?
                    List<rawpattern> patterns = Encoder42.rpnMap[rawLine.token];

                    if (patterns.Count > 0)
                    {
                        bool matched = false;

                        for (int i = 0; i < patterns.Count; i++)
                        {
                            rawpattern pattern = patterns[i];

                            //Append whitespace + eol to all pattern.regex
                            // const regex = new RegExp(pattern.regex.source + /\s*$/.source);
                            Regex lRegex = new Regex(pattern.regex + @"\s*$");
                            //match ?
                            Match match = lRegex.Match(rawLine.workCode);
                            if (match.Success)
                            {
                                matched = true;
                                // Params included ?
                                this.checkParamsInMatch(pattern, ref rawLine, match);

                                rawLine.raw = pattern.raw;

                                break;
                            }
                        }

                        if (!matched)
                        {
                            progErrorText = "unvalid parameter";
                        }
                    }
                    else
                    {
                        progErrorText = "unvalid command";
                    }
                }
                else
                {
                    progErrorText = "unvalid command";
                }

                //#endregion

                //#region Checks ...

                if (useLineNumbers && this.prgmLineNo != rawLine.codeLineNo)
                {
                    progErrorText = "line number not correct: " + this.prgmLineNo + "!=" + rawLine.codeLineNo;
                }

                //#endregion
            }

            if (progErrorText != "")
            {
                rawLine.error = new CodeError(docLine, useLineNumbers ? this.prgmLineNo : -1, rawLine.docCode, progErrorText);
            }

            return rawLine;
        }

        public void SetDocument(List<string> rpnDoc)
        {
            document = new List<string>(rpnDoc); 
        }

        #region private 

        // Test si la ligne peut être ignorée 
        private bool ignoredLine(string line)
        {
            // ignore blank lines, length === 0
            // ignore { 33-Byte Prgm }...
            // ignore comments (#|//|@)
            line = line.Trim();
            bool ignored = line.Length == 0 || Regex.Match(line, "{.*}").Success || Regex.Match(line, @"^\s*(#|@|//)").Success;
            return ignored;
        }


        //#region Private Methods
        /** Read params from match like regex named group */
        private void checkParamsInMatch(rawpattern pattern, ref RawLine rawLine , Match match )
        {
            if (pattern.parames!="")
            {
                string[] lMatch = match.Value.Split(' ');
                string lM = ""; 

                string[] parames = pattern.parames.Split(','); // nombre de parametre 
                // assign params like regex named groups
                for (int  p = 0; p < parames.Length; p++) {
                    string paramet = parames[p];

                    int k = p + 1; 
                  
                   // MatchCollection val = Regex.Matches(match.Value, "\".*\"| [0-9A-F]{1,}|( [XYZLT]$)|( A|B|C|D|E|F|G|H|I|J|a|b|c|d|e)$");   // valeur numerique 

                    string lVal=match.Groups[k].Value; 


                    switch (paramet)
                    {
                        case "dig":
                            rawLine.parames.dig = lVal;
                            rawLine.parames.digno = int.Parse(lVal);
                            break;
                        case "flg":
                            rawLine.parames.flg = lVal; //val[p].Value;
                            rawLine.parames.flgno = int.Parse(lVal); //int.Parse(val[p].Value);
                            break;
                        case "key-1":
                            rawLine.parames.key = lVal; //val[p].Value;
                            rawLine.parames.keyno = int.Parse(lVal) - 1; //int.Parse(val[p].Value) - 1;
                            break;
                        case "key":
                            rawLine.parames.key = lVal;
                            rawLine.parames.keyno = int.Parse(lVal);
                            break;
                        case "lblno":
                            rawLine.parames.lblno = int.Parse(this.getLblNo(lVal.Trim())); //parseInt(this.getLblNo(match[k]));
                            break;
                        case "lbl":
                            //rawLine.parames.lbl = this.removeDoubleQuotes(Str.Value);
                            rawLine.parames.lbl = this.removeDoubleQuotes(lVal.Trim());
                            break;
                        case "nam":
                            //rawLine.parames.nam = this.removeDoubleQuotes(Str.Value);
                            rawLine.parames.nam = this.removeDoubleQuotes(lVal.Trim());
                            break;
                        case "reg":
                            rawLine.parames.reg = lVal;
                            rawLine.parames.regno = int.Parse(lVal);
                            break;
                        case "siz":
                            rawLine.parames.siz = lVal;
                            rawLine.parames.sizno = int.Parse(lVal);
                            break;
                        case "stk":
                            rawLine.parames.stk = lVal.Trim();
                            break;
                        case "ton":
                            rawLine.parames.ton = lVal;
                            rawLine.parames.tonno = int.Parse(lVal);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        /** Read a number */
        private void readFloatNumber(ref RawLine rawLine )
        {
            Match match = Regex.Match(rawLine.workCode,@"^\s*-?\d+(\.\d+|)((ᴇ|e|E)-?\d{1,3}|)\s*$");
            if (match.Success)
            {
                rawLine.parames.num = match.Value; // match[0];
                //rawLine.parames.numno =(int?) Decimal.Parse(match.Value); // or parseInt()
                rawLine.workCode = Regex.Replace(rawLine.workCode,@"^\s*-?\d+(\.\d+|)((ᴇ|e|E)-?\d{1,3}|)\s*$", "`num`");
            }
        }

        /** Read a string */
        private void readString(ref RawLine rawLine )
        {
            string  str = "";
            // only ⊢, not |- or ├
            Match  lmatch = Regex.Match(rawLine.workCode,@"^s*(⊢|)("".*"")");// let match = rawLine.workCode.match(/^\s*(⊢|)(".*")/);
            if (lmatch.Groups.Count>0)
            {
                
               str = this.removeDoubleQuotes(lmatch.Groups[2].Value);
              

             //   str = this.removeDoubleQuotes(lmatch[1].Value);//  str = this.removeDoubleQuotes(match[2]);
               rawLine.workCode = Regex.Replace(rawLine.workCode,"^s*(⊢|)\"(.*)\"", "$1`str`"); //rawLine.workCode.replace(/^\s*(⊢|)"(.*)"/, '$1`str`');
                if (lmatch.Groups[1].Value == "")
                {
                    rawLine.raw = "Fn"; //see EncoderFOCAL.rpnMap.get(`str`)[0];
                }
                else
                {
                    rawLine.raw = "Fn 7F"; //see EncoderFOCAL.rpnMap.get(⊢`str`)[0];
                }
            }
            // replace all occurences of focal character
            if (str!="" && Regex.IsMatch(str,@"(÷|×|√|∫|░|Σ|▶|π|¿|≤|\[LF\]|≥|≠|↵|↓|→|←|µ|μ|£|₤|°|Å|Ñ|Ä|∡|ᴇ|Æ|…|␛|Ö|Ü|▒|■|•|\\\\|↑)"))
            {

                foreach (KeyValuePair<string,int> cm in Encoder42.charCodeMap)
                {
                    if ( str != "")
                    {
                       str  = Regex.Replace(str, cm.Key,cm.Value.ToString()); 
                    }

                }
                   
            }
            rawLine.parames.str = str;
        }


        /** Converts lbl to number */
        private string getLblNo(string lbl) 
        {
             int entier=0;
            string  num= "";

            // Label A-J,a-e to number, or local number label
            if (lbl!="")
            {
                if (Regex.IsMatch(lbl, "(A|B|C|D|E|F|G|H|I|J)"))
                {
                    // A=65+37=102(0x66),B=66+37=103(0x67),...
                    entier = (int)lbl[0] + 37;
                    num = entier.ToString();
                } else if (Regex.IsMatch(lbl, "(a|b|c|d|e)"))
                {
                    // a=97+26=123(0x7B),b=98+26=124(0x7C),...
                    entier = (int)lbl[0] + 26; //CharCode
                    num = entier.ToString();
                } else
                {
                    num = lbl;
                 }
            }

            return num;
         }

          private void pushLine(RawLine rawLine ) {
            if (this.debug > 0)
            {
                //!!console.log(rawLine.workCode + ' -> ' + rawLine.raw);
            }

            this.programs[this.prgmIndex].rawLines.Add(rawLine);
        }

        /** Prepare line */
        private string formatLine(string line)
        {
            string lLine = line;
            //!! supprime le nombre entier devant un commentaire !!

            // Remove leading line numbers 01▸LBL "AA" or 07 SIN
            lLine = Regex.Replace(lLine, @"^\d+(▸|▶|>|\s+)", "");

            // ^\d + (▸|▶|>|\s+)/, ''

            // Comment //|@|#...
            Match match = Regex.Match(lLine, "\"");
            if (match.Success)
            {
                // lines with strings and comment supprime le commentaire ...

                //Match mm= Regex.Match(line, @"("".*"")\s*(//|@|#).*$");
                lLine = Regex.Replace(lLine, @"("".*"")\s*(//|@|#).*$", "$1");
            }
            else
            {
                // all other lines ...
                lLine = Regex.Replace(lLine, @"(//|@|#).*$", "");

                // Replace too long spaces (?<!".*)\s{2,} , but not in strings
                //let regex = new RegExp(/\s{2,}/, "g");
                lLine = Regex.Replace(lLine, @"\s{2,}", " ");
            }

            // Trim spaces
            lLine = lLine.Trim();

            return lLine;
        }

        /** Removes double quotes */
        private string removeDoubleQuotes(string str)
        {
            string lStr = str; 
            if (str!="")
            {
                // too simple
                // str = str.replace(/"/g, '');
                // cut start and end
                lStr = lStr.Substring(1, lStr.Length - 2);
            }
            return lStr;
        }

        /** Check in range min <= x <= max */
        private bool inRange(int x, int min,int  max) 
        {
            return (x - min) * (x - max) <= 0;
        }



        #endregion



    }
}
