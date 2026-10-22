using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace DM42.encoder
{
    public  static class Encoder42
    {

        public static Dictionary<string, List<rawpattern>> rpnMap = new Dictionary<string, List<rawpattern>>();
        public static Dictionary<string, int> stackMap = new Dictionary<string, int>();
        public static Dictionary<string, int> charCodeMap = new Dictionary<string, int>();

        static bool initialized  = false;

        public static void initialize()
        {
            if (!initialized)
            {
                
                // transform arr_rawMap -> rawMap
                foreach (KeyValuePair<string, List<rawpattern>> data in arr_rpnMap)
                {
                    rpnMap.Add(data.Key, data.Value);
                }

                
                foreach (KeyValuePair<string, int> data in arr_stackMap)
                {
                    stackMap.Add(data.Key, data.Value);
                };

                foreach (KeyValuePair<string, int> data in arr_charCodeMap)
                {
                    charCodeMap.Add(data.Key, data.Value);
                };

                initialized = true;

            }
        }

        /// <summary>
        ///  Formattage de la ligne RPN  pour mettre un seul espace entre mot sauf dans les chaines entre "
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string FormatRpnLine(string line)
        {
            string lLine =line.Trim(); 
            string lStr = Regex.Match(lLine, "\".*\"").Value; // recupere les chaines 
            string  lRpl = Regex.Replace(lLine, "\".*\"", "~"); // remplace tous le contenu de la chaine par le caractere
            lLine = Regex.Replace(lRpl, "[ ]{2,}", " "); // Remplace tous les espaces par un seul espace 
            lLine = lLine.Replace("~", lStr); // replace la chaine a sa place 
            return lLine;
        }

        public static void toRaw(RawLine rawLine , string languageId )
        {
             string progErrorText="";
             string languageIdFromCode = "";

            if (rawLine.workCode != "")
            {
                languageIdFromCode = getLanguageIdFromCode(rawLine, languageId);

                if (languageId != languageIdFromCode)
                {
                    progErrorText = "free42 command '" + rawLine.token + "' in hp42s program";
                }

                if (progErrorText == "")
                {
                    if (rawLine.tokenlengh == 1)
                    {
                        //#region 1 Token
                        if (rawLine.parames.str != "" && Regex.IsMatch(rawLine.workCode,"^(⊢|)`str`"))
                        {
                            // is it a string ...
                            // Fn = F0 + longueur sur 4 bits : 15 octets maximum après Fn.
                            // ⊢"..." occupe déjà un octet (7F) : 14 caractères maximum, sinon le raw est corrompu.
                            int lMaxLen = rawLine.workCode.StartsWith("⊢") ? 14 : 15;
                            if (rawLine.parames.str.Length > lMaxLen)
                                progErrorText = "string too long (" + rawLine.parames.str.Length + " > " + lMaxLen + " characters)";
                            else
                                rawLine.raw = insertStringInRaw(rawLine.raw, rawLine.parames.str);
                        } 
                        else if (rawLine.parames.num != "" && Regex.IsMatch(rawLine.workCode,"^`num`"))
                        {
                            // is it a simple number ...
                            rawLine.raw = convertNumberToRaw(rawLine.parames.num);
                        } else if (rawLine.token != "")
                        {
                            // is it a single "fixed" opcode ...
                        }

                        // Some error ...
                        if (rawLine.error == null && progErrorText != "")
                        {
                            rawLine.error = new CodeError(rawLine.docLine, rawLine.codeLineNo, rawLine.docCode, progErrorText);
                        }

                        //#endregion
                    }
                    else
                    {
                        //#region n Tokens

                        // is it a string ...
                        // 1. Insert Name
                        // KEY `key` GTO IND `nam`
                        // ASSIGN `nam` TO `key`
                        if (rawLine.parames.nam != null) {
                            rawLine.raw = insertStringInRaw(rawLine.raw, rawLine.parames.nam);
                        }

                        // is it a key ...
                        // 2. Insert key
                        // KEY `key` GTO IND `nam`
                        // ASSIGN `nam` TO `key`
                        if (rawLine.parames.keyno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.keyno);
                        }

                        // is it a global label ...
                        if (rawLine.parames.lbl != null) {
                            rawLine.raw = insertStringInRaw(rawLine.raw, rawLine.parames.lbl);
                        }

                        // is it a register ...
                        if (rawLine.parames.regno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.regno);
                        }

                        // is it a tone ...
                        if (rawLine.parames.tonno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.tonno);
                        }

                        // is it a local char label A-J,a-e coded as number ......
                        if (rawLine.parames.lblno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.lblno);
                        }

                        // flag
                        if (rawLine.parames.flgno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.flgno);
                        }

                        // 0-9 digits
                        if (rawLine.parames.digno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.digno);
                        }

                        // 10 or 11 digits
                        if (Regex.IsMatch(rawLine.workCode,"(ENG|FIX|SCI)(10|11)"))
                        {
                            // nothing to do ...
                        }

                        // size
                        if (rawLine.parames.sizno != null) {
                            rawLine.raw = insertNumberInRaw(rawLine.raw, rawLine.parames.sizno);
                        }

                        // is it a stack ...
                        if (rawLine.parames.stk != null) {
                             int lInt= stackMap[rawLine.parames.stk];
                            rawLine.raw = insertNumberInRaw(rawLine.raw, lInt);
                        }

                        if (rawLine.error == null && progErrorText != "")
                        {
                            rawLine.error = new CodeError(rawLine.docLine, rawLine.codeLineNo, rawLine.docCode, progErrorText);
                        }

                        //#endregion
                    }
                }
                else
                {
                    // wrong extension, free42 commands in hp42s file
                    if (rawLine.error == null)
                    {
                        rawLine.error = new CodeError(rawLine.docLine, rawLine.codeLineNo, rawLine.docCode, progErrorText);
                    }
                }
            }
        }

        /** Check if free42 command used */
        private static string getLanguageIdFromCode(RawLine rawLine , string languageId) 
        {
            string  lLanguageIdFromCode;
            if ( rawLine.token!="" &&  Regex.IsMatch(rawLine.token, @"(ACCEL|LOCAT|HEADING|ADATE|ATIME|ATIME24|CLK12|CLK24|DATE|DATE+|DDAYS|DMY|DOW|MDY|TIME|YMD|BSIGNED|BWRAP|BRESET|LSTO.+)"))
             
            {
              lLanguageIdFromCode = "free42";
            } else {
              lLanguageIdFromCode = languageId;
            }
            return lLanguageIdFromCode;
  }

        private static string insertNumberInRaw(string raw, int? num)
        {

            string lRaw = raw;
            if (lRaw != "" && num != null)
            {

                if (Regex.IsMatch(lRaw, "kk"))
                {
                    lRaw = Regex.Replace(lRaw, "kk", convertByteAsHex((int)num));
                    return lRaw;
                }

                if (Regex.IsMatch(raw, "rr"))
                {
                    lRaw = Regex.Replace(lRaw, "rr", convertByteAsHex((int)num));
                    return lRaw;
                }

                if (Regex.IsMatch(lRaw, "tn"))
                {
                    lRaw = Regex.Replace(lRaw, "tn", convertByteAsHex((int)num));
                    return lRaw;
                }
                if (Regex.IsMatch(lRaw, "nn"))
                {
                    lRaw = Regex.Replace(lRaw, "nn", convertByteAsHex((int)num));
                    return lRaw;
                }
                if (Regex.IsMatch(lRaw, "ll"))
                {
                    lRaw = Regex.Replace(lRaw, "ll", convertByteAsHex((int)num));
                    return lRaw;
                }
                if (Regex.IsMatch(lRaw, "ss ss")) // SIZE
                {
                    //raw = Regex.Replace(raw, "ss ss", Encoder42.convertByteAsHex(num));
                    string lsize = convertByteAsHex((int)num / 256) + ' ' + convertByteAsHex((int)num % 256);
                    lRaw = Regex.Replace(lRaw, "ss ss", lsize);
                    return lRaw;

                }

                if (Regex.IsMatch(lRaw, @"([\dA-F])l"))
                {
                    // not working: hex = hex.replace(/([\dA-F])l/, this.convertNumberToHexString(parseInt('0x' + '$1' + '0') + 1 + int));
                    Match lmatch = Regex.Match(lRaw, @"([\dA-F])l");
                    if (lmatch.Success)
                    {
                        //lRaw = Regex.Replace(lRaw, @"([\dA-F])l", convertByteAsHex(int.Parse(lmatch.Value[0].ToString()) + 1 +(int)num));
                        lRaw = Regex.Replace(lRaw, @"([\dA-F])l", lmatch.Value[0] +convertByteAsHex((int)num+1)[1].ToString());


                    }
                    return lRaw;
                }

                if (Regex.IsMatch(lRaw, @"(\d)r"))
                {
                    // not working: $1
                    Match lmatch = Regex.Match(lRaw, @"(\d)r");
                    lRaw = Regex.Replace(lRaw, @"(\d)r", convertByteAsHex(int.Parse(lmatch.Value[0].ToString()) * 16 + (int)num));
                    return lRaw;

                }

               

                if (Regex.IsMatch(lRaw, @"([\dA-F])t"))
                {
                    // stack, not working: $1
                    Match lmatch = Regex.Match(lRaw, @"([\dA-F])t");

                    lRaw = Regex.Replace(lRaw,@"([\dA-F])t",lmatch.Value[0]+  num.ToString());
                    return lRaw;

                }
               
            }

            return lRaw;
        }

        private static string insertStringInRaw(string raw, string  str) 
        {
            string lRaw = raw; 
           if (raw != "")
           {
              if (str != "")
              { 
                    int  len_str = str.Length;
                    int  pos_Fn = raw.IndexOf("Fn");

                    len_str = str.Length;

                    // str too long ? len > 14: max concat string length; 15: opcodes with Fn; 7: else
                 //   if (len_str > (Regex.Match(lRaw, "Fn 7F").Value ? 14 : Regex.IsMatch(lRaw, "Fn") ? 15 : 7))
                 //   {
                 //     //TODO: error !!
                 //   }

                    // loop each character in str and append hex to opcode
                    for (int lIdx = 0; lIdx < str.Length; lIdx++)
                    {
                        int lHexcode = Convert.ToChar(str[lIdx]);
                        //special char ?
                        if (Encoder42.charCodeMap.ContainsKey(str[lIdx].ToString()))
                        {
                            int lv = Encoder42.charCodeMap[str[lIdx].ToString()];
                            lHexcode = lv != 0 ? lv : 0;

                        }

                        lRaw += ' ' + convertByteAsHex(lHexcode);
                    }


                    // ASSIGN opcode search, replace aa
                    lRaw = Regex.Replace(lRaw, " aa(.*)", "$1 nn");

                    int  length_hex_after_Fn = (lRaw.Length - (pos_Fn + 2)) / 3;

                    //console.log(
                    //  HP42SEncoder.convertNumberToHexString(240 + length_hex_after_Fn)
                    //);

                    // concat three parts ...
                    lRaw = lRaw.Substring(0, pos_Fn) + // 1. part
                      convertByteAsHex(240 + length_hex_after_Fn) + // 2. part
                      lRaw.Substring(pos_Fn + 2); // 3. part
               } else
               {
                   lRaw = "";
               }
           }

         return lRaw;
        }

