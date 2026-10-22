using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DM42.decoder
{
    public class dm42decoder
    {

        List<string> document = null;

        /** Decode raw input to readable code string */
        public decoderresult decode()
        {

            List<string> raw = readDocumentBytes();

            RawParser parser = new RawParser(raw);
            parser.parse();


            // conversion final en RPN source 
            for (int lIdp =0; lIdp < parser.programs.Count; lIdp++)
            {

                for (int lIdR = 0; lIdR < parser.programs[lIdp].rpnLines.Count; lIdR++)
                {

                    RpnLine lLine = parser.programs[lIdp].rpnLines[lIdR]; 
                    Decoder42.toRpn(ref lLine);
                    parser.programs[lIdp].rpnLines[lIdR] = lLine;

                }
                
            }


            // return result
            decoderresult result = new decoderresult();
            result.programs = parser.programs;
            result.languageId = parser.languageId;

            return result;
        }

        public void SetDocument(List<string> doc)
        {
            document = new List<string>(doc); 
        }

        public void SetDocument(string doc)
        {
            document = new List<string>();
            document.Add(doc); 
        }


        /// <summary>
        ///  Formatte une ligne avec l'ajout 1 espace entre octets
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public string formatRawLine(string line)
        {
            return   Regex.Replace(line, @"\B(?=(?:[0-9A-F]{2})+\b)", " "); // place des espaces entre octets si pas d'espace
        }

        /// <summary>
        ///  Mise en forme du document RAW sous forme d'octets dans une liste
        /// </summary>
        /// <returns>Retourne la liste d'octets</returns>
        List<string> readDocumentBytes()
        {
            int docLineCount = document.Count;
            List<string> bytes = new List<string>();
             string content = "";
            List<string> Bytes = new List<string>();


            for (int docLine = 0; docLine < docLineCount; docLine ++)
            {
                string line = document[docLine].ToUpper();
                string linetext = line.Trim(); 
                content += line+' '; 
            }


            string[] lList = content.Trim().Split(' '); // recuperation des octets 
            foreach (string str in lList) Bytes.Add(str); 

            return Bytes;
        }
    }
}
