using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace DM42.decoder
{
    class RawParser
    {
        //#region Members

          public  int  debug = 0; // debug level 0=nothing, 1=minimal, 2=verbose
          public   List<rpnprogram> programs = new List<rpnprogram>(); 
          public string languageId = "hp42s";

          private List<string> raw= new List<string>();
          private int codeLineNo= 0;
          private string number = "";
          private bool readingNumber = false;
         

        //#endregion

        //#region Public

        public RawParser(List<string> raw)
        {
            this.raw = raw;
            // only one program used
            rpnprogram lProgram = new rpnprogram();
            this.programs.Add(lProgram); 

        }


        /// <summary>
        ///  Traitement principal d'analyse du fichier brut 
        /// </summary>
        public void parse()
        {
            //index walking through byte array
            int index = 0;
            string end="";
            string entete = "";
            int lLastIndex = 0; 

            while (index < this.raw.Count)
            {
                int length = 0;
                string  b0 = this.raw[index];
                Regex regex= new Regex("1[0-9A-C]");

                if (regex.Match(b0).Success || this.readingNumber)
                {
                    //this.readingNumber || /1[0-9A-C]/.test(b0) || (/00/.test(b0) && this.readingNumber)
                    length = this.parseNumber(index);
                }
                else
                {
                    length = (int)this.parseCommand(index);
                }

                // no match !
                if (length == 0)
                {
                    // abort !
                    break;
                }

                index = index + length;
                if (index >= 3)    end = this.raw[index - 3] + " " + this.raw[index - 2] + " " + this.raw[index - 1];

                if (end == "C0 00 0D") // octets marquant la fin du programme 
                {
                    // fin du programme 
                    this.programs[programs.Count - 1].size = index - 3;

                    // Ajoute la ligne 00
                    entete = "{ " + (index - lLastIndex-3).ToString() + " Bytes Prgm }";
                    RpnLine lEnteteLine = new RpnLine();
                    lEnteteLine.codeLineNo = 0;
                    lEnteteLine.workCode = entete;
                    this.programs[programs.Count - 1].rpnLines.Insert(0, lEnteteLine); 
                    lLastIndex = index;

                    // creation d'un nouveau programme 
                    rpnprogram lProgram = new rpnprogram();
                    this.programs.Add(lProgram);
                    codeLineNo = 0; // reset le compteur 
                }


            }

            int size = this.raw.Count; 
            if (size >= 3)
            {
                 end = this.raw[size - 3] + " " + this.raw[size - 2] + " " + this.raw[size - 1];
                if (end == "C0 00 0D") // octets marquant la fin du programme 
                {
                    size -= 3;
                }
            }

            this.programs[programs.Count-1].size = size;
        }


        #region Private Methods

        /// <summary>
        ///  Decodage des nombres 
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private  string DecodeNumber(string raw)
        {
            string[] lBytes = raw.Split(' ');
            string lValue = ""; 

            foreach (string value in lBytes)
            {
                if (value == "1C") { lValue += "-"; continue; }
                if (value == "1A") { lValue += "."; continue; }
                if (value == "1B") { lValue += "E"; continue; }
                if (value[0]=='1')  lValue += value[1]; 
            }
            return lValue;

        }


        /// <summary>
        ///  Traitement d'un pattern numérique 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private  int parseNumber(int index) 
        {
            string  b0 = this.raw[index];
            string StrNumber = ""; 

            // number detected
            if (!this.readingNumber) {
              this.readingNumber = true;
              this.number = "";
            }

            //put it together
             Match lMatch = Regex.Match(b0, "(1[0-9A-C]|00)");
            if (lMatch.Success) {
              this.number += b0 + " ";

                //end of number ?
             
              if (Regex.Match(b0, "00").Success) {
                this.readingNumber = false;

                    // create new line
                RpnLine rpnLine = new RpnLine();
                rpnLine.docRaw = this.number.Trim();
                rpnLine.rawLength = this.number.Length / 3; //'00 '
                rpnLine.workCode = "`num`";
   
                // Decoder42.convertRawToNumber() attend les octets bruts ("11 10 10 00") et fait lui-même
                // la conversion. Lui passer le nombre déjà décodé ("100") le décodait deux fois :
                // "00$" supprimait les zéros de fin (100 -> 1) et "1([0-9])" mangeait les "10" (1E-10 -> 1E-0).
                rpnLine.Parames.num = rpnLine.docRaw;

                // collect rawLines
                this.pushLine(rpnLine);
            }
            } else {
              this.readingNumber = false;

                // create new line
               RpnLine rpnLine = new RpnLine();
              rpnLine.docRaw = this.number;
              rpnLine.rawLength = this.number.Length / 3; //'00 '
              rpnLine.error = new CodeError(-1, index, b0 + " ...", "Unknown byte sequence for number");
              rpnLine.Parames.num = this.number.Trim();

              // collect rawLines
              this.pushLine(rpnLine);
            }

            return 1;
        }


        /// <summary>
        ///   Traitement d'un pattern de commande 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private  int? parseCommand(int index)
        {
            // new temp maps
            string b0 = this.raw[index];
            string n0 = b0[0].ToString(); // recupere le premier chiffre de l'octet  ('0..F')

            // create new line
            RpnLine rpnLine = new RpnLine();
            string hex = "";

            // recupere la liste des expressions possible  et la taille de la commande 
            if (Decoder42.rawMap.ContainsKey(n0))
            {
                //recupere tous les patterns possibles pour la lettre de commande 
                List<RpnPattern> patterns = Decoder42.rawMap[n0];

                if (patterns.Count > 0)
                {
                    //params
                    bool matched = false;

                    // scan tous les patterns  pour rechercher le motif d'octets
                    for (int i = 0; i < patterns.Count; i++)
                    {
                        // recupe le pattern  de recherche 
                        RpnPattern pattern = patterns[i];

                        //recupers les bytes de la liste brute raw en fonction de la longueur du pattern 
                        hex = "";
                        for (int j = 0; j < pattern.len; j++)
                        {
                            hex += this.raw[index + j] + ' ';
                        }

                        hex = hex.Trim();

                        //hp42s/free42 ?
                        this.checkLanguageId(pattern, hex);

                        //match ?

                        Match lMatch = pattern.regex.Match(hex);

                        if (lMatch.Length > 0)
                        {

                            rpnLine.rawLength = pattern.len; // longueur du pattern 
                                                             // offset
                            int next = index + pattern.len; //  inc l'index de pointage dans la table RAW 

                            // test si parametre disponible pour la commande 
                            this.checkParamsInMatch(ref rpnLine, next, pattern, lMatch);

                            // test si la chaine parametre ne déborde pas ? 
                            if (this.checkParseLength(rpnLine, next))
                            {
                                break; // débordement 
                            }

                            // check if str/lbl/name found and adjust length ...
                            rpnLine.rawLength += this.readStringParams(ref rpnLine, next);

                            // increase length for
                            // 'ASSIGN `nam` TO key'
                            // 'KEY `key` GTO (|IND) `nam`'
                            // 'KEY `key` XEQ (|IND) `nam`'

                            // get all raw bytes of this code line
                            hex += ' ';
                            for (int j = pattern.len; j < rpnLine.rawLength; j++)
                            {
                                hex += this.raw[index + j] + ' ';
                            }

                            hex = hex.Trim();

                            // fill RpnLine
                            rpnLine.docRaw = hex;

                            rpnLine.workCode = pattern.rpn; 

                            // collect rawLines
                            this.pushLine(rpnLine);

                            // someting matched successfully
                            matched = true;

                            break;
                        }
                    } // for 

                    // no match, then error ...
                    if (!matched)
                    {
                        rpnLine.docRaw = b0;
                        rpnLine.error = new CodeError(-1, index, b0 + " ...", "Unknown byte sequence");
                        this.pushLine(rpnLine);
                    }
                }
            }
            else
            {
                // unknown nibble
                rpnLine.docRaw = hex;
                rpnLine.error = new CodeError(-1, index, b0 + " ...", "Unknown byte sequence");
                this.pushLine(rpnLine);
            }

            return rpnLine.rawLength;
        }


        private void checkParamsInMatch(ref RpnLine rpnLine, int next, RpnPattern pattern, Match match)
        {
            string[] lparams;
           
            if (pattern.Params != "")
            {

                lparams = pattern.Params.Split(','); //OK 
                // assign params like regex named groups
                for (int p = 0; p < lparams.Length; p++)
                {
                    string param = lparams[p];
                    int k = p + 1; 

                    string lLen = match.Groups[k].Value; 

                    switch (param)
                    {
                        case ("strl-2"):

                            rpnLine.Parames.strl = Convert.ToInt16(lLen, 16) - 2;
                            break;
                        case "strl-1":
                            rpnLine.Parames.strl = Convert.ToInt16(lLen, 16) - 1;
                            break;
                        case "strl":
                            rpnLine.Parames.strl = Convert.ToInt16(lLen, 16);
                            break;

                        case "lblno":
                            rpnLine.Parames.lblno = Convert.ToInt16(lLen, 16);
                            break;
                        case "lblno-1":
                            rpnLine.Parames.lblno = Convert.ToInt16(lLen, 16) - 1;
                            break;

                        case "lbll-2":
                            rpnLine.Parames.lbll = Convert.ToInt16(lLen, 16) - 2;
                            break;
                        case "lbll-1":
                            rpnLine.Parames.lbll = Convert.ToInt16(lLen, 16) - 1;
                            break;
                        case "lbll":
                            rpnLine.Parames.lbll = Convert.ToInt16(lLen, 16);
                            break;
                        case "naml-2":
                            rpnLine.Parames.naml = Convert.ToInt16(lLen, 16) - 2;
                            break;
                        case "naml-1":
                            rpnLine.Parames.naml = Convert.ToInt16(lLen, 16) - 1;
                            break;
                        case "naml":
                            rpnLine.Parames.naml = Convert.ToInt16(lLen, 16);
                            break;
                        case "reg-128":
                            rpnLine.Parames.reg = lLen;
                            rpnLine.Parames.regno = Convert.ToInt16(lLen, 16) - 128;
                            break;
                        case "reg":
                            rpnLine.Parames.reg = lLen;
                            rpnLine.Parames.regno = Convert.ToInt16(lLen, 16);
                            break;
                        case "stk":
                            rpnLine.Parames.stkno = Convert.ToInt16(lLen, 16);
                            if (Decoder42.stackMap.ContainsKey((int)rpnLine.Parames.stkno))
                            {
                                rpnLine.Parames.stk = Decoder42.stackMap[(int)rpnLine.Parames.stkno];
                            }
                            break;
                        case "dig":
                            rpnLine.Parames.dig = lLen;
                            rpnLine.Parames.digno = Convert.ToInt16(lLen, 16);
                            break;
                        case "flg":
                            rpnLine.Parames.flg = lLen;
                            rpnLine.Parames.flgno = Convert.ToInt16(lLen, 16);
                            break;
                        case "+key+1":
                            // fetch key-byte after name
                            //!! a verifier 
                            rpnLine.Parames.key = rpnLine.Parames.naml != null ? this.raw[next + (int)rpnLine.Parames.naml] : null;
                            rpnLine.Parames.keyno = rpnLine.Parames.naml != null ? (int?)Convert.ToInt16(this.raw[next + (int)rpnLine.Parames.naml], 16) + 1 : null;
                            // include key byte
                            rpnLine.rawLength++;
                            break;
                        case "key":
                            {
                                rpnLine.Parames.key = lLen;
                                rpnLine.Parames.keyno = Convert.ToInt16(lLen, 16);

                            }
                            break;

                        case "size":
                            {

                                rpnLine.Parames.siz = lLen;
                               // rpnLine.params.sizno = parseInt(match[k].replace(" ", ""), 16);
                                rpnLine.Parames.sizno = Convert.ToInt16(lLen.Replace(" ",""), 16);
                                break;
                            }
                        case "ton":
                            rpnLine.Parames.ton = lLen;
                            rpnLine.Parames.tonno = Convert.ToInt16(lLen, 16);
                            break;
                        default:
                            break;
                    }

                }
            }
        }


        // contrôle de la longueur de la ligne qui ne doit pas depasser la longueur du fichier 
        private bool checkParseLength( RpnLine rpnLine , int next) 
        {
            int  rawlength = this.raw.Count;
            if (rpnLine.Parames.strl != null  && next + rpnLine.Parames.strl >= rawlength) {
              return true;
            }
            if (rpnLine.Parames.lbll != null && next + rpnLine.Parames.lbll >= rawlength) {
              return true;
            }
            if (rpnLine.Parames.naml != null && next + rpnLine.Parames.naml >= rawlength) {
              return true;
            }

            return false;
  }

    /** Check if str/lbl/nam found and adjust length */
         private int? readStringParams(ref RpnLine rpnLine , int next)
        {
            if (rpnLine.Parames.strl != null) {
                rpnLine.Parames.str = "";
                // where the string starts ...
                for (int j = 0; j < rpnLine.Parames.strl; j++) {
                    rpnLine.Parames.str += this.raw[next + j] + " ";
                }
                rpnLine.Parames.str = rpnLine.Parames.str.Trim();
                return rpnLine.Parames.strl;
            }
            if (rpnLine.Parames.lbll != null) {
                rpnLine.Parames.lbl = "";
                // where the string starts ...
                for (int j = 0; j < rpnLine.Parames.lbll; j++) {
                    rpnLine.Parames.lbl += this.raw[next + j] + " ";
                }
                rpnLine.Parames.lbl = rpnLine.Parames.lbl.Trim();
                return rpnLine.Parames.lbll;
            }
            if (rpnLine.Parames.naml != null) {
                rpnLine.Parames.nam = "";
                // where the string starts ...
                for (int j = 0; j < rpnLine.Parames.naml; j++) {
                    rpnLine.Parames.nam += this.raw[next + j] + " ";
                }
                rpnLine.Parames.nam = rpnLine.Parames.nam.Trim();
                return rpnLine.Parames.naml;
            }

            return 0;
        }


        // contrôle du langage  free42 ou DM42 
        // Free42 ayant plus de commande 
        private void checkLanguageId(RpnPattern pattern, string hex)
        {
            if (pattern.len == 2)
            {
                //free42 extension commands:
                //ACCEL,LOCAT,HEADING,ADATE,ATIME24,CLK12,CLK24,DATE,DATE+,DDAYS,DMY,DOW,MDY,TIME,YMD,BSIGNED,BWRAP,BRESET,LSTO (4x)
                string  free42Ext = "(A7 CF|A7 D0|A7 D1|A6 81|A6 84|A6 85|A6 86|A6 87|A6 8C|A6 8D|A6 8E|A6 8F|A6 90|A6 91|A6 9C|A7 D5|A7 D6|A7 D7|A7 D8|F2 ED([89A-E][0-9A-F])|F2 ED F([0-4])|F([1-9A-F]) C7|F([1-9A-F]) CF)";
                bool  isFree42 = Regex.Match(hex, free42Ext).Success;
                if (isFree42)
                {
                    this.languageId = "free42";
                }
            }
        }


        // enregistre la  ligne dans le programme 
        private void pushLine(RpnLine rpnLine )
        {
            this.codeLineNo++;
            rpnLine.codeLineNo = this.codeLineNo;

            if (this.debug > 0)
            {
                //console.log(rpnLine.docRaw + ' -> ' + rpnLine.workCode);
            }

            this.programs[programs.Count-1].addLine(rpnLine);
        }

        #endregion 

    }
}
