using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace DM42.decoder
{
    public class Decoder42
    {

        public static Dictionary<string, List<RpnPattern>> rawMap = new Dictionary<string, List<RpnPattern>>();
        public static Dictionary<int, string> stackMap = new Dictionary<int, string>();
        public static Dictionary<int, string> charMap = new Dictionary<int, string>();
        public static Dictionary<int, string> lblMap = new Dictionary<int, string>();

        public static bool initialized = false;

        public static void initialize()
        {
            
            if (!Decoder42.initialized)
            {
                // transform arr_rawMap -> rawMap
                foreach( KeyValuePair<string,List<RpnPattern>> data  in  arr_rawMap )
                {
                    rawMap.Add(data.Key,data.Value);
                }
 
                foreach (KeyValuePair<int, string> data in arr_stackMap)
                {
                    stackMap.Add(data.Key, data.Value);
                }

                foreach (KeyValuePair<int, string> data in arr_charMap)
                {
                    charMap.Add(data.Key, data.Value);
                }

              
                Decoder42.initialized = true;
            }
           
        }


        /// <summary>
        ///  conversion finale en code RPN 
        /// </summary>
        /// <param name="rpnLine"></param>
        public static void toRpn(ref  RpnLine  rpnLine )
        {

            if (rpnLine.workCode!= "")
            {
               
                if ((rpnLine.Parames.stk != null) && Regex.IsMatch(rpnLine.workCode, "`stk`")) {
                    replaceStack("`stk`", ref  rpnLine);
                    
                }

                
                if ((rpnLine.Parames.num != null) && Regex.IsMatch(rpnLine.workCode, "`num`")) {
                    replaceNumber("`num`", ref  rpnLine);
                   
                }

               
                if ((rpnLine.Parames.key != null) && Regex.IsMatch(rpnLine.workCode, "`key`")) {
                    replaceKey("`key`", ref  rpnLine);
                   
                }

                // numbers
              
                if (rpnLine.Parames.lblno != null && Regex.IsMatch(rpnLine.workCode, "sl"))
                {
                    replaceLabelNo("sl",ref  rpnLine);
                   
                }
               
               
                if (rpnLine.Parames.lblno != null && Regex.IsMatch(rpnLine.workCode,"ll"))
                {
                    replaceLabelNo("ll",ref  rpnLine);
                 
                }

                
                if (rpnLine.Parames.regno != null && Regex.IsMatch(rpnLine.workCode,"sr"))
                {
                    replaceRegister("sr",ref  rpnLine);
                  
                }

                
                if (rpnLine.Parames.regno != null && Regex.IsMatch(rpnLine.workCode,"rr"))
                {
                    replaceRegister("rr",ref  rpnLine);
                    
                }

               
                if (rpnLine.Parames.flgno != null && Regex.IsMatch(rpnLine.workCode,"fl"))
                {
                    replaceFlag("fl", ref rpnLine);
                   
                }

              
                if (rpnLine.Parames.sizno != null && Regex.IsMatch(rpnLine.workCode,"ss ss"))
                {
                    replaceSize("ss ss", ref rpnLine);
                    return;
                }

               
                if (rpnLine.Parames.tonno != null && Regex.IsMatch(rpnLine.workCode,"tn")) {
                    replaceTone("tn", ref rpnLine);
                    
                }

               
                 if (rpnLine.Parames.digno != null && Regex.IsMatch(rpnLine.workCode,"sd")) {
                    replaceDigits("sd", ref rpnLine);
                }

               
                if ((rpnLine.Parames.lbl != null) && Regex.IsMatch(rpnLine.workCode,"lbl")) {
                    replaceLabel("`lbl`", ref rpnLine);
                   
                }
                
                if ((rpnLine.Parames.str != null) && Regex.IsMatch(rpnLine.workCode, "`str`")) {
                    replaceString("`str`",ref  rpnLine);
                    
                }

               
                if ((rpnLine.Parames.nam != null) && Regex.IsMatch(rpnLine.workCode, "`nam`")) {
                    replaceName("`nam`", ref rpnLine);
                   
                    
                }

               
                if ((rpnLine.Parames.nam != null) && Regex.IsMatch(rpnLine.workCode,"reg"))
                {
                    replaceRegister("reg", ref rpnLine);
                    return;

                }


            }
        }

        #region private 
        private static void replaceLabel(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string  lbl = convertRawToString(rpnLine.Parames.lbl);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, '"' + lbl + '"');
            }
        }

        private static void replaceString(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string  str = convertRawToString(rpnLine.Parames.str);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, '"' + str + '"');
            }
        }

        private static void replaceName(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string  nam = convertRawToString(rpnLine.Parames.nam);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, '"' + nam + '"');
            }
        }

        private static void replaceNumber(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string  number = convertRawToNumber(rpnLine.Parames.num);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, number);
            }
        }

        private static void replaceLabelNo(string replace, ref RpnLine rpnLine)
        {
            // ... 99  ... 63 dec:16-99; hex:10-63
            // ... A   ... 66 dec:102  char:65
            // ... J   ... 6F dec:111
            // ... a   ... 7B dec:123  char:97
            // ... e   ... 7F dec:127
            if (rpnLine.workCode != "")
            {
                string  number = "";

                if (rpnLine.Parames.lblno != null) {
                    if (Decoder42.inRange(rpnLine.Parames.lblno, 0, 99))
                    {
                        //00-99
                        number = formatNN(rpnLine.Parames.lblno);
                    }
                    else if (Decoder42.inRange(rpnLine.Parames.lblno, 102, 111))
                    {
                        //A-F
                        System.Text.ASCIIEncoding convertor = new System.Text.ASCIIEncoding();
                        number= convertor.GetChars(new byte[] { (byte)(rpnLine.Parames.lblno - 37)})[0].ToString();
                        // number = String.fromCharCode(rpnLine.Parames.lblno - 37);
                    }
                    else if (Decoder42.inRange(rpnLine.Parames.lblno, 123, 127))
                    {
                        //a-e
                        System.Text.ASCIIEncoding convertor = new System.Text.ASCIIEncoding();
                        number = convertor.GetChars(new byte[] { (byte)(rpnLine.Parames.lblno - 26) })[0].ToString();
                        //number = String.fromCharCode(rpnLine.Parames.lblno - 26);
                    }
                }

                rpnLine.workCode = rpnLine.workCode.Replace(replace, number);
            }
        }

        private static void replaceRegister(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string  number = formatNN(rpnLine.Parames.regno);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, number);
            }
        }

        private static void replaceFlag(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                string number = formatNN(rpnLine.Parames.flgno);
                rpnLine.workCode = rpnLine.workCode.Replace(replace, number);
            }
        }

        private static void replaceStack(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode!= "")
            {
                rpnLine.workCode = rpnLine.workCode.Replace(replace, "" + rpnLine.Parames.stk);
            }
        }

        private static void replaceKey(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                rpnLine.workCode = rpnLine.workCode.Replace(replace, "" + rpnLine.Parames.keyno);
            }
        }

        private static void replaceSize(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode != "")
            {
                rpnLine.workCode = rpnLine.workCode.Replace(replace, formatNN(rpnLine.Parames.sizno)); // 01-9999
            }
        }

        private static void replaceTone(string replace, ref RpnLine rpnLine)
        {
            if (rpnLine.workCode  != "")
            {
                rpnLine.workCode = rpnLine.workCode.Replace(replace, "" + rpnLine.Parames.tonno); // 0-9
            }
        }

        private static void replaceDigits(string replace, ref RpnLine rpnLine )
        {
            if (rpnLine.workCode!="")
            {
                // rpnLine.params.digno: 0-9, rpnLine.params.dig: 00-09
                rpnLine.workCode = rpnLine.workCode.Replace(replace, formatNN(rpnLine.Parames.digno));
            }
        }

        /** Changing numbers into corresponding opcodes. 11 1A 12 13 14 1B 1C 14 15 15 00 -> '1.234E-455' */
        private static string convertRawToNumber(string raw)
        {
            string  number = "";
            Regex lReg; 

            if (raw!= "")
            {
              lReg = new Regex("00$");
              raw = lReg.Replace(raw, "");
              lReg = new Regex("1A");
              raw = lReg.Replace(raw, ".");
              lReg = new Regex("1B");
              raw = lReg.Replace(raw, "ᴇ");
              lReg = new Regex("1C");
              raw = lReg.Replace(raw, "-");
              lReg = new Regex("1([0-9])");
              raw = lReg.Replace(raw, "$1");
              lReg = new Regex(" ");
              raw = lReg.Replace(raw, "");
              number = raw;
            }

            // TODO: Format 0.008 -> 8ᴇ-3
            lReg = new Regex(@"(0 +)\.(0 +)(\d +)(ᴇ |)((-|)\d +|)");
            MatchCollection MCol= lReg.Matches(number); 
            if (MCol.Count>0)
            { 
            
              int len1 = MCol[1].Length;
              int len2 = MCol[2].Length;
              int len3 = MCol[3].Length;
              int exp =  int.Parse(MCol[5].Value);
              string FloatStr = MCol[0].Value;
               FloatStr.Replace('ᴇ', 'e'); 
              float floatValue = float.Parse(FloatStr);
              // 0.008 -> 8ᴇ-3
              if (len2 > 1)
              {
                //console.log(
                //  (float * Math.pow(10, (-exp + len2 + len3))).toExponential(0).replace('e', 'ᴇ') + (exp - (len2 + len3))
                //);
              }
            }

    return number;
    }

     private static string convertRawToString(string raw)
        {
        string str = "";
        if (raw !="")
        {
            string[] chars = raw.Split(' ');
            foreach(string  Chr in chars)
            {
                 Byte lB = convertHexAsByte(Chr); 
                 if (charMap.ContainsKey(lB))
                 {
                    str += Decoder42.charMap[lB];
                 } else
                {

                   System.Text.ASCIIEncoding convertor = new System.Text.ASCIIEncoding();
                    str += convertor.GetChars(new byte[] { lB })[0];
                 }

             }
         }
   
        return str;
        }

        /** Changing integers (size one byte, 0-255) into hex string . 123 -> 7B, 255 -> FF */
     private static Byte convertHexAsByte(string hex) 
      {
            Byte lByte = Convert.ToByte(hex, 16); //parseInt(hex, 16);
            return lByte;
      }


    private static string formatNN(int? n) 
    {
        if(n != null)
        {
        return n > 9 ? "" + n : "0" + n;
        }
        return "??";
    }

    private static bool inRange(int? x, int min, int max) 
    {
                return ((x - min) * (x - max)) <= 0;
    }

    private static Dictionary<int, string> arr_stackMap = new Dictionary<int, string>()
     {
            {  0,  "T" },
            {  1,  "Z" },
            {  2,  "Y" },
            {  3,  "X" },
            {  4,  "L"}
     };

    
        // FOCAL character set https://en.wikipedia.org/wiki/FOCAL_character_set key is used as regex 
    private static Dictionary<int, string> arr_charMap =  new Dictionary<int, string>()
        {
            {  0,  "÷" },
            {  1,  "×" },
            {  2,  "√" },
            {  3,  "∫" },
            {  4,  "░" },
            {  5,  "Σ" },
            {  6,  "▶" },
            {  7,  "π" },
            {  8,  "¿" },
            {  9,  "≤" },
            {  10,  "\\[LF\\]" }, // for [LF] line feed
           // {  10,  "␊" }, // ␊ see: https://www.compart.com/de/unicode/U+240A
            {  11,  "≥" },
            {  12,  "≠" },
            {  13,  "↵" },
            {  14,  "↓" },
            {  15,  "→" },
            {  16,  "←" },
            {  17,  "µ" }, // different bytes B5
            //{  17,  "μ" }, // different bytes N<
            {  18,  "£" },
            //{  18,  "₤" },
            {  19,  "°" },
            {  20,  "Å" },
            {  21,  "Ñ" },
            {  22,  "Ä" },
            {  23,  "∡" },
            {  24,  "ᴇ" },
            {  25,  "Æ" },
            {  26,  "…" },
            {  27,  "␛" }, // ␛ see: https://www.compart.com/de/unicode/U+241B
            {  28,  "Ö" },
            {  29,  "Ü" },
            {  30,  "▒" },
            {  31,  "■" },
            //{  31,  "•" }, // see above
            // {  32,  "SP" },
            // {  33,  "!" },
            // {  34,  """ }, // single quote
            // {  35,  "#" },
            // {  36,  "$" },
            // {  37,  "%" },
            // {  38,  "&" },
            // {  39,  """ }, // double quotes !!
            // {  40,  "(" },
            // {  41,  ")" },
            // {  42,  "*" },
            // {  43,  "+" },
            // {  44,  "," },
            // {  45,  "-" },
            // {  46,  "." },
            // {  47,  "" },
            // {  48,  "0" },
            // {  49,  "1" },
            // {  50,  "2" },
            // {  51,  "3" },
            // {  52,  "4" },
            // {  53,  "5" },
            // {  54,  "6" },
            // {  55,  "7" },
            // {  56,  "8" },
            // {  57,  "9" },
            // {  58,  ":" },
            // {  59,  ";" },
            // {  60,  "<" },
            // {  61,  "=" },
            // {  62,  ">" },
            // {  63,  "?" },
            // {  64,  "@" },
            // {  65,  "A" },
            // {  66,  "B" },
            // {  67,  "C" },
            // {  68,  "D" },
            // {  69,  "E" },
            // {  70,  "F" },
            // {  71,  "G" },
            // {  72,  "H" },
            // {  73,  "I" },
            // {  74,  "J" },
            // {  75,  "K" },
            // {  76,  "L" },
            // {  77,  "M" },
            // {  78,  "N" },
            // {  79,  "O" },
            // {  80,  "P" },
            // {  81,  "Q" },
            // {  82,  "R" },
            // {  83,  "S" },
            // {  84,  "T" },
            // {  85,  "U" },
            // {  86,  "V" },
            // {  87,  "W" },
            // {  88,  "X" },
            // {  89,  "Y" },
            // {  90,  "Z" },
            // {  91,  "[" },
            {  92,  "\\\\" }, // for \
            // {  93,  "]" },
            {  94,  "↑" }
            // {  95,  "_" },
            // {  96,  "`" },
            // {  ??,  "´" },
            // {  97,  "a" },
            // {  98,  "b" },
            // {  99,  "c" },
            // {  100,  "d" },
            // {  101,  "e" },
            // {  102,  "f" },
            // {  103,  "g" },
            // {  104,  "h" },
            // {  105,  "i" },
            // {  106,  "j" },
            // {  107,  "k" },
            // {  108,  "l" },
            // {  109,  "m" },
            // {  110,  "n" },
            // {  111,  "o" },
            // {  112,  "p" },
            // {  113,  "q" },
            // {  114,  "r" },
            // {  115,  "s" },
            // {  116,  "t" },
            // {  117,  "u" },
            // {  118,  "v" },
            // {  119,  "w" },
            // {  120,  "x" },
            // {  121,  "y" },
            // {  122,  "z" },
            // {  123,  "{" },
            // {  124,  "|" },
            // {  125,  "}" },
            // {  126,  "~" }
            // {  127,  "⊦" }

            // {  ???,  "´" }
        };

        // 0(?<lblno>[2-9A-F]): LBL 01-15
        // Fn: F([1-9A-F]) Label: max. length 14
        // 7t: 7([0-4]); stack 0-4
        // 8r: ([89A-E][0-9A-F]); r: dec:1..99; 128 + r => hex:81..E3
        // Ft: F([0-4]); ... IND ST [XYZLT]
        // [23]r: (2[0-9A-F]); register dec:1-15, hex: 21-2F
        // rr: ([0-7][0-9A-F]); register dec:16-99, hex:10-63
        // nn: (?<digits>0[1-9]); digits; dec:0-9; 00-09
        // nn: digits; dec:10,11
        // tone: (?<tone>0[0-9]); dec:1-9; hex:01-09

        // LBL 14  0F
        // LBL 99  CF63
        // LBL A   CF66
        // LBL J   CF6F
        // LBL a   CF7B
        // LBL e   CF7F

        // GTO 14  BF00
        // GTO 99  D00063
        // GTO A   D00066
        // GTO J   D0006F
        // GTO a   D0007B
        // GTO e   D0007F

        // RCL 01  21
        // RCL 15  2F
        // RCL 16  9010
        // RCL 99  9063

        // XEQ 14  E0000E
        // XEQ 99  E00063
        // XEQ A   E00066
        // XEQ J   E0006F
        // XEQ a   E0007B
        // XEQ e   E0007F

      

       private static Dictionary<string, List<RpnPattern>> arr_rawMap = new Dictionary<string, List<RpnPattern>>()
       {
            {
                "0", new List<RpnPattern>
                {
                    new RpnPattern (  "00",  1, "NULL","NULL" ),
                    new RpnPattern (  "0([1-9A-F])",  1, "LBL sl",  "lblno-1" ) //+ LBL 00-14
                }
            },
            {
                "1",new List<RpnPattern>()

                {
                 new RpnPattern (  "1D F([1-9A-F])",  2,  @"GTO `lbl`",  "lbll" ), //+ lbl max length 14
                 new RpnPattern (  "1E F([1-9A-F])",  2,  @"XEQ `lbl`",  "lbll" ) //+ lbl max length 14
                                 //no numbers here: { " /(1[0-9A-C] )+00/,  1,  "`num`",  "number" }
                }
            },
            {
                "2",new List<RpnPattern>()

                {
                 new RpnPattern (  "2([0-9A-F])",  1,  "RCL sr", "reg" )
                // RCL 01  21
                // RCL 15  2F
                }

            }, //+
            {
                "3",new List<RpnPattern>()
                {
                 new RpnPattern  ( "3([0-9A-F])",  1,  "STO sr",  "reg"), //+
                }
             },

            {
                 "4",new List<RpnPattern>()
                 {

                    new RpnPattern  (  "40",  1,  "+" ), //+
                    new RpnPattern  (  "41",  1,  "-" ), //+
                    new RpnPattern  (  "42",  1,  "×" ), //+
                    new RpnPattern  (  "43",  1,  "÷" ), //+
                    new RpnPattern  (  "44",  1,  "X<Y?" ), //+
                    new RpnPattern  (  "45",  1,  "X>Y?" ),
                    new RpnPattern  (  "46",  1,  "X≤Y?" ), //+
                    new RpnPattern  (  "47",  1,  "Σ+" ),
                    new RpnPattern  (  "48",  1,  "Σ-" ),
                    new RpnPattern  (  "49",  1,  "HMS+" ),
                    new RpnPattern  (  "4A",  1,  "HMS-" ),
                    new RpnPattern  (  "4B",  1,  "MOD" ),
                    new RpnPattern  (  "4C",  1,  "%" ),
                    new RpnPattern  (  "4D",  1,  "%CH" ),
                    new RpnPattern  (  "4E",  1,  "→REC" ),
                    new RpnPattern  (  "4F",  1,  "→POL" )
                 }

            },
            {
                "5", new List<RpnPattern>()

           {
                 new RpnPattern ( "50",  1,  "LN" ),
                 new RpnPattern ( "51",  1,  "X↑2" ),
                 new RpnPattern ( "52",  1,  "SQRT" ),
                 new RpnPattern ( "53",  1,  "Y↑X" ),
                 new RpnPattern ( "54",  1,  "+/-" ),
                 new RpnPattern ( "55",  1,  "E↑X" ),
                 new RpnPattern ( "56",  1,  "LOG" ),
                 new RpnPattern ( "57",  1,  "10↑X" ),
                 new RpnPattern ( "58",  1,  "E↑X-1" ),
                 new RpnPattern ( "59",  1,  "SIN" ),
                 new RpnPattern ( "5A",  1,  "COS" ),
                 new RpnPattern ( "5B",  1,  "TAN" ),
                 new RpnPattern ( "5C",  1,  "ASIN" ),
                 new RpnPattern ( "5D",  1,  "ACOS" ),
                 new RpnPattern ( "5E",  1,  "ATAN" ),
                 new RpnPattern ( "5F",  1,  "→DEC" )
             }
            },
            {
                "6",new List<RpnPattern>()

                {
                 new RpnPattern ( "60",  1,  "1/X" ),
                 new RpnPattern ( "61",  1,  "ABS" ), //+
                 new RpnPattern ( "62",  1,  "N!" ),
                 new RpnPattern ( "63",  1,  "X≠0?" ),
                 new RpnPattern ( "64",  1,  "X>0?" ),
                 new RpnPattern ( "65",  1,  "LN1+X" ),
                 new RpnPattern ( "66",  1,  "X<0?" ),
                 new RpnPattern ( "67",  1,  "X=0?" ), //+
                 new RpnPattern ( "68",  1,  "IP" ), //+
                 new RpnPattern ( "69",  1,  "FP" ),
                 new RpnPattern ( "6A",  1,  "→RAD" ),
                 new RpnPattern ( "6B",  1,  "→DEG" ),
                 new RpnPattern ( "6C",  1,  "→HMS" ),
                 new RpnPattern ( "6D",  1,  "→HR" ),
                 new RpnPattern ( "6E",  1,  "RND" ),
                 new RpnPattern ( "6F",  1,  "→OCT" )
                }
            },

            {
                 "7",new List<RpnPattern>()

                 {
                new RpnPattern ( "70",  1,  "CLΣ" ),
                new RpnPattern ( "71",  1,  "X<>Y" ), //+
                new RpnPattern ( "72",  1,  "PI" ),
                new RpnPattern ( "73",  1,  "CLST" ), //+
                new RpnPattern ( "74",  1,  "R↑" ),
                new RpnPattern ( "75",  1,  "R↓" ),
                new RpnPattern ( "76",  1,  "LASTX" ),
                new RpnPattern ( "77",  1,  "CLX" ),
                new RpnPattern ( "78",  1,  "X=Y?" ), //+
                new RpnPattern ( "79",  1,  "X≠Y?" ), //+
                new RpnPattern ( "7A",  1,  "SIGN" ),
                new RpnPattern ( "7B",  1,  "X≤0?" ),
                new RpnPattern ( "7C",  1,  "MEAN" ),
                new RpnPattern ( "7D",  1,  "SDEV" ),
                new RpnPattern ( "7E",  1,  "AVIEW" ), //+
                new RpnPattern ( "7F",  1,  "CLD" )
                }
            },

            {
                "8",new List<RpnPattern>()
               {
                new RpnPattern ( "80",  1,  "DEG" ),
                new RpnPattern ( "81",  1,  "RAD" ),
                new RpnPattern ( "82",  1,  "GRAD" ),
                new RpnPattern ( "83",  1,  "ENTER" ), //+
                new RpnPattern ( "84",  1,  "STOP" ), //+
                new RpnPattern ( "85",  1,  "RTN" ), //+
                new RpnPattern ( "86",  1,  "BEEP" ), //+
                new RpnPattern ( "87",  1,  "CLA" ),
                new RpnPattern ( "88",  1,  "ASHF" ),
                new RpnPattern ( "89",  1,  "PSE" ), //+
                new RpnPattern ( "8A",  1,  "CLRG" ),
                new RpnPattern ( "8B",  1,  "AOFF" ),
                new RpnPattern ( "8C",  1,  "AON" ),
                new RpnPattern ( "8D",  1,  "OFF" ),
                new RpnPattern ( "8E",  1,  "PROMPT" ),
                new RpnPattern ( "8F",  1,  "ADV" )
               }
            },

            {
                "9",new List<RpnPattern>()

                {
                new RpnPattern ( "90 7([0-4])",  2,  "RCL ST `stk`",  "stk" ),
                new RpnPattern ( "90 ([89A-E][0-9A-F])",  2,  "RCL IND rr",  "reg-128" ), //+ r dec:0-99 + 128
                new RpnPattern ( "90 F([0-4])",  2,  "RCL IND ST `stk`",  "stk" ),
                new RpnPattern ( "90 ([0-7][0-9A-F])",  2,  "RCL rr",  "reg" ),
                // RCL 16  90 10  dec:16-99; hex:10-63
                // RCL 99  90 63
                new RpnPattern ( "91 7([0-4])",  2,  "STO ST `stk`",  "stk" ), //+
                new RpnPattern ( "91 ([89A-E][0-9A-F])",  2,  "STO IND rr",  "reg-128" ), //+
                new RpnPattern ( "91 F([0-4])",  2,  "STO IND ST `stk`",  "stk" ),
                new RpnPattern ( "91 ([0-7][0-9A-F])",  2,  "STO rr",  "reg" ),
                new RpnPattern ( "92 7([0-4])",  2,  "STO+ ST `stk`",  "stk" ),
                new RpnPattern ( "92 ([89A-E][0-9A-F])",  2,  "STO+ IND rr",  "reg-128" ),
                new RpnPattern ( "92 F([0-4])",  2,  "STO+ IND ST `stk`",  "stk" ),
                new RpnPattern ( "92 ([0-7][0-9A-F])",  2,  "STO+ rr",  "reg" ),
                new RpnPattern ( "93 7([0-4])",  2,  "STO- ST `stk`",  "stk" ),
                new RpnPattern ( "93 ([89A-E][0-9A-F])",  2,  "STO- IND rr",  "reg-128" ),
                new RpnPattern ( "93 F([0-4])",  2,  "STO- IND ST `stk`",  "stk" ),
                new RpnPattern ( "93 ([0-7][0-9A-F])",  2,  "STO- rr",  "reg" ), //+
                new RpnPattern ( "94 7([0-4])",  2,  "STO× ST `stk`",  "stk" ),
                new RpnPattern ( "94 ([89A-E][0-9A-F])",  2,  "STO× IND rr",  "reg-128" ),
                new RpnPattern ( "94 F([0-4])",  2,  "STO× IND ST `stk`",  "stk" ),
                new RpnPattern ( "94 ([0-7][0-9A-F])",  2,  "STO× rr",  "reg" ),
                new RpnPattern ( "95 7([0-4])",  2,  "STO÷ ST `stk`",  "stk" ),
                new RpnPattern ( "95 ([89A-E][0-9A-F])",  2,  "STO÷ IND rr",  "reg-128" ),
                new RpnPattern ( "95 F([0-4])",  2,  "STO÷ IND ST `stk`",  "stk" ),
                new RpnPattern ( "95 ([0-7][0-9A-F])",  2,  "STO÷ rr",  "reg" ),
                new RpnPattern ( "96 7([0-4])",  2,  "ISG ST `stk`",  "stk" ), //+
                new RpnPattern ( "96 ([89A-E][0-9A-F])",  2,  "ISG IND rr",  "reg-128" ), //+
                new RpnPattern ( "96 F([0-4])",  2,  "ISG IND ST `stk`",  "stk" ),
                new RpnPattern ( "96 ([0-7][0-9A-F])",  2,  "ISG rr",  "reg" ), //+
                new RpnPattern ( "97 7([0-4])",  2,  "DSE ST `stk`",  "stk" ),
                new RpnPattern ( "97 ([89A-E][0-9A-F])",  2,  "DSE IND rr",  "reg-128" ),
                new RpnPattern ( "97 F([0-4])",  2,  "DSE IND ST `stk`",  "stk" ),
                new RpnPattern ( "97 ([0-7][0-9A-F])",  2,  "DSE rr",  "reg" ), //+
                new RpnPattern ( "98 7([0-4])",  2,  "VIEW ST `stk`",  "stk" ),
                new RpnPattern ( "98 ([89A-E][0-9A-F])",  2,  "VIEW IND rr",  "reg-128" ),
                new RpnPattern ( "98 F([0-4])",  2,  "VIEW IND ST `stk`",  "stk" ),
                new RpnPattern ( "98 ([0-7][0-9A-F])",  2,  "VIEW rr",  "reg" ),
                new RpnPattern ( "99 ([89A-E][0-9A-F])",  2,  "ΣREG IND rr",  "reg-128" ),
                new RpnPattern ( "99 F([0-4])",  2,  "ΣREG IND ST `stk`",  "stk" ),
                new RpnPattern ( "99 ([0-7][0-9A-F])",  2,  "ΣREG rr",  "reg" ),
                new RpnPattern ( "9A 7([0-4])",  2,  "ASTO ST `stk`",  "stk" ),
                new RpnPattern ( "9A ([89A-E][0-9A-F])",  2,  "ASTO IND rr",  "reg-128" ),
                new RpnPattern ( "9A F([0-4])",  2,  "ASTO IND ST `stk`",  "stk" ),
                new RpnPattern ( "9A ([0-7][0-9A-F])",  2,  "ASTO rr",  "reg" ),
                new RpnPattern ( "9B 7([0-4])",  2,  "ARCL ST `stk`",  "stk" ),
                new RpnPattern ( "9B ([89A-E][0-9A-F])",  2,  "ARCL IND rr",  "reg-128" ),
                new RpnPattern ( "9B F([0-4])",  2,  "ARCL IND ST `stk`",  "stk" ),
                new RpnPattern ( "9B ([0-7][0-9A-F])",  2,  "ARCL rr",  "reg" ), //+
                new RpnPattern ( "9C ([89A-E][0-9A-F])",  2,  "FIX IND rr",  "reg-128" ),
                new RpnPattern ( "9C F([0-4])",  2,  "FIX IND ST `stk`",  "stk" ),
                new RpnPattern ( "9C (0[0-9])",  2,  "FIX sd",  "dig" ),
                new RpnPattern ( "9D ([89A-E][0-9A-F])",  2,  "SCI IND rr",  "reg-128" ),
                new RpnPattern ( "9D F([0-4])",  2,  "SCI IND ST `stk`",  "stk" ),
                new RpnPattern ( "9D (0[0-9])",  2,  "SCI sd",  "dig" ),
                new RpnPattern ( "9E ([89A-E][0-9A-F])",  2,  "ENG IND rr",  "reg-128" ),
                new RpnPattern ( "9E F([0-4])",  2,  "ENG IND ST `stk`",  "stk" ),
                new RpnPattern ( "9E (0[0-9])",  2,  "ENG sd",  "dig" ),
                new RpnPattern ( "9F ([89A-E][0-9A-F])",  2,  "TONE IND rr",  "reg-128" ),
                new RpnPattern ( "9F F([0-4])",  2,  "TONE IND ST `stk`",  "stk" ),
                new RpnPattern ( "9F (0[0-9])",  2,  "TONE tn",  "ton" ) //+
                }
            },
            {
                "A",new List<RpnPattern>()

                {
                new RpnPattern ( "A0 61",  2,  "SINH" ),
                new RpnPattern ( "A0 62",  2,  "COSH" ),
                new RpnPattern ( "A0 63",  2,  "TANH" ),
                new RpnPattern ( "A0 64",  2,  "ASINH" ),
                new RpnPattern ( "A0 65",  2,  "ATANH" ),
                new RpnPattern ( "A0 66",  2,  "ACOSH" ),
                new RpnPattern ( "A0 6F",  2,  "COMB" ),
                new RpnPattern ( "A0 70",  2,  "PERM" ),
                new RpnPattern ( "A0 71",  2,  "RAN" ),
                new RpnPattern ( "A0 72",  2,  "COMPLEX" ), //+
                new RpnPattern ( "A0 73",  2,  "SEED" ),
                new RpnPattern ( "A0 74",  2,  "GAMMA" ),
                new RpnPattern ( "A0 9F",  2,  "BEST" ),
                new RpnPattern ( "A0 A0",  2,  "EXPF" ),
                new RpnPattern ( "A0 A1",  2,  "LINF" ),
                new RpnPattern ( "A0 A2",  2,  "LOGF" ),
                new RpnPattern ( "A0 A3",  2,  "PWRF" ),
                new RpnPattern ( "A0 A4",  2,  "SLOPE" ),
                new RpnPattern ( "A0 A5",  2,  "SUM" ),
                new RpnPattern ( "A0 A6",  2,  "YINT" ),
                new RpnPattern ( "A0 A7",  2,  "CORR" ),
                new RpnPattern ( "A0 A8",  2,  "FCSTX" ),
                new RpnPattern ( "A0 A9",  2,  "FCSTY" ),
                new RpnPattern ( "A0 AA",  2,  "INSR" ), //+
                new RpnPattern ( "A0 AB",  2,  "DELR" ),
                new RpnPattern ( "A0 AC",  2,  "WMEAN" ),
                new RpnPattern ( "A0 AD",  2,  "LINΣ" ),
                new RpnPattern ( "A0 AE",  2,  "ALLΣ" ),
                new RpnPattern ( "A0 E2",  2,  "HEXM" ),
                new RpnPattern ( "A0 E3",  2,  "DECM" ),
                new RpnPattern ( "A0 E4",  2,  "OCTM" ),
                new RpnPattern ( "A0 E5",  2,  "BINM" ),
                new RpnPattern ( "A0 E6",  2,  "BASE+" ),
                new RpnPattern ( "A0 E7",  2,  "BASE-" ), //+
                new RpnPattern ( "A0 E8",  2,  "BASE×" ),
                new RpnPattern ( "A0 E9",  2,  "BASE÷" ),
                new RpnPattern ( "A0 EA",  2,  "BASE±" ),
                new RpnPattern ( "A2 59",  2,  "POLAR" ),
                new RpnPattern ( "A2 5A",  2,  "RECT" ), //+
                new RpnPattern ( "A2 5B",  2,  "RDX." ),
                new RpnPattern ( "A2 5C",  2,  "RDX," ),
                new RpnPattern ( "A2 5D",  2,  "ALL" ), //+
                new RpnPattern ( "A2 5E",  2,  "MENU" ), //+
                new RpnPattern ( "A2 5F",  2,  "X≥0?" ),
                new RpnPattern ( "A2 60",  2,  "X≥Y?" ), //+
                new RpnPattern ( "A2 62",  2,  "CLKEYS" ),
                new RpnPattern ( "A2 63",  2,  "KEYASN" ),
                new RpnPattern ( "A2 64",  2,  "LCLBL" ),
                new RpnPattern ( "A2 65",  2,  "REAL?" ),
                new RpnPattern ( "A2 66",  2,  "MAT?" ),
                new RpnPattern ( "A2 67",  2,  "CPX?" ),
                new RpnPattern ( "A2 68",  2,  "STR?" ),
                new RpnPattern ( "A2 6A",  2,  "CPXRES" ),
                new RpnPattern ( "A2 6B",  2,  "REALRES" ),
                new RpnPattern ( "A2 6C",  2,  "EXITALL" ), //+
                new RpnPattern ( "A2 6D",  2,  "CLMENU" ), //+
                new RpnPattern ( "A2 6E",  2,  "GETKEY" ),
                new RpnPattern ( "A2 6F",  2,  "CUSTOM" ),
                new RpnPattern ( "A2 70",  2,  "ON" ),
                new RpnPattern ( "A5 87",  2,  "NOT" ),
                new RpnPattern ( "A5 88",  2,  "AND" ),
                new RpnPattern ( "A5 89",  2,  "OR" ),
                new RpnPattern ( "A5 8A",  2,  "XOR" ),
                new RpnPattern ( "A5 8B",  2,  "ROTXY" ),
                new RpnPattern ( "A5 8C",  2,  "BIT?" ),
                new RpnPattern ( "A6 31",  2,  "AIP" ), //+
                new RpnPattern ( "A6 41",  2,  "ALENG" ),
                new RpnPattern ( "A6 46",  2,  "AROT" ),
                new RpnPattern ( "A6 47",  2,  "ATOX" ),
                new RpnPattern ( "A6 5C",  2,  "POSA" ),
                new RpnPattern ( "A6 6F",  2,  "XTOA" ),
                new RpnPattern ( "A6 78",  2,  "ΣREG?" ),
                new RpnPattern ( "A6 81",  2,  "ADATE" ),
                new RpnPattern ( "A6 84",  2,  "ATIME" ),
                new RpnPattern ( "A6 85",  2,  "ATIME24" ),
                new RpnPattern ( "A6 86",  2,  "CLK12" ),
                new RpnPattern ( "A6 87",  2,  "CLK24" ),
                new RpnPattern ( "A6 8C",  2,  "DATE" ),
                new RpnPattern ( "A6 8D",  2,  "DATE+" ),
                new RpnPattern ( "A6 8E",  2,  "DDAYS" ),
                new RpnPattern ( "A6 8F",  2,  "DMY" ),
                new RpnPattern ( "A6 90",  2,  "DOW" ),
                new RpnPattern ( "A6 91",  2,  "MDY" ),
                new RpnPattern ( "A6 9C",  2,  "TIME" ),
                new RpnPattern ( "A6 C9",  2,  "TRANS" ),
                new RpnPattern ( "A6 CA",  2,  "CROSS" ),
                new RpnPattern ( "A6 CB",  2,  "DOT" ),
                new RpnPattern ( "A6 CC",  2,  "DET" ),
                new RpnPattern ( "A6 CD",  2,  "UVEC" ),
                new RpnPattern ( "A6 CE",  2,  "INVRT" ),
                new RpnPattern ( "A6 CF",  2,  "FNRM" ),
                new RpnPattern ( "A6 D0",  2,  "RSUM" ),
                new RpnPattern ( "A6 D1",  2,  "R<>R" ),
                new RpnPattern ( "A6 D2",  2,  "I+" ),
                new RpnPattern ( "A6 D3",  2,  "I-" ),
                new RpnPattern ( "A6 D4",  2,  "J+" ), //+
                new RpnPattern ( "A6 D5",  2,  "J-" ),
                new RpnPattern ( "A6 D6",  2,  "STOEL" ),
                new RpnPattern ( "A6 D7",  2,  "RCLEL" ), //+
                new RpnPattern ( "A6 D8",  2,  "STOIJ" ),
                new RpnPattern ( "A6 D9",  2,  "RCLIJ" ),
                new RpnPattern ( "A6 DA",  2,  "NEWMAT" ), //+
                new RpnPattern ( "A6 DB",  2,  "OLD" ),
                new RpnPattern ( "A6 DC",  2,  "←" ),
                new RpnPattern ( "A6 DD",  2,  "→" ), //+
                new RpnPattern ( "A6 DE",  2,  "↑" ),
                new RpnPattern ( "A6 DF",  2,  "↓" ),
                new RpnPattern ( "A6 E1",  2,  "EDIT" ),
                new RpnPattern ( "A6 E2",  2,  "WRAP" ), //+
                new RpnPattern ( "A6 E3",  2,  "GROW" ), //+
                new RpnPattern ( "A6 E7",  2,  "DIM?" ),
                new RpnPattern ( "A6 E8",  2,  "GETM" ),
                new RpnPattern ( "A6 E9",  2,  "PUTM" ),
                new RpnPattern ( "A6 EA",  2,  "[MIN]" ),
                new RpnPattern ( "A6 EB",  2,  "[MAX]" ),
                new RpnPattern ( "A6 EC",  2,  "[FIND]" ),
                new RpnPattern ( "A6 ED",  2,  "RNRM" ),
                new RpnPattern ( "A7 48",  2,  "PRA" ),
                new RpnPattern ( "A7 52",  2,  "PRΣ" ),
                new RpnPattern ( "A7 53",  2,  "PRSTK" ),
                new RpnPattern ( "A7 54",  2,  "PRX" ),
                new RpnPattern ( "A7 5B",  2,  "MAN" ),
                new RpnPattern ( "A7 5C",  2,  "NORM" ),
                new RpnPattern ( "A7 5D",  2,  "TRACE" ),
                new RpnPattern ( "A7 5E",  2,  "PRON" ),
                new RpnPattern ( "A7 5F",  2,  "PROFF" ),
                new RpnPattern ( "A7 60",  2,  "DELAY" ),
                new RpnPattern ( "A7 61",  2,  "PRUSR" ),
                new RpnPattern ( "A7 62",  2,  "PRLCD" ),
                new RpnPattern ( "A7 63",  2,  "CLLCD" ),
                new RpnPattern ( "A7 64",  2,  "AGRAPH" ),
                new RpnPattern ( "A7 65",  2,  "PIXEL" ),
                new RpnPattern ( "A7 CF",  2,  "ACCEL" ),
                new RpnPattern ( "A7 D0",  2,  "LOCAT" ),
                new RpnPattern ( "A7 D1",  2,  "HEADING" ),
                new RpnPattern ( "A7 D3",  2,  "WSIZE" ),
                new RpnPattern ( "A7 D4",  2,  "WSIZE?" ),
                new RpnPattern ( "A7 D5",  2,  "YMD" ),
                new RpnPattern ( "A7 D6",  2,  "BSIGNED" ),
                new RpnPattern ( "A7 D7",  2,  "BWRAP" ),
                new RpnPattern ( "A7 D8",  2,  "BRESET" ),
                new RpnPattern ( "A8 ([89A-E][0-9A-F])",  2,  "SF IND rr",  "reg-128" ),
                new RpnPattern ( "A8 F([0-4])",  2,  "SF IND ST `stk`",  "stk" ),
                new RpnPattern ( "A8 ([0-7][0-9A-F])",  2,  "SF fl",  "flg" ), //+ flags dec:00-99 hex:00-63
                new RpnPattern ( "A9 ([89A-E][0-9A-F])",  2,  "CF IND rr",  "reg-128" ),
                new RpnPattern ( "A9 F([0-4])",  2,  "CF IND ST `stk`",  "stk" ),
                new RpnPattern ( "A9 ([0-7][0-9A-F])",  2,  "CF fl",  "flg" ),
                new RpnPattern ( "AA ([89A-E][0-9A-F])",  2,  "FS?C IND rr",  "reg-128" ),
                new RpnPattern ( "AA F([0-4])",  2,  "FS?C IND ST `stk`",  "stk" ),
                new RpnPattern ( "AA ([0-7][0-9A-F])",  2,  "FS?C fl",  "flg" ), //+
                new RpnPattern ( "AB ([89A-E][0-9A-F])",  2,  "FC?C IND rr",  "reg-128" ),
                new RpnPattern ( "AB F([0-4])",  2,  "FC?C IND ST `stk`",  "stk" ),
                new RpnPattern ( "AB ([0-7][0-9A-F])",  2,  "FC?C fl",  "flg" ),
                new RpnPattern ( "AC ([89A-E][0-9A-F])",  2,  "FS? IND rr",  "reg-128" ),
                new RpnPattern ( "AC F([0-4])",  2,  "FS? IND ST `stk`",  "stk" ),
                new RpnPattern ( "AC ([0-7][0-9A-F])",  2,  "FS? fl",  "flg" ), //+
                new RpnPattern ( "AD ([89A-E][0-9A-F])",  2,  "FC? IND rr",  "reg-128" ),
                new RpnPattern ( "AD F([0-4])",  2,  "FC? IND ST `stk`",  "stk" ),
                new RpnPattern ( "AD ([0-7][0-9A-F])",  2,  "FC? fl",  "flg" ), //+
                new RpnPattern ( "AE 7([0-4])",  2,  "GTO IND ST `stk`",  "stk" ),
                new RpnPattern ( "AE ([89A-E][0-9A-F])",  2,  "XEQ IND rr",  "reg-128" ),
                new RpnPattern ( "AE F([0-4])",  2,  "XEQ IND ST `stk`",  "stk" ),
                new RpnPattern ( "AE ([0-7][0-9A-F])",  2,  "GTO IND rr",  "reg" )
                }
            },
            {
            "B",new List<RpnPattern>()
            {

                new RpnPattern ( "B([1-9A-F]) 00",  2,  "GTO sl",  "lblno-1" ) //+ dec: 01-14, l+1: 2-F
             }
                // GTO 14  BF 00
      
            },
            {
            "C",new List<RpnPattern>()

            {
                new RpnPattern ( "C0 00 0D",  3,  "END" ),
                new RpnPattern ( "C0 00 F([1-9A-F]) 00",  4,  "LBL `lbl`",  "lbll-1" ), //+
                new RpnPattern ( "CE 7([0-4])",  2,  "X<> ST `stk`",  "stk" ),
                new RpnPattern ( "CE ([89A-E][0-9A-F])",  2,  "X<> IND rr",  "reg-128" ),
                new RpnPattern ( "CE F([0-4])",  2,  "X<> IND ST `stk`",  "stk" ),
                new RpnPattern ( "CE ([0-6][0-9A-F])",  2,  "X<> rr",  "reg" ),
                new RpnPattern ( "CF ([0-7][0-9A-F])",  2,  "LBL ll",  "lblno" ) //+
                    // LBL 99  CF 63 dec:16-99; hex:10-63
                    // LBL A   CF 66
                    // LBL J   CF 6F
                    // LBL a   CF 7B
                    // LBL e   CF 7F
                }
            },
            {
            "D",new List<RpnPattern>()

            {
                 new RpnPattern ( "D0 00 ([0-7][0-9A-F])",  3,  "GTO ll",  "lblno" ) //+
                    // GTO 99  D0 00 63 dec:15-99 hex:0F-63
                    // GTO A   D0 00 66
                    // GTO J   D0 00 6F
                    // GTO a   D0 00 7B
                    // GTO e   D0 00 7F
                }
            },

            {
            "E",new List<RpnPattern>()

            {
                new RpnPattern (  "E0 00 ([0-7][0-9A-F])",  3,  "XEQ ll",  "lblno" ) //+
                    // XEQ 14  E0 00 0E dec:15-99 hex:0F-63
                    // XEQ 99  E0 00 63
                    // XEQ A   E0 00 66
                    // XEQ J   E0 00 6F
                    // XEQ a   E0 00 7B
                    // XEQ e   E0 00 7F
                }
            },

            {
            "F",new List<RpnPattern>()

            {
                new RpnPattern ( "F1 D5",  2,  "FIX 10" ),
                new RpnPattern ( "F1 D6",  2,  "SCI 10" ),
                new RpnPattern ( "F1 D7",  2,  "ENG 10" ),
                new RpnPattern ( "F1 E5",  2,  "FIX 11" ),
                new RpnPattern ( "F1 E6",  2,  "SCI 11" ),
                new RpnPattern ( "F1 E7",  2,  "ENG 11" ),
                new RpnPattern ( "F2 D0 7([0-4])",  3,  "INPUT ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D0 ([0-7][0-9A-F])",  3,  "INPUT rr",  "reg" ),
                new RpnPattern ( "F2 D1 7([0-4])",  3,  "RCL+ ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D1 ([89A-E][0-9A-F])",  3,  "RCL+ IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D1 F([0-4])",  3,  "RCL+ IND ST `stk`",  "stk" ),//+
                new RpnPattern ( "F2 D1([0-7][0-9A-F])",  3,  "RCL+ rr",  "reg" ),
                new RpnPattern ( "F2 D2 7([0-4])",  3,  "RCL- ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D2 ([89A-E][0-9A-F])",  3,  "RCL- IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D2 F([0-4])",  3,  "RCL- IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D2 ([0-7][0-9A-F])",  3,  "RCL- rr",  "reg" ),
                new RpnPattern ( "F2 D3 7([0-4])",  3,  "RCL× ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D3 ([89A-E][0-9A-F])",  3,  "RCL× IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D3 F([0-4])",  3,  "RCL× IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D3 ([0-7][0-9A-F])",  3,  "RCL× rr",  "reg" ),
                new RpnPattern ( "F2 D4 7([0-4])",  3,  "RCL÷ ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D4 ([89A-E][0-9A-F])",  3,  "RCL÷ IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D4 F([0-4])",  3,  "RCL÷ IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D4 ([0-7][0-9A-F])",  3,  "RCL÷ rr",  "reg" ),
                new RpnPattern ( "F2 D8 ([89A-E][0-9A-F])",  3,  "CLV IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D8 F([0-4])",  3,  "CLV IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 D9 ([89A-E][0-9A-F])",  3,  "PRV IND rr",  "reg-128" ),
                new RpnPattern ( "F2 D9 F([0-4])",  3,  "PRV IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 DA ([89A-E][0-9A-F])",  3,  "INDEX IND rr",  "reg-128" ),
                new RpnPattern ( "F2 DA F([0-4])",  3,  "INDEX IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 E8 ([89A-E][0-9A-F])",  3,  "PGMINT IND rr",  "reg-128" ),
                new RpnPattern ( "F2 E8 F([0-4])",  3,  "PGMINT IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 E9 ([89A-E][0-9A-F])",  3,  "PGMSLV IND rr",  "reg-128" ),
                new RpnPattern ( "F2 E9 F([0-4])",  3,  "PGMSLV IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 EA ([89A-E][0-9A-F])",  3,  "INTEG IND rr",  "reg-128" ),
                new RpnPattern ( "F2 EA F([0-4])",  3,  "INTEG IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 EB ([89A-E][0-9A-F])",  3,  "SOLVE IND rr",  "reg-128" ),
                new RpnPattern ( "F2 EB F([0-4])",  3,  "SOLVE IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 EC ([89A-E][0-9A-F])",  3,  "DIM IND rr",  "reg-128" ),
                new RpnPattern ( "F2 EC F([0-4])",  3,  "DIM IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 ED ([89A-E][0-9A-F])",  3,  "LSTO IND rr",  "reg-128" ),
                new RpnPattern ( "F2 ED F([0-4])",  3,  "LSTO IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 EE ([89A-E][0-9A-F])",  3,  "INPUT IND rr",  "reg-128" ),
                new RpnPattern ( "F2 EE F([0-4])",  3,  "INPUT IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 EF ([89A-E][0-9A-F])",  3,  "EDITN IND rr",  "reg-128" ),
                new RpnPattern ( "F2 EF F([0-4])",  3,  "EDITN IND ST `stk`",  "stk" ),
                new RpnPattern ( "F2 F8 ([89A-E][0-9A-F])",  3,  "VARMENU IND rr",  "reg-128" ),
                new RpnPattern ( "F2 F8 F([0-4])",  3,  "VARMENU IND ST `stk`",  "stk" ),
                new RpnPattern ( "F3 E2 (0[1-9]) ([89A-E][0-9A-F])",  4,  "KEY `key` XEQ IND rr",  "key,reg-128" ),
                new RpnPattern ( "F3 E2 (0[1-9]) F([0-4])",  4,  "KEY `key` XEQ IND ST `stk`",  "key,stk" ),
                new RpnPattern ( "F3 E2 (0[1-9]) ([0-7][0-9A-F])",  4,  "KEY `key` XEQ ll",  "key,lblno" ),
                new RpnPattern ( "F3 E2 (0[1-9]) ([0-7][0-9A-F])",  4,  "KEY `key` XEQ sl",  "key,lblno" ),
                //... XEQ 99  ... 63 dec:16-99; hex:10-63
                ///... XEQ A   ... 66
                //... XEQ J   ... 6F
                //... XEQ a   ... 7B
                //... XEQ e   ... 7F
                new RpnPattern ( "F3 E3 (0[1-9]) ([89A-E][0-9A-F])",  4,  "KEY `key` GTO IND rr",  "key,reg-128" ),
                new RpnPattern ( "F3 E3 (0[1-9]) F([0-4])",  4,  "KEY `key` GTO IND ST `stk`",  "key,stk" ),
                new RpnPattern ( "F3 E3 (0[1-9]) ([0-7][0-9A-F])",  4,  "KEY `key` GTO ll",  "key,lblno" ),
                new RpnPattern ( "F3 E3 (0[1-9]) ([0-7][0-9A-F])",  4,  "KEY `key` GTO sl",  "key,lblno" ),
                new RpnPattern ( "F3 F7 ([0-9A-F][0-9A-F] [0-9A-F][0-9A-F])",  4,  "SIZE ss ss",  "size" ),
                new RpnPattern ( "F([1-9A-F]) 7F",  2,  "⊢`str`",  "strl-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 80",  2,  "VIEW `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 81",  2,  "STO `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 82",  2,  "STO+ `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 83",  2,  "STO- `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 84",  2,  "STO× `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 85",  2,  "STO÷ `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 86",  2,  "X<> `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 87",  2,  "INDEX `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 88",  2,  "VIEW IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 89",  2,  "STO IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8A",  2,  "STO+ IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8B",  2,  "STO- IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8C",  2,  "STO× IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8D",  2,  "STO÷ IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8E",  2,  "X<> IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 8F",  2,  "INDEX IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 90",  2,  "MVAR `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 91",  2,  "RCL `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 92",  2,  "RCL+ `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 93",  2,  "RCL- `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 94",  2,  "RCL× `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 95",  2,  "RCL÷ `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 96",  2,  "ISG `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 97",  2,  "DSE `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 99",  2,  "RCL IND `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) 9A",  2,  "RCL+ IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 9B",  2,  "RCL- IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 9C",  2,  "RCL× IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 9D",  2,  "RCL÷ IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 9E",  2,  "ISG IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) 9F",  2,  "DSE IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) A8",  2,  "SF IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) A9",  2,  "CF IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AA",  2,  "FS?C IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AB",  2,  "FC?C IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AC",  2,  "FS? IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AD",  2,  "FC? IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AE",  2,  "GTO IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) AF",  2,  "XEQ IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) B0",  2,  "CLV `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) B1",  2,  "PRV `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) B2",  2,  "ASTO `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) B3",  2,  "ARCL `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) B4",  2,  "PGMINT `lbl`",  "lbll-1" ),
                new RpnPattern ( "F([1-9A-F]) B5",  2,  "PGMSLV `lbl`",  "lbll-1" ),
                new RpnPattern ( "F([1-9A-F]) B6",  2,  "INTEG `lbl`",  "lbll-1" ),
                new RpnPattern ( "F([1-9A-F]) B7",  2,  "SOLVE `lbl`",  "lbll-1" ),
                new RpnPattern ( "F([1-9A-F]) B8",  2,  "CLV IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) B9",  2,  "PRV IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BA",  2,  "ASTO IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BB",  2,  "ARCL IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BC",  2,  "PGMINT IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BD",  2,  "PGMSLV IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BE",  2,  "INTEG IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) BF",  2,  "SOLVE IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) C0",  2,  "ASSIGN `nam` TO `key`",  "naml-2,+key+1" ), //
                new RpnPattern ( "F([1-9A-F]) C1",  2,  "VARMENU `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) C2 (0[1-9])",  3,  "KEY `key` XEQ `lbl`",  "lbll-2,key" ), //+
                new RpnPattern ( "F([1-9A-F]) C3 (0[1-9])",  3,  "KEY `key` GTO `lbl`",  "lbll-2,key" ),
                new RpnPattern ( "F([1-9A-F]) C4",  2,  "DIM `nam`",  "naml-1" ), //+
                new RpnPattern ( "F([1-9A-F]) C5",  2,  "INPUT `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) C6",  2,  "EDITN `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) C7",  2,  "LSTO `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) C9",  2,  "VARMENU IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) CA (0[1-9])",  3,  "KEY `key` XEQ IND `nam`",  "naml-2,key" ),
                new RpnPattern ( "F([1-9A-F]) CB (0[1-9])",  3,  "KEY `key` GTO IND `nam`",  "naml-2,key" ),
                new RpnPattern ( "F([1-9A-F]) CC",  2,  "DIM IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) CD",  2,  "INPUT IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) CE",  2,  "EDITN IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) CF",  2,  "LSTO IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) DB",  2,  "ΣREG IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) DC",  2,  "FIX IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) DD",  2,  "SCI IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) DE",  2,  "ENG IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) DF",  2,  "TONE IND `nam`",  "naml-1" ),
                new RpnPattern ( "F([1-9A-F]) F0",  2,  "CLP `lbl`",  "lbll-1" ),
                new RpnPattern ( "F([1-9A-F])",  1,  "`str`",  "strl" ) //+ max. length 15
             }
            }
       };

        #endregion 

    }



}