/** Changing numbers into corresponding opcodes.
  * "1.234E-455" -> 11 1A 12 13 14 1B 1C 14 15 15 00
  */
        private static string convertNumberToRaw(string num)
        {
            string lNum = num; 
            if (lNum != "")
            {
                // "1.234E-455" -> 11 1A 12 13 14 1B 1C 14 15 15 00
                lNum = Regex.Replace(lNum, @"(\d)", " 1$1"); // replace 1234E-455                 -> 11 .  12 13 14 E  -  14 15 15
                lNum = Regex.Replace(lNum, @"\."," 1A"); // replace .                             -> 11 1A 12 13 14 E  -  14 15 15
                lNum = Regex.Replace(lNum, @"(ᴇ|e|E)", " 1B"); // replace (ᴇ|e|E)                  -> 11 1A 12 13 14 1B -  14 15 15
                lNum = Regex.Replace(lNum,@"-", " 1C"); // replace - -> 1C                       -> 11 1A 12 13 14 1B 1C 14 15 15
                lNum = Regex.Replace(lNum,"^ ", "") + " 00";                             // remove first space + append 00   ->11 1A 12 13 14 1B 1C 14 15 15 00
            }

            return lNum;
        }
        
        private static string convertByteAsHex(int bytes) 
        {
            byte[] lArray = new byte[1];
            lArray[0] =(byte) bytes;
            string hex = BitConverter.ToString(lArray);// let hex = ('0' + (byte & 0xff).toString(16)).slice(-2).toUpperCase();
            return hex;
        }



    #region dictionnaires 

       
    private static Dictionary<string, int> arr_stackMap = new Dictionary<string, int>()
        {
                {   "T",0 },
                {   "Z",1 },
                {   "Y",2 },
                {   "X",3 },
                {   "L",4 }
        };

       
    private static Dictionary<string, int> arr_charCodeMap = new Dictionary<string, int>()
       {
         {  "÷",0 },
        {  "×",  1 },
        {  "√",  2 },
        {  "∫",  3 },
        {  "░",  4 },
        {  "Σ",  5 },
        {  "▶",  6 },
        {  "π",  7 },
        {  "¿",  8 },
        {  "≤",  9 },
        {  "\\[LF\\]",  10 }, // for [LF] line feed
        {  "␊",  10 }, // ␊ see: https://www.compart.com/de/unicode/U+240A
        {  "≥",  11 },
        {  "≠",  12 },
        {  "↵",  13 },
        {  "↓",  14 },
        {  "→",  15 },
        {  "←",  16 },
        {  "µ",  17 }, // different bytes B5
        {  "μ",  17 }, // different bytes N<
        {  "£",  18 },
        {  "₤",  18 },
        {  "°",  19 },
        {  "Å",  20 },
        {  "Ñ",  21 },
        {  "Ä",  22 },
        {  "∡",  23 },
        {  "ᴇ",  24 },
        {  "Æ",  25 },
        {  "…",  26 },
        {  "␛",  27 }, // ␛ see: https://www.compart.com/de/unicode/U+241B
        {  "Ö",  28 },
        {  "Ü",  29 },
        {  "▒",  30 },
        {  "■",  31 },
        {  "•",  31 }, // • -> ■
        // {  "SP",  32 },
        // {  "!",  33 },
        // {  """,  34 },
        // {  "#",  35 },
        // {  "$",  36 },
        // {  "%",  37 },
        // {  "&",  38 },
        // {  """,  39 }, // double quotes !!
        // {  "(",  40 },
        // {  ")",  41 },
        // {  "*",  42 },
        // {  "+",  43 },
        // {  ",",  44 },
        // {  "-",  45 },
        // {  ".",  46 },
        // {  "/",  47 },
        // {  "0",  48 },
        // {  "1",  49 },
        // {  "2",  50 },
        // {  "3",  51 },
        // {  "4",  52 },
        // {  "5",  53 },
        // {  "6",  54 },
        // {  "7",  55 },
        // {  "8",  56 },
        // {  "9",  57 },
        // {  ":",  58 },
        // {  ";",  59 },
        // {  "<",  60 },
        // {  "=",  61 },
        // {  ">",  62 },
        // {  "?",  63 },
        // {  "@",  64 },
        // {  "A",  65 },
        // {  "B",  66 },
        // {  "C",  67 },
        // {  "D",  68 },
        // {  "E",  69 },
        // {  "F",  70 },
        // {  "G",  71 },
        // {  "H",  72 },
        // {  "I",  73 },
        // {  "J",  74 },
        // {  "K",  75 },
        // {  "L",  76 },
        // {  "M",  77 },
        // {  "N",  78 },
        // {  "O",  79 },
        // {  "P",  80 },
        // {  "Q",  81 },
        // {  "R",  82 },
        // {  "S",  83 },
        // {  "T",  84 },
        // {  "U",  85 },
        // {  "V",  86 },
        // {  "W",  87 },
        // {  "X",  88 },
        // {  "Y",  89 },
        // {  "Z",  90 },
        // {  "[",  91 },
        {  "\\\\",  92 }, // for \
        // {  "]",  93 },
        {  "↑",  94 }
        // {  "_",  95 },
        // {  "`",  96 },
        // {  "´",  ??? },
        // {  "a",  97 },
        // {  "b",  98 },
        // {  "c",  99 },
        // {  "d",  100 },
        // {  "e",  101 },
        // {  "f",  102 },
        // {  "g",  103 },
        // {  "h",  104 },
        // {  "i",  105 },
        // {  "j",  106 },
        // {  "k",  107 },
        // {  "l",  108 },
        // {  "m",  109 },
        // {  "n",  110 },
        // {  "o",  111 },
        // {  "p",  112 },
        // {  "q",  113 },
        // {  "r",  114 },
        // {  "s",  115 },
        // {  "t",  116 },
        // {  "u",  117 },
        // {  "v",  118 },
        // {  "w",  119 },
        // {  "x",  120 },
        // {  "y",  121 },
        // {  "z",  122 },
        // {  "{",  123 },
        // {  "|",  124 },
        // {  "}",  125 },
        // {  "~",  126 }
        // {  "⊦",  127 }

        // {  "´",  ??? }
      };
    

        

    // 0(?<lblno>[2-9A-F]): LBL 01-15
    // Fn: F([1-9A-F]) Label: max. length 14
    // 7t: 7([0-4]); stack 0-4
    // 8r: ([89A-E][0-9A-F]); r: dec:1..99; 128 + r => hex:81..E3
    // Ft: F([0-4]); ... IND ST [XYZLT]
    // [23]r: (2[0-9A-F]); register dec:1-15, hex: 21-2F
    // rr: ([0-7][0-9A-F]); register dec:16-99, hex:10-63
    // ENG|FIX|SCI sd: digits 00-09
    // ENG|FIX|SCI 10|11: digits 10,11
    // LBL|GTO sl|ll: sl: 00-14, ll:15-99
    // RCL|STO sr|rr: sr 0-15, rr: 16-99
    // tone: dec:0-9; hex:00-09
    // nam: name max length 14
    // CharLabels: A=102(0x66), B=107(0x67), ..., J, a=123(0x7B), b=124(0x7C), ..., e


         
    private static Dictionary<string, List<rawpattern>> arr_rpnMap = new Dictionary<string, List<rawpattern>>()
     {
       
        
        {"%",   new List<rawpattern> { new rawpattern (  "%", "4C" ) }},
        {"%CH", new List<rawpattern> {new rawpattern(  "%CH",  "4D" ) }},
        {"+", new List<rawpattern> {new rawpattern(@"\+", "40") }},
        {"+/-", new List<rawpattern> {new rawpattern(@"\+/-", "54") }},
        {"-", new List<rawpattern> {new rawpattern("-", "41") }},
        {".END.", new List<rawpattern> {new rawpattern(".END.", "C0 00 0D") }},
        {"1/X", new List<rawpattern> {new rawpattern(@"1/X", "60") }},
        {"10^X", new List<rawpattern> {new rawpattern(@"10\^X", "57") }},
        {"10↑X", new List<rawpattern> {new rawpattern("10↑X", "57") }},
        {"ABS", new List<rawpattern> {new rawpattern("ABS", "61") }},
        {  "ACCEL",  new List<rawpattern> {new rawpattern( "ACCE",  "A7 CF")  } },
        {  "ACOS",   new List<rawpattern> {new rawpattern( "ACOS",  "5D")  } },
        {  "ACOSH",  new List<rawpattern> {new rawpattern(  "ACOSH",  "A0 66" ) } },
        {  "ADATE",  new List<rawpattern> {new rawpattern(  "ADATE",  "A6 81" ) } },
        {  "ADV",   new List<rawpattern> {new rawpattern( "ADV",  "8F")  } },
        {  "AGRAPH",  new List<rawpattern> {new rawpattern( "AGRAPH",  "A7 64" ) }},
        {  "AIP",  new List<rawpattern> {new rawpattern(  "AIP",  "A6 31" ) } },
        {  "ALENG",  new List<rawpattern> {new rawpattern(  "ALENG",  "A6 41")  } },
        {  "ALL",  new List<rawpattern> {new rawpattern( "ALL",  "A2 5D") } },
        {  "ALLΣ", new List<rawpattern> {new rawpattern(   "ALLΣ",  "A0 AE" ) } },
        {  "AND",  new List<rawpattern> {new rawpattern( "AND",  "A5 88") } },
        {  "AOFF", new List<rawpattern> {new rawpattern(  "AOFF",  "8B") } },
        {  "AON",  new List<rawpattern> {new rawpattern(  "AON",  "8C") } },
        
       
        {  "ARCL",new List<rawpattern>
             {
                { new rawpattern("ARCL IND ST ([XYZLT])",  "9B Ft", "stk") },
                { new rawpattern(@"ARCL IND ("".{1,14}"")",  "Fn BB", "nam") },
                { new rawpattern(@"ARCL IND (\d{2})",  "9B 8r", "reg") },
                { new rawpattern("ARCL ST ([XYZLT])",  "9B 7t", "stk") },
                { new rawpattern(@"ARCL ("".{1,14}"")" ,  "Fn B3", "nam") },
                { new rawpattern(@"ARCL (\d{2})",  "9B rr", "reg") }

             }
         },
        
        {  "AROT",new List<rawpattern> {new rawpattern( "AROT",  "A6 46") }},
        {  "ASHF",new List<rawpattern> {new rawpattern( "ASHF",  "88" ) } },
        {  "ASIN",new List<rawpattern> {new rawpattern( "ASIN",  "5C" ) }},
        {  "ASINH",new List<rawpattern> {new rawpattern( "ASINH",  "A0 64" )}},
        { "ASSIGN",new List<rawpattern> {new rawpattern(@"ASSIGN ("".{1,14}"") TO (0[1-9]|1[0-8])",  "Fn C0 aa",  "nam,key-1" )} },

        { "ASTO",new List<rawpattern>
          {
            { new rawpattern( "ASTO IND ST ([XYZLT])","9A Ft",  "stk") },
            { new rawpattern( @"ASTO IND ("".{1,14}"")","Fn BA",  "nam") },
            {new rawpattern(  @"ASTO IND (\d{2})", "9A 8r",  "reg") },
            { new rawpattern( "ASTO ST ([XYZLT])", "9A 7t",  "stk") },
            { new rawpattern(@"ASTO ("".{1,14}"")", "Fn B2",  "nam") },
            { new rawpattern(@"ASTO (\d{2})", "9A rr",  "reg") }
          }
        },


            {  "ATAN",new List<rawpattern> {new rawpattern("ATAN","5E") } },
            {  "ATANH",new List<rawpattern> {new rawpattern(   "ATANH",  "A0 65") } },
            {  "ATIME",new List<rawpattern> {new rawpattern(   "ATIME",  "A6 84") } },
            {  "ATIME24",new List<rawpattern> {new rawpattern(   "ATIME24",  "A6 85") } },
            {  "ATOX",new List<rawpattern> {new rawpattern(   "ATOX",  "A6 47") } },
            {  "AVIEW",new List<rawpattern> {new rawpattern(   "AVIEW",  "7E") } },
            {  "BASE+",new List<rawpattern> {new rawpattern(   @"BASE\+",  "A0 E6" )} },
            {  "BASE-",new List<rawpattern> {new rawpattern(  "BASE-",  "A0 E7") } },
            {  "BASE±",new List<rawpattern> {new rawpattern(   "BASE±",  "A0 EA") } },
            { "BASE+/-",new List<rawpattern> {new rawpattern(   @"BASE\+/-",  "A0 EA") } },
            {  "BASE×",new List<rawpattern> {new rawpattern(   "BASE×",  "A0 E8") } },
            {  "BASEx",new List<rawpattern> {new rawpattern(   "BASEx",  "A0 E8") } },
            {  "BASE÷",new List<rawpattern> {new rawpattern(  "BASE÷",  "A0 E9") } },
            {  "BASE/",new List<rawpattern> {new rawpattern(  "BASE/",  "A0 E9") } },
            {  "BEEP", new List<rawpattern> {new rawpattern(  "BEEP",  "86") } },
            {  "BEST",new List<rawpattern> {new rawpattern(   "BEST",  "A0 9F" )} },
            {  "BINM",new List<rawpattern> {new rawpattern(   "BINM",  "A0 E5") } },
            {  "BIT?",new List<rawpattern> {new rawpattern(   @"BIT\?",  "A5 8C") } },
            {  "BSIGNED",new List<rawpattern> {new rawpattern(   "BSIGNED",  "A7 D6") } },
            {  "BWRAP",new List<rawpattern> {new rawpattern(   "BWRAP",  "A7 D7") } },
            {  "BRESET",new List<rawpattern> {new rawpattern(  "BRESET",  "A7 D8") } },


            {  "CF",new List<rawpattern>

                {
                    {new rawpattern("CF IND ST ([XYZLT])",  "A9 Ft", "stk") },
                    {new rawpattern(@"CF IND ("".{1,14}"")",  "Fn A9",  "nam" )},
                    {new rawpattern(@"CF IND (\d{2})",  "A9 8r",  "reg" )},
                    { new rawpattern(@"CF (\d{2})",  "A9 rr",  "flg") }      //00-99
                 }
            }, 


            {  "CLA",new List<rawpattern> {new rawpattern( "CLA",  "87") } },
            {  "CLD",new List<rawpattern> {new rawpattern("CLD",  "7F") } },
            {  "CLK12",new List<rawpattern> {new rawpattern("CLK12",  "A6 86") } },
            {  "CLK24",new List<rawpattern> {new rawpattern("CLK24",  "A6 87") } },
            {  "CLLCD",new List<rawpattern> {new rawpattern("CLLCD",  "A7 63" )} },
            {  "CLMENU",new List<rawpattern> {new rawpattern("CLMENU",  "A2 6D") } },
             { "CLP",new List<rawpattern> {new rawpattern(@"CLP ("".{1,14}"")",  "Fn F0","lbl") } },
            {  "CLRG",new List<rawpattern> {new rawpattern("CLRG",  "8A") } },
            {  "CLST",new List<rawpattern> {new rawpattern("CLST",  "73") } },

            { "CLV",new List<rawpattern>

                {
                    {new rawpattern(  "CLV IND ST ([XYZLT])",  "F2 D8 Ft", "stk")  },
                    {new rawpattern( @"CLV IND ("".{1,14}"")",  "Fn B8",  "nam") },
                    {new rawpattern( @"CLV IND (\d{2})",  "F2 D8 8r",  "reg")  },
                    {new rawpattern( @"CLV ("".{1,14}"")",  "Fn B0",  "nam") }
                 }
            },

            {  "CLX",new List<rawpattern> {new rawpattern(  "CLX",  "77") }},
            {  "CLKEYS",new List<rawpattern> {new rawpattern( "CLKEYS",  "A2 62") } },
            {  "CLΣ",new List<rawpattern> {new rawpattern( "CLΣ",  "70" )} },
            {  "COMB",new List<rawpattern> {new rawpattern("COMB",  "A0 6F") } },
            {  "COMPLEX",new List<rawpattern> {new rawpattern( "COMPLEX",  "A0 72") } },
            {  "CORR",new List<rawpattern> {new rawpattern("CORR",  "A0 A7") } },
            {  "COS",new List<rawpattern> {new rawpattern("COS",  "5A") } },
            {  "COSH",new List<rawpattern> {new rawpattern("COSH",  "A0 62") } },
            {  "CPX?",new List<rawpattern> {new rawpattern( @"CPX\?",  "A2 67") } },
            {  "CPXRES",new List<rawpattern> {new rawpattern("CPXRES",  "A2 6A") } },
            {  "CROSS", new List<rawpattern> {new rawpattern("CROSS",  "A6 CA") } },
            {  "CUSTOM",new List<rawpattern> {new rawpattern("CUSTOM",  "A2 6F") } },
            {  "DATE",new List<rawpattern> {new rawpattern("DATE",  "A6 8C") } },
            {  "DATE+",new List<rawpattern> {new rawpattern("DATE+",  "A6 8D") } },
            {  "DDAYS",new List<rawpattern> {new rawpattern("DDAYS",  "A6 8E") } },
            {  "DECM",new List<rawpattern> {new rawpattern("DECM",  "A0 E3") } },
            {  "DEG",new List<rawpattern> {new rawpattern("DEG",  "80") } },
            {  "DELAY",new List<rawpattern> {new rawpattern("DELAY",  "A7 60") } },
            {  "DELR",new List<rawpattern> {new rawpattern("DELR",  "A0 AB" )} },
            {  "DET",new List<rawpattern> {new rawpattern("DET",  "A6 CC" )} },

            { "DIM",new List<rawpattern>

            {
                { new rawpattern( @"DIM IND ST ([XYZLT])",  "F2 EC Ft",  "stk") },
                { new rawpattern( @"DIM IND ("".{1,14}"")",  "Fn CC",  "nam") },
                { new rawpattern( @"DIM IND (\d{2})",  "F2 EC 8r",  "reg") },
                {new rawpattern(  @"DIM ("".{1,14}"")",  "Fn C4",  "nam") }
              }
            }, 

            {  "DIM?",new List<rawpattern> {new rawpattern(@"DIM\?",  "A6 E7") } },
            {  "DMY",new List<rawpattern> {new rawpattern( "DMY",  "A6 8F" )} },
            {  "DOT",new List<rawpattern> {new rawpattern( "DOT",  "A6 CB") } },
            {  "DOW",new List<rawpattern> {new rawpattern( "DOW",  "A6 90") } },

            {"DSE",new List<rawpattern>

                {
                    { new rawpattern( "DSE IND ST ([XYZLT])",  "97 Ft",  "stk") },
                    { new rawpattern( @"DSE IND ("".{1,14}"")",  "Fn 9F",  "nam" )},
                    { new rawpattern(  @"DSE IND (\d{2})",  "97 8r",  "reg") },
                    {  new rawpattern( @"DSE ST ([XYZLT])",  "97 7t",  "stk") },
                    { new rawpattern(  @"DSE ("".{1,14}"")",  "Fn 97",  "nam") },
                    {  new rawpattern( @"DSE (\d{2})",  "97 rr",  "reg" )}
                  }
            },
            {  "EDIT", new List<rawpattern> {new rawpattern("EDIT",  "A6 E1") } },

            { "EDITN",new List<rawpattern>

                {
                    { new rawpattern( "EDITN IND ST ([XYZLT])",  "F2 EF Ft",  "stk") },
                    {new rawpattern(  @"EDITN IND ("".{1,14}"")",  "Fn CE",  "nam" )},
                    {new rawpattern(  @"EDITN IND (\d{2})",  "F2 EF 8r",  "reg") },
                    {new rawpattern(  @"EDITN ("".{1,14}"")",  "Fn C6",  "nam") }
                }
            },

            {  "END",new List<rawpattern> {new rawpattern("END",  "C0 00 0D") } },

            { "ENG",new List<rawpattern>

                {
                    { new rawpattern(  "ENG 10",  "F1 D7") },
                    { new rawpattern(  "ENG 11",  "F1 E7") },
                    { new rawpattern(  "ENG IND ST ([XYZLT])",  "9E Ft",  "stk") },
                    {  new rawpattern( @"ENG IND ("".{1,14}"")",  "Fn DE",  "nam") },
                    {  new rawpattern( @"ENG IND (\d{2})",  "9E 8r",  "reg") },
                    {  new rawpattern( @"ENG (0[0-9])",  "9E nn",  "dig") } //00-09
                 }
            },

            {  "ENTER",new List<rawpattern> {new rawpattern( "ENTER",  "83") } },
            {  "EXITALL",new List<rawpattern> {new rawpattern( "EXITALL",  "A2 6C") } },
            {  "EXPF",new List<rawpattern> {new rawpattern( "EXPF",  "A0 A0") } },
            {  "E↑X",new List<rawpattern> {new rawpattern( "E↑X",  "55") } },
            {  "E^X",new List<rawpattern> {new rawpattern( @"E\^X",  "55") } },
            {  "E↑X-1",new List<rawpattern> {new rawpattern("E↑X-1",  "58") } },
            {  "E^X-1",new List<rawpattern> {new rawpattern(@"E\^X-1",  "58") } },

            {  "FC?",new List<rawpattern>
                {
                    { new rawpattern(  @"FC\? IND ST ([XYZLT])",  "AD Ft",  "stk" )},
                    { new rawpattern(  @"FC\? IND ("".{1,14}"")",  "Fn AD",  "nam") },
                    { new rawpattern(  @"FC\? IND (\d{2})",  "AD 8r",  "reg") },
                    {  new rawpattern( @"FC\? (\d{2})",  "AD rr",  "flg" )} //00-99
                 }
            }, 

            {"FC?C",new List<rawpattern>

                {
                    {new rawpattern( @"FC\?C IND ST ([XYZLT])",  "AB Ft",  "stk") },
                    {new rawpattern( @"FC\?C IND ("".{1,14}"")",  "Fn AB",  "nam" )},
                    {new rawpattern( @"FC\?C IND (\d{2})",  "AB 8r",  "reg") },
                    {new rawpattern( @"FC\?C (\d{2})",  "AB rr",  "flg" )} //00-99
                 }
            },

            {  "FCSTX",new List<rawpattern> {new rawpattern( "FCSTX",  "A0 A8") } },
            {  "FCSTY",new List<rawpattern> {new rawpattern( "FCSTY",  "A0 A9") } },

            {"FIX",new List<rawpattern>

                {
                    { new rawpattern( "FIX 10",  "F1 D5") },
                    {new rawpattern(  "FIX 11",  "F1 E5") },
                    { new rawpattern( "FIX IND ST ([XYZLT])",  "9C Ft",  "stk") },
                    {new rawpattern(  @"FIX IND ("".{1,14}"")",  "Fn DC",  "nam") },
                    {new rawpattern(  @"FIX IND (\d{2})",  "9C 8r",  "reg") },
                    {new rawpattern(  @"FIX (0[0-9])",  "9C nn",  "dig" )} //00-09
                }
            }, 

            {  "FNRM",new List<rawpattern> {new rawpattern(  "FNRM",  "A6 CF") } },
            {  "FP",new List<rawpattern> {new rawpattern( "FP",  "69") } },

            { "FS?",new List<rawpattern>
              {
                { new rawpattern(@"FS\? IND ST ([XYZLT])",  "AC Ft",  "stk") },
                { new rawpattern(@"FS\? IND ("".{1,14}"")",  "Fn AC",  "nam") },
                { new rawpattern(@"FS\? IND (\d{2})",  "AC 8r",  "reg") },
                { new rawpattern(@"FS\? (\d{2})",  "AC rr",  "flg" )} //00-99
              }
            },

            {"FS?C",new List<rawpattern>
                {
                    {new rawpattern( @"FS\?C IND ST ([XYZLT])",  "AA Ft",  "stk") },
                    {new rawpattern(  @"FS\?C IND ("".{1,14}"")",  "Fn AA",  "nam" )},
                    {new rawpattern(  @"FS\?C IND (\d{2})",  "AA 8r",  "reg") },
                    {new rawpattern(  @"FS\?C (\d{2})",  "AA rr",  "flg") } //00-99
                }

             }, 

            {  "GAMMA",new List<rawpattern> {new rawpattern("GAMMA",  "A0 74") } },
            {  "GETM",new List<rawpattern> { new rawpattern("GETM",  "A6 E8" )} },
            {  "GETKEY",new List<rawpattern> {new rawpattern( "GETKEY",  "A2 6E" )} },
            {  "GRAD", new List<rawpattern> {new rawpattern( "GRAD",  "82") } },
            {  "GROW", new List<rawpattern> {new rawpattern( "GROW",  "A6 E3" )} },

            { "GTO",new List<rawpattern>

         {
                {new rawpattern( "GTO IND ST ([XYZLT])",  "AE 7t",  "stk") },
                {new rawpattern(  @"GTO IND ("".{1,14}"")",  "Fn AE",  "nam") },
                {new rawpattern(  @"GTO IND (\d{2})",  "AE nn",  "reg") },
                {new rawpattern(  @"GTO ("".{1,14}"")",  "1D Fn",  "lbl") },
                {new rawpattern(  "GTO (1[5-9]|[2-9][0-9]|[a-eA-J])",  "D0 00 nn",  "lblno") }, // 15-99,A-J,a-e
                {new rawpattern(  "GTO (0[0-9]|1[0-4])",  "Bl 00",  "lblno" )} //00-14
             }
            },

            {  "HEADING",new List<rawpattern> {new rawpattern( "HEADING",  "A7 D1") } },
            {  "HEXM",new List<rawpattern> {new rawpattern("HEXM",  "A0 E2") } },
            {  "HMS+",new List<rawpattern> {new rawpattern(@"HMS\+",  "49") } },
            {  "HMS-",new List<rawpattern> {new rawpattern("HMS-",  "4A") } },
            {  "I+",new List<rawpattern> {new rawpattern(@"I\+",  "A6 D2") } },
            {  "I-",new List<rawpattern> {new rawpattern("I-",  "A6 D3") } },

            {  "INDEX",new List<rawpattern>
                {
                    {new rawpattern( "INDEX IND ST ([XYZLT])",  "F2 DA Ft",  "stk") },
                    { new rawpattern(@"INDEX IND ("".{1,14}"")",  "Fn 8F",  "nam" )},
                    {new rawpattern(@"INDEX IND (\d{2})",  "F2 DA 8r",  "reg") },
                    {new rawpattern(@"INDEX ("".{1,14}"")",  "Fn 87",  "nam") }
                }
            }, 

            { "INPUT",new List<rawpattern>

            {
                { new rawpattern( "INPUT IND ST ([XYZLT])",  "F2 EE Ft",  "stk") },
                {new rawpattern( @"INPUT IND ("".{1,14}"")",  "Fn CD",  "nam" )},
                {new rawpattern(@"INPUT IND (\d{2})",  "F2 EE 8r",  "reg") },
                { new rawpattern(@"INPUT ST ([XYZLT])",  "F2 D0 7t",  "stk") },
                { new rawpattern(@"INPUT ("".{1,14}"")",  "Fn C5",  "nam") },
                {new rawpattern(@"INPUT (\d{2})",  "F2 D0 rr",  "reg") }
              }
            },

            {  "INSR", new List<rawpattern> {new rawpattern( "INSR",  "A0 AA") } },


            { "INTEG",new List<rawpattern>

                {
                    { new rawpattern( "INTEG IND ST ([XYZLT])",  "F2 EA Ft",  "stk") },
                    {  new rawpattern( @"INTEG IND ("".{1,14}"")",  "Fn BE",  "nam" )},
                    { new rawpattern(  @"INTEG IND (\d{2})",  "F2 EA 8r",  "reg") },
                    {  new rawpattern( @"INTEG ("".{1,14}"")",  "Fn B6",  "lbl") }
                }
            }, 

            { "INVRT",new List<rawpattern>{new rawpattern("INVRT",  "A6 CE") } },
            { "IP", new List<rawpattern> { new rawpattern("IP",  "68") } },

            {"ISG",new List<rawpattern>

                {
                    {new rawpattern(  "ISG IND ST ([XYZLT])",  "96 Ft",  "stk") },
                    {new rawpattern(  @"ISG IND ("".{1,14}"")",  "Fn 9E",  "nam") },
                    {new rawpattern(  @"ISG IND (\d{2})",  "96 8r",  "reg") },
                    {new rawpattern(  "ISG ST ([XYZLT])",  "96 7t",  "stk") },
                    {new rawpattern(  @"ISG ("".{1,14}"")",  "Fn 96",  "nam") },
                    {new rawpattern(  @"ISG (\d{2})",  "96 rr",  "reg") }
                }
            },

            {  "J+",new List<rawpattern>{new rawpattern(@"J\+",  "A6 D4") } },
            {  "J-",new List<rawpattern>{new rawpattern("J-",  "A6 D5") } },

            { "KEY",new List<rawpattern>

                {
                    {new rawpattern("KEY ([1-9]) GTO IND ST ([XYZLT])",  "F3 E3 kk Ft",  "key,stk") }, // 1-9
                    {new rawpattern(@"KEY ([1-9]) GTO IND ("".{1,14}"")",  "Fn CB kk",  "key,nam") },
                    {new rawpattern(@"KEY ([1-9]) GTO IND (\d{2})",  "F3 E3 kk 8r",  "key,reg") },
                    {new rawpattern(@"KEY ([1-9]) GTO ("".{1,14}"")",  "Fn C3 kk",  "key,lbl") },
                    {new rawpattern(@"KEY ([1-9]) GTO (0[0-9]|[1-9][0-9]|[a-eA-J])",  "F3 E3 kk rr",  "key,lblno") }, // {  /KEY `key` GTO sl/,  "F3 E3 kk rr" },
                    {new rawpattern(@"KEY ([1-9]) XEQ IND ST ([XYZLT])",  "F3 E2 kk Ft",  "key,stk" )},
                    {new rawpattern(@"KEY ([1-9]) XEQ IND ("".{1,14}"")",  "Fn CA kk",  "key,nam" )},
                    {new rawpattern(@"KEY ([1-9]) XEQ IND (\d{2})",  "F3 E2 kk 8r",  "key,reg") },
                    {new rawpattern(@"KEY ([1-9]) XEQ ("".{1,14}"")",  "Fn C2 kk",  "key,lbl" )},
                    {new rawpattern(@"KEY ([1-9]) XEQ (0[0-9]|[1-9][0-9]|[a-eA-J])",  "F3 E2 kk rr",  "key,lblno") } //, {  /KEY `key` XEQ sl/,  "F3 E2 kk rr" }
                  }
            }, 

            {  "KEYASN",new List<rawpattern>{new rawpattern( "KEYASN",  "A2 63") } },
            {  "LASTX",new List<rawpattern>{new rawpattern( "LASTX",  "76") } },

            {"LBL",new List<rawpattern>
                {
                    {new rawpattern(@"LBL ("".{1,14}"")",  "C0 00 Fn 00",  "lbl") },
                    {new rawpattern(@"LBL (1[5-9]|[2-9][0-9]|[a-eA-J])",  "CF nn",  "lblno") }, // 15-99,A-J,a-e
                    {new rawpattern(@"LBL (0[0-9]|1[0-4])",  "0l",  "lblno") } // 00-14
                 }
            },

            {  "LCLBL",new List<rawpattern>{new rawpattern("LCLBL",  "A2 64") } },
            {  "LINF", new List<rawpattern>{new rawpattern("LINF",  "A0 A1" )} },
            {  "LINΣ", new List<rawpattern>{new rawpattern("LINΣ",  "A0 AD") } },
            {  "LN", new List<rawpattern>{new rawpattern("LN",  "50" )} },
            {  "LN1+X",new List<rawpattern>{new rawpattern(@"LN1\+X",  "65") } },
            {  "LOCAT",new List<rawpattern>{new rawpattern("LOCAT",  "A7 D0" )}},
            {  "LOG",new List<rawpattern>{new rawpattern( "LOG",  "56") } },
            {  "LOGF",new List<rawpattern>{new rawpattern("LOGF",  "A0 A2") } },
            {

         "LSTO",new List<rawpattern>
             {
                    {new rawpattern(@"LSTO IND ST ([XYZLT])",  "F2 ED Ft",  "stk")},
                    {new rawpattern(@"LSTO IND ("".{1,14}"")",  "Fn CF",  "nam" )},
                    {new rawpattern(@"LSTO IND (\d{2})",  "F2 ED 8r",  "reg") },
                    //{  /LSTO ST ([XYZLT])/,  "91 7t",  "stk" },
                    {new rawpattern(@"LSTO ("".{1,14}"")",  "Fn C7",  "nam" )}
                 //{  /LSTO (1[6-9]|[2-9][0-9])/,  "91 rr",  "reg" }, // 16-99
                 //{  /LSTO (0[0-9]|1[0-5])/,  "3r",  "reg" } // 00-15
             }
            },

            { "MAN",new List<rawpattern>{new rawpattern( "MAN",  "A7 5B" )} },
            { "MAT?",new List<rawpattern>{new rawpattern( @"MAT\?",  "A2 66") } },
            { "MDY", new List<rawpattern>{new rawpattern("MDY",  "A6 91" )} },
            { "MEAN",new List<rawpattern>{new rawpattern("MEAN",  "7C" )} },
            { "MENU", new List<rawpattern>{new rawpattern("MENU",  "A2 5E") } },
            { "MOD", new List<rawpattern>{new rawpattern("MOD",  "4B" )} },
            { "MVAR", new List<rawpattern>{new rawpattern(@"MVAR ("".{1,14}"")",  "Fn 90",  "nam") } },
            { "N!", new List<rawpattern>{new rawpattern( "N!",  "62") } },
            { "NEWMAT", new List<rawpattern>{new rawpattern("NEWMAT",  "A6 DA") } },
            { "NORM", new List<rawpattern>{new rawpattern( "NORM",  "A7 5C" )} },
            { "NOT", new List<rawpattern>{new rawpattern("NOT",  "A5 87") } },
            { "NULL", new List<rawpattern>{new rawpattern("NULL",  "00") } },
            { "OCTM", new List<rawpattern>{new rawpattern("OCTM",  "A0 E4" )} },
            { "OFF",new List<rawpattern>{new rawpattern("OFF",  "8D") } },
            { "OLD", new List<rawpattern>{new rawpattern("OLD",  "A6 DB") } },
            { "ON",new List<rawpattern>{new rawpattern("ON",  "A2 70") } },
            { "OR", new List<rawpattern>{new rawpattern("OR",  "A5 89" )} },
            { "PERM", new List<rawpattern>{new rawpattern("PERM",  "A0 70") } },
            {
         "PGMINT",new List<rawpattern>
             {
                    {new rawpattern(@"PGMINT IND ST ([XYZLT])",  "F2 E8 Ft",  "stk") },
                    { new rawpattern(@"PGMINT IND ("".{1,14}"")",  "Fn BC",  "nam") },
                    {new rawpattern(@"PGMINT IND (\d{2})",  "F2 E8 8r",  "reg") },
                    {new rawpattern(@"PGMINT ("".{1,14}"")",  "Fn B4",  "lbl") }
             }
           },

           { "PGMSLV",new List<rawpattern>
               {
                    {new rawpattern(@"PGMSLV IND ST ([XYZLT])",  "F2 E9 Ft",  "stk") },
                    {new rawpattern(@"PGMSLV IND ("".{1,14}"")",  "Fn BD",  "nam" )},
                    {new rawpattern(@"PGMSLV IND (\d{2})",  "F2 E9 8r",  "reg") },
                    {new rawpattern(@"PGMSLV ("".{1,14}"")",  "Fn B5",  "lbl") }
               }
            }, 

            {  "PI",  new List<rawpattern>{new rawpattern(  "PI",  "72" )} },
            {  "PIXEL",  new List<rawpattern>{new rawpattern(  "PIXEL",  "A7 65") } },
            {  "POLAR",  new List<rawpattern>{new rawpattern(  "POLAR",  "A2 59") } },
            {  "POSA",  new List<rawpattern>{new rawpattern(  "POSA",  "A6 5C") } },
            {  "PRA",  new List<rawpattern>{new rawpattern(  "PRA",  "A7 48") } },
            {  "PRLCD", new List<rawpattern>{new rawpattern( "PRLCD",  "A7 62") } },
            {  "PROFF",  new List<rawpattern>{new rawpattern( "PROFF",  "A7 5F") } },
            {  "PROMPT",  new List<rawpattern>{new rawpattern( "PROMPT",  "8E") } },
            {  "PRON",  new List<rawpattern>{new rawpattern(  "PRON",  "A7 5E" )} },
            {  "PRSTK",  new List<rawpattern>{new rawpattern( "PRSTK",  "A7 53") } },
            {  "PRUSR",  new List<rawpattern>{new rawpattern( "PRUSR",  "A7 61") } },

            {"PRV",new List<rawpattern>
                {
                    { new rawpattern(@"PRV IND ST ([XYZLT])",  "F2 D9 Ft",  "stk") },
                    { new rawpattern(@"PRV IND ("".{1,14}"")",  "Fn B9",  "nam" )},
                    {new rawpattern(@"PRV IND (\d{2})",  "F2 D9 8r",  "reg") },
                    {new rawpattern(@"PRV ("".{1,14}"")",  "Fn B1",  "nam") }
                 }
            },

            {  "PRX",  new List<rawpattern>{new rawpattern(  "PRX",  "A7 54") } },
            {  "PRΣ",  new List<rawpattern>{new rawpattern(  "PRΣ",  "A7 52") } },
            {  "PSE",  new List<rawpattern>{new rawpattern(  "PSE",  "89" )} },
            {  "PUTM",  new List<rawpattern>{new rawpattern(  "PUTM",  "A6 E9" )}},
            {  "PWRF",  new List<rawpattern>{new rawpattern(  "PWRF",  "A0 A3") } },
            {  "R<>R",  new List<rawpattern>{new rawpattern(  "R<>R",  "A6 D1") } },
            {  "RAD",  new List<rawpattern>{new rawpattern(  "RAD",  "81") } },
            {  "RAN",  new List<rawpattern>{new rawpattern(  "RAN",  "A0 71") } },

            {"RCL",new List<rawpattern>
                {
                    {new rawpattern(@"RCL IND ST ([XYZLT])",  "90 Ft",  "stk") },
                    {new rawpattern(@"RCL IND ("".{1,14}"")",  "Fn 99",  "nam" )},
                    {new rawpattern(@"RCL IND (\d{2})",  "90 8r",  "reg" )},
                    {new rawpattern(@"RCL ST ([XYZLT])",  "90 7t",  "stk" )},
                    {new rawpattern(@"RCL ("".{1,14}"")",  "Fn 91",  "nam" )},
                    {new rawpattern(@"RCL (1[6-9]|[2-9][0-9])",  "90 rr",  "reg") }, // 16-99
                    {new rawpattern(@"RCL (0[0-9]|1[0-5])",  "2r",  "reg") } // 00-15
                 }
            }, 

            {"RCL+",new List<rawpattern>
                 {
                    {new rawpattern(@"RCL\+ IND ST ([XYZLT])",  "F2 D1 Ft",  "stk") },
                    {new rawpattern(@"RCL\+ IND ("".{1,14}"")",  "Fn 9A",  "nam") },
                    {new rawpattern(@"RCL\+ IND (\d{2})",  "F2 D1 8r",  "reg" )},
                    {new rawpattern(@"RCL\+ ST ([XYZLT])",  "F2 D1 7t",  "stk") },
                    {new rawpattern(@"RCL\+ ("".{1,14}"")",  "Fn 92",  "nam") },
                    {new rawpattern(@"RCL\+ (\d{2})",  "F2 D1 rr",  "reg") }
                }
            },

            { "RCL-",new List<rawpattern>
                 {
                    {new rawpattern(@"RCL- IND ST ([XYZLT])",  "F2 D2 Ft",  "stk") },
                    {new rawpattern(@"RCL- IND ("".{1,14}"")",  "Fn 9B",  "nam") },
                    {new rawpattern(@"RCL- IND (\d{2})",  "F2 D2 8r",  "reg") },
                    {new rawpattern(@"RCL- ST ([XYZLT])",  "F2 D2 7t",  "stk") },
                    {new rawpattern(@"RCL- ("".{1,14}"")",  "Fn 93",  "nam") },
                    {new rawpattern(@"RCL- (\d{2})",  "F2 D2 rr",  "reg") }
                  }
            }, 

            {  "RCLEL",  new List<rawpattern> { new rawpattern("RCLEL",  "A6 D7") } },
            {  "RCLIJ", new List<rawpattern> { new rawpattern("RCLIJ",  "A6 D9") } },

            {"RCL×",new List<rawpattern>
             {
                {new rawpattern(@"RCL× IND ST ([XYZLT])",  "F2 D3 Ft",  "stk") },
                {new rawpattern(@"RCL× IND ("".{1,14}"")",  "Fn 9C",  "nam") },
                {new rawpattern(@"RCL× IND (\d{2})",  "F2 D3 8r",  "reg") },
                {new rawpattern(@"RCL× ST ([XYZLT])",  "F2 D3 7t",  "stk") },
                {new rawpattern(@"RCL× ("".{1,14}"")",  "Fn 94",  "nam") },
                {new rawpattern(@"RCL× (\d{2})",  "F2 D3 rr",  "reg") }
              }
            },

             {"RCLx",new List<rawpattern>
                 {
                    {new rawpattern(@"RCLx IND ST ([XYZLT])",  "F2 D3 Ft",  "stk") },
                    {new rawpattern(@"RCLx IND ("".{1,14}"")",  "Fn 9C",  "nam") },
                    {new rawpattern(@"RCLx IND (\d{2})",  "F2 D3 8r",  "reg") },
                    {new rawpattern(@"RCLx ST ([XYZLT])",  "F2 D3 7t",  "stk") },
                    {new rawpattern(@"RCLx ("".{1,14}"")",  "Fn 94",  "nam") },
                    {new rawpattern(@"RCLx (\d{2})",  "F2 D3 rr",  "reg") }
                  }
            },


            {"RCL÷",new List<rawpattern>
              {
                {new rawpattern(@"RCL÷ IND ST ([XYZLT])",  "F2 D4 Ft",  "stk") },
                {new rawpattern(@"RCL÷ IND ("".{1,14}"")",  "Fn 9D",  "nam") },
                {new rawpattern(@"RCL÷ IND (\d{2})",  "F2 D4 8r",  "reg") },
                {new rawpattern(@"RCL÷ ST ([XYZLT])",  "F2 D4 7t",  "stk") },
                {new rawpattern(@"RCL÷ ("".{1,14}"")",  "Fn 95",  "nam" )},
                {new rawpattern(@"RCL÷ (\d{2})",  "F2 D4 rr",  "reg") }
              }
            },

             {"RCL/",new List<rawpattern>
              {
                {new rawpattern(@"RCL/ IND ST ([XYZLT])",  "F2 D4 Ft",  "stk") },
                {new rawpattern(@"RCL/ IND ("".{1,14}"") ",  "Fn 9D",  "nam") },
                {new rawpattern(@"RCL/ IND (\d{2})",  "F2 D4 8r",  "reg") },
                {new rawpattern(@"RCL/ ST ([XYZLT])",  "F2 D4 7t",  "stk") },
                {new rawpattern(@"RCL/ ("".{1,14}"")",  "Fn 95",  "nam" )},
                {new rawpattern(@"RCL/ (\d{2})",  "F2 D4 rr",  "reg") }
              }
            },

            {  "RDX,",  new List<rawpattern> { new rawpattern(  "RDX,",  "A2 5C") } },
            {  "RDX.", new List<rawpattern> { new rawpattern(  "RDX.",  "A2 5B" )} },
            {  "REAL?",  new List<rawpattern> { new rawpattern(  @"REAL\?",  "A2 65") } },
            {  "REALRES", new List<rawpattern> { new rawpattern(  "REALRES",  "A2 6B") }},
            {  "RECT",  new List<rawpattern> { new rawpattern(  "RECT",  "A2 5A" )}},
            {  "RND",  new List<rawpattern> { new rawpattern(  "RND",  "6E") } },
            {  "RNRM",  new List<rawpattern> { new rawpattern(  "RNRM",  "A6 ED" )} },
            {  "ROTXY",  new List<rawpattern> { new rawpattern( "ROTXY",  "A5 8B") } },
            {  "RSUM", new List<rawpattern> { new rawpattern(  "RSUM",  "A6 D0" )} },
            {  "RTN", new List<rawpattern> { new rawpattern( "RTN",  "85") } },
            {  "R↑",  new List<rawpattern> { new rawpattern(  "R↑",  "74") } },
            {  "R^",  new List<rawpattern> { new rawpattern(  @"R\^",  "74") } },
            {  "R↓",  new List<rawpattern> { new rawpattern(  "R↓",  "75") } },
            {  "Rv",  new List<rawpattern> { new rawpattern(  "Rv",  "75") } },

            {"SCI",new List<rawpattern>

                {
                    {new rawpattern(@"SCI 10",  "F1 D6") },
                    {new rawpattern(@"SCI 11",  "F1 E6") },
                    {new rawpattern(@"SCI IND ST ([XYZLT])",  "9D Ft",  "stk") },
                    {new rawpattern(@"SCI IND ("".{1,14}"")",  "Fn DD",  "nam") },
                    {new rawpattern(@"SCI IND (\d{2})",  "9D 8r",  "reg" )},
                    {new rawpattern(@"SCI (0[0-9])",  "9D nn",  "dig" )} //00-09
                }
            },

            {  "SDEV", new List<rawpattern> { new rawpattern("SDEV",  "7D") }},
            {  "SEED", new List<rawpattern> { new rawpattern("SEED",  "A0 73" )}},

            {"SF",new List<rawpattern>
               {
                    { new rawpattern(@"SF IND ST ([XYZLT])",  "A8 Ft",  "stk") },
                    {new rawpattern(@"SF IND ("".{1,14}"")",  "Fn A8",  "nam") },
                    { new rawpattern(@"SF IND (\d{2})",  "A8 8r",  "reg" )},
                    { new rawpattern(@"SF (\d{2})",  "A8 rr",  "flg" )} //00-99
               }
            }, 

            {"SIGN",  new List<rawpattern> { new rawpattern("SIGN",  "7A") } },
            {"SIN",  new List<rawpattern> { new rawpattern( "SIN",  "59") } },
            {"SINH",  new List<rawpattern> { new rawpattern("SINH",  "A0 61" )} },
            {"SIZE",  new List<rawpattern> { new rawpattern( @"SIZE (\d{2,4})",  "F3 F7 ss ss",  "siz") } },
            {"SLOPE",  new List<rawpattern> { new rawpattern( "SLOPE",  "A0 A4" )} },

            { "SOLVE",new List<rawpattern>

                 {
                    { new rawpattern(@"SOLVE IND ST ([XYZLT])",  "F2 EB Ft",  "stk") },
                    { new rawpattern(@"SOLVE IND ("".{1,14}"")",  "Fn BF",  "nam" )},
                    { new rawpattern(@"SOLVE IND (\d{2})",  "F2 EB 8r",  "reg") },
                    { new rawpattern(@"SOLVE ("".{1,14}"")",  "Fn B7",  "lbl" )}
                 }
            }, 

            { "SQRT", new List<rawpattern> { new rawpattern(  "SQRT",  "52") } },


            { "STO",new List<rawpattern>
                {
                    { new rawpattern(@"STO IND ST ([XYZLT])",  "91 Ft",  "stk") },
                    { new rawpattern(@"STO IND ("".{1,14}"")",  "Fn 89",  "nam" )},
                    { new rawpattern(@"STO IND (\d{2})",  "91 8r",  "reg") },
                    { new rawpattern(@"STO ST ([XYZLT])",  "91 7t",  "stk" )},
                    {new rawpattern(@"STO ("".{1,14}"")",  "Fn 81",  "nam" )},
                    {new rawpattern(@"STO (1[6-9]|[2-9][0-9])",  "91 rr",  "reg") }, // 16-99
                    { new rawpattern(@"STO (0[0-9]|1[0-5])",  "3r",  "reg") } // 00-15
                  }
            },

            { "STO+",new List<rawpattern>

            {
                { new rawpattern(@"STO\+ IND ST ([XYZLT])",  "92 Ft",  "stk") },
                { new rawpattern(@"STO\+ IND ("".{1,14}"")",  "Fn 8A",  "nam") },
                { new rawpattern(@"STO\+ IND (\d{2})",  "92 8r",  "reg") },
                { new rawpattern(@"STO\+ ST ([XYZLT])",  "92 7t",  "stk" )},
                { new rawpattern(@"STO\+ ("".{1,14}"")",  "Fn 82",  "nam") },
                { new rawpattern(@"STO\+ (\d{2})",  "92 rr",  "reg") }
             }
            }, 

            {"STO-",new List<rawpattern>

            {
                { new rawpattern(@"STO- IND ST ([XYZLT])",  "93 Ft",  "stk") },
                { new rawpattern(@"STO- IND ("".{1,14}"")",  "Fn 8B",  "nam") },
                {new rawpattern(@"STO- IND (\d{2})",  "93 8r",  "reg") },
                { new rawpattern(@"STO- ST ([XYZLT])",  "93 7t",  "stk") },
                { new rawpattern(@"STO- ("".{1,14}"")",  "Fn 83",  "nam") },
                { new rawpattern(@"STO- (\d{2})",  "93 rr",  "reg") }
             }
            }, 

            {  "STOEL", new List<rawpattern> { new rawpattern("STOEL",  "A6 D6") } },
            {  "STOIJ",  new List<rawpattern> { new rawpattern("STOIJ",  "A6 D8") } },
            {  "STOP", new List<rawpattern> { new rawpattern( "STOP",  "84") } },

            {"STO×",new List<rawpattern>

                {
                    { new rawpattern(@"STO× IND ST ([XYZLT])",  "94 Ft",  "stk") },
                    { new rawpattern(@"STO× IND ("".{1,14}"")",  "Fn 8C",  "nam") },
                    { new rawpattern(@"STO× IND (\d{2})",  "94 8r",  "reg") },
                    { new rawpattern(@"STO× ST ([XYZLT])",  "94 7t",  "stk") },
                    { new rawpattern(@"STO× ("".{1,14}"")",  "Fn 84",  "nam") },
                    { new rawpattern(@"STO× (\d{2})",  "94 rr",  "reg" )}
                 }
            },

             {"STOx",new List<rawpattern>

                {
                    { new rawpattern(@"STOx IND ST ([XYZLT])",  "94 Ft",  "stk") },
                    { new rawpattern(@"STOx IND ("".{1,14}"")",  "Fn 8C",  "nam") },
                    { new rawpattern(@"STOx IND (\d{2})",  "94 8r",  "reg") },
                    { new rawpattern(@"STOx ST ([XYZLT])",  "94 7t",  "stk") },
                    { new rawpattern(@"STOx ("".{1,14}"")",  "Fn 84",  "nam") },
                    { new rawpattern(@"STOx (\d{2})",  "94 rr",  "reg" )}
                 }
            },

            { "STO÷",new List<rawpattern>

                 {
                    { new rawpattern(@"STO÷ IND ST ([XYZLT])",  "95 Ft",  "stk") },
                    { new rawpattern(@"STO÷ IND ("".{1,14}"")",  "Fn 8D",  "nam") },
                    { new rawpattern(@"STO÷ IND (\d{2})",  "95 8r",  "reg") },
                    { new rawpattern(@"STO÷ ST ([XYZLT])",  "95 7t",  "stk") },
                    { new rawpattern(@"STO÷ ("".{1,14}"")",  "Fn 85",  "nam") },
                    { new rawpattern(@"STO÷ (\d{2})",  "95 rr",  "reg") }
                  }
            },

             { "STO/",new List<rawpattern>

                 {
                    { new rawpattern(@"STO/ IND ST ([XYZLT])",  "95 Ft",  "stk") },
                    { new rawpattern(@"STO/ IND ("".{1,14}"")",  "Fn 8D",  "nam") },
                    { new rawpattern(@"STO/ IND (\d{2})",  "95 8r",  "reg") },
                    { new rawpattern(@"STO/ ST ([XYZLT])",  "95 7t",  "stk") },
                    { new rawpattern(@"STO/ ("".{1,14}"")",  "Fn 85",  "nam") },
                    { new rawpattern(@"STO/ (\d{2})",  "95 rr",  "reg") }
                  }
            },


            {  "STR?",  new List<rawpattern> { new rawpattern(@"STR\?",  "A2 68") } },
            {  "SUM",  new List<rawpattern> { new rawpattern("SUM",  "A0 A5") } },
            {  "TAN", new List<rawpattern> { new rawpattern("TAN",  "5B") } },
            {  "TANH",  new List<rawpattern> { new rawpattern("TANH",  "A0 63" )} },
            {  "TIME",  new List<rawpattern> { new rawpattern("TIME",  "A6 9C" )} },

            {"TONE",new List<rawpattern>
                {
                    { new rawpattern(@"TONE IND ST ([XYZLT])",  "9F Ft",  "stk") },
                    { new rawpattern(@"TONE IND ("".{1,14}"")",  "Fn DF",  "nam") },
                    { new rawpattern(@"TONE IND (\d{2})",  "9F 8r",  "reg") },
                    {new rawpattern(@"TONE ([0-9])",  "9F tn",  "ton" )} // 0-9
                }
            }, 
            {  "TRACE",  new List<rawpattern> { new rawpattern( "TRACE",  "A7 5D") }},
            {  "TRANS",  new List<rawpattern> { new rawpattern("TRANS",  "A6 C9") }},
            {  "UVEC",  new List<rawpattern> { new rawpattern("UVEC",  "A6 CD" )}},

            { "VARMENU",new List<rawpattern>
             {
                { new rawpattern(@"VARMENU IND ST ([XYZLT])",  "F2 F8 Ft",  "stk")},
                { new rawpattern(@"VARMENU IND ("".{1,14}"")",  "Fn C9",  "nam" )},
                { new rawpattern(@"VARMENU IND (\d{2})",  "F2 F8 8r",  "reg" )},
                { new rawpattern(@"VARMENU ("".{1,14}"")",  "Fn C1",  "nam") }
             }
            }, 

            {"VIEW",new List<rawpattern>

                {
                    { new rawpattern(@"VIEW IND ST ([XYZLT])",  "98 Ft",  "stk") },
                    { new rawpattern(@"VIEW IND ("".{1,14}"")",  "Fn 88",  "nam") },
                    { new rawpattern(@"VIEW IND (\d{2})",  "98 8r",  "reg" )},
                    { new rawpattern(@"VIEW ST ([XYZLT])",  "98 7t",  "stk" )},
                    { new rawpattern(@"VIEW ("".{1,14}"")",  "Fn 80",  "nam" )},
                    { new rawpattern(@"VIEW (\d{2})",  "98 rr",  "reg" )}
                }
            },
            {  "WMEAN", new List<rawpattern> { new rawpattern( "WMEAN",  "A0 AC") } },
            {  "WRAP",  new List<rawpattern> { new rawpattern(  "WRAP",  "A6 E2" )} },
            {  "WSIZE", new List<rawpattern> { new rawpattern(  "WSIZE",  "A7 D3") } },
            {  "WSIZE?", new List<rawpattern> { new rawpattern( @"WSIZE\?",  "A7 D4") } },
           

            { "X<>",new List<rawpattern>
                {
                    { new rawpattern(@"X<> IND ST ([XYZLT])",  "CE Ft",  "stk") },
                    { new rawpattern(@"X<> IND ("".{1,14}"")",  "Fn 8E",  "nam") },
                    {  new rawpattern(@"X<> IND (\d{2})",  "CE 8r",  "reg") },
                    { new rawpattern(@"X<> ST ([XYZLT])",  "CE 7t",  "stk" )},
                    { new rawpattern(@"X<> ("".{1,14}"")",  "Fn 86",  "nam") },
                    { new rawpattern(@"X<> (\d{2})",  "CE rr",  "reg") }
                 }
            }, 
            {  "X<>Y",  new List<rawpattern> { new rawpattern( "X<>Y",  "71") } },
            {  "X<Y?", new List<rawpattern> { new rawpattern(  @"X<Y\?",  "44" )} },
            {  "X=0?",  new List<rawpattern> { new rawpattern( @"X=0\?",  "67" )} },
            {  "X=Y?",  new List<rawpattern> { new rawpattern( @"X=Y\?",  "78" )} },
            {  "X>0?", new List<rawpattern> { new rawpattern( @"X>0\?",  "64" )} },
            {  "X<0?",  new List<rawpattern> { new rawpattern(  @"X<0\?",  "66" )}},
            {  "X>Y?",new List<rawpattern> { new rawpattern(  @"X>Y\?",  "45" )} },

            { "XEQ",new List<rawpattern>
                {
                    { new rawpattern(@"XEQ IND ST ([XYZLT])",  "AE Ft",  "stk") },
                    {  new rawpattern(@"XEQ IND ("".{1,14}"")",  "Fn AF",  "nam" )},
                    {  new rawpattern(@"XEQ IND (\d{2})",  "AE 8r",  "reg" )},
                    {  new rawpattern(@"XEQ ("".{1,14}"")",  "1E Fn",  "lbl" )},
                    {   new rawpattern(@"XEQ (\d{2}|[a-eA-J])",  "E0 00 nn",  "lblno" )} // 00-99,A-J,a-e {  /XEQ sl/,  "E0 00 nn" }
                }
            } ,

            {  "XOR", new List<rawpattern> { new rawpattern( "XOR",  "A5 8A")} },
            {  "XTOA", new List<rawpattern> { new rawpattern( "XTOA",  "A6 6F" )} },
            {  "YMD", new List<rawpattern> { new rawpattern( "YMD",  "A7 D5" )} },
            {  "X↑2", new List<rawpattern> { new rawpattern( "X↑2",  "51" )} },
            {  "X^2", new List<rawpattern> { new rawpattern( @"X\^2",  "51" )} },
            {  "X≠0?", new List<rawpattern> { new rawpattern( @"X≠0\?",  "63" )} },
            { "X!=0?",  new List<rawpattern> { new rawpattern(@"X!=0\?",  "63" )} },
            { "X<>0?",  new List<rawpattern> { new rawpattern(@"X<>0\?",  "63" )} },
            {  "X≠Y?", new List<rawpattern> { new rawpattern( @"X≠Y\?",  "79" )} },
            { "X!=Y?",  new List<rawpattern> { new rawpattern(@"X!=Y\?",  "79" )} },
            {  "X≤0?", new List<rawpattern> { new rawpattern( @"X≤0\?",  "7B" )} },
            {  "X<=0?", new List<rawpattern> { new rawpattern( @"X<=0\?",  "7B" )} },
            {  "X≤Y?", new List<rawpattern> { new rawpattern( @"X≤Y\?",  "46" )} },
            {  "X<=Y?", new List<rawpattern> { new rawpattern( @"X<=Y\?",  "46" )} },
            {  "X≥0?", new List<rawpattern> { new rawpattern( @"X≥0\?",  "A2 5F" )} },
            { "X>=0?",  new List<rawpattern> { new rawpattern(  @"X>=0\?","A2 5F" )} },
            {  "X≥Y?", new List<rawpattern> { new rawpattern( @"X≥Y\?",  "A2 60" )} },
            {  "X>=Y?", new List<rawpattern> { new rawpattern( @"X>=Y\?",  "A2 60" )} },
            {  "X≶Y", new List<rawpattern> { new rawpattern(  "X≶Y",  "71" )} },
            {  "X<=Y", new List<rawpattern> { new rawpattern(  "X<=Y",  "71" )} },
            {  "YINT", new List<rawpattern> { new rawpattern( "YINT",  "A0 A6" )} },
            {  "Y↑X", new List<rawpattern> { new rawpattern( "Y↑X",  "53" )} },
            {  "Y^X", new List<rawpattern> { new rawpattern( @"Y\^X",  "53" )} },
            {  "[FIND]", new List<rawpattern> { new rawpattern(@"\[FIND\]",  "A6 EC" )} },
            {  "[MAX]", new List<rawpattern> { new rawpattern(  @"\[MAX\]",  "A6 EB" )} },
            {  "[MIN]", new List<rawpattern> { new rawpattern(  @"\[MIN\]",  "A6 EA" )} },
            {  "`str`", new List<rawpattern> { new rawpattern( @"("".{15}"")" ,  "Fn",  "str") }}, // max length 15 !
            {  "±", new List<rawpattern> { new rawpattern(  "±",  "54") } },
            {  "×", new List<rawpattern> { new rawpattern(  "×",  "42" )} },
            {  "x", new List<rawpattern> { new rawpattern(  "x",  "42" )} },
            {  "÷", new List<rawpattern> { new rawpattern(  "÷",  "43" )} },
            {  "/", new List<rawpattern> { new rawpattern(  @"\/",  "54") } },
            {  "Σ+", new List<rawpattern> { new rawpattern(  @"Σ\+",  "47" )} },
            {  "Σ-", new List<rawpattern> { new rawpattern(  "Σ-",  "48" )} },
            {  "S+", new List<rawpattern> { new rawpattern(  @"S\+",  "47" )} },
            {  "S-", new List<rawpattern> { new rawpattern(  "S-",  "48" )} },


            {"ΣREG",new List<rawpattern>

                {
                    {   new rawpattern(@"ΣREG IND ST ([XYZLT])",  "99 Ft",  "stk" )},
                    {   new rawpattern(@"ΣREG IND ("".{1,14}"")",  "Fn DB",  "nam" )},
                    {   new rawpattern(@"ΣREG IND (\d{2})",  "99 8r",  "reg" )},
                    {   new rawpattern(@"ΣREG (\d{2})",  "99 rr",  "reg") }
                }
            },

             {"SREG",new List<rawpattern>

                {
                    {   new rawpattern(@"SREG IND ST ([XYZLT])",  "99 Ft",  "stk" )},
                    {   new rawpattern(@"SREG IND ("".{1,14}"")",  "Fn DB",  "nam" )},
                    {   new rawpattern(@"SREG IND (\d{2})",  "99 8r",  "reg" )},
                    {   new rawpattern(@"SREG (\d{2})",  "99 rr",  "reg") }
                }
            },

            {  "ΣREG?", new List<rawpattern> { new rawpattern(@"ΣREG\?",  "A6 78") } },
            {  "SREG?", new List<rawpattern> { new rawpattern(@"SREG\?",  "A6 78") } },
            {  "←", new List<rawpattern> { new rawpattern(  "←",  "A6 DC" )} },
            {  "<", new List<rawpattern> { new rawpattern(  "<",  "A6 DC" )} },
            {  "↑", new List<rawpattern> { new rawpattern(  "↑",  "A6 DE" )} },
            {  "^", new List<rawpattern> { new rawpattern(  @"\^",  "A6 DE" )} },
            {  "→", new List<rawpattern> { new rawpattern(  "→",  "A6 DD" )} },
            {  ">", new List<rawpattern> { new rawpattern(  ">",  "A6 DD" )} },
            {  "→DEC", new List<rawpattern> { new rawpattern( "→DEC",  "5F" )} },
            {  ">DEC", new List<rawpattern> { new rawpattern( ">DEC",  "5F" )} },
            {  "→DEG", new List<rawpattern> { new rawpattern( "→DEG",  "6B" )} },
            {  ">DEG", new List<rawpattern> { new rawpattern( ">DEG",  "6B" )} },
            {  "→HMS", new List<rawpattern> { new rawpattern( "→HMS",  "6C" )} },
            {  ">HMS", new List<rawpattern> { new rawpattern( ">HMS",  "6C" )} },
            {  "→HR", new List<rawpattern> { new rawpattern( "→HR",  "6D" )} },
            {  ">HR", new List<rawpattern> { new rawpattern( ">HR",  "6D" )} },
            {  "→OCT", new List<rawpattern> { new rawpattern( "→OCT",  "6F" )} },
            {  ">OCT", new List<rawpattern> { new rawpattern( ">OCT",  "6F" )} },
            {  "→POL", new List<rawpattern> { new rawpattern( "→POL",  "4F" )} },
            {  ">POL", new List<rawpattern> { new rawpattern( ">POL",  "4F" )} },
            {  "→RAD", new List<rawpattern> { new rawpattern( "→RAD",  "6A" )} },
            {  ">RAD", new List<rawpattern> { new rawpattern( ">RAD",  "6A" )} },
            {  "→REC", new List<rawpattern> { new rawpattern( "→REC",  "4E" )} },
            {  ">REC", new List<rawpattern> { new rawpattern( ">REC",  "4E" )} },
            {  "↓", new List<rawpattern> { new rawpattern(  "↓",  "A6 DF" )} },
            {  "v", new List<rawpattern> { new rawpattern(  "v",  "A6 DF" )} },
            {  @"⊢`str`", new List<rawpattern> { new rawpattern(  @"⊢("".{14}"")",  "Fn 7F",  "str" )} }  // max. length 14
        
       
           
       };

        #endregion

    }
}
