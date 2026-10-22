using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42.decoder
{


    /// <summary>
    ///  Class de gestion du retour du decodage 
    /// </summary>
    public class decoderresult
    {
        public List<rpnprogram> programs;
        public string languageId= "";

        bool  succeeded()
        {
            bool lSucceeded = true;
            foreach(rpnprogram prgm in programs)
            {
                lSucceeded = lSucceeded && prgm.succeeded();
            }
            return lSucceeded;

        }

        CodeError getFirstError()
        {
            CodeError lError;

            for (int index = 0; index < this.programs.Count; index++)
            {
                rpnprogram program = this.programs[index];
                lError = program.getFirstError();
                if (lError != null)
                {
                    return lError;
                }

            }
            return null;
        }


        /// <summary>
        ///  retourne le code hexadecimal complet 
        /// </summary>
        /// <param name="eol"></param>
        /// <param name="useWhitespaceBetweenHex"></param>
        /// <param name="useLineNumbers"></param>
        /// <returns></returns>
       public string getHex(string eol, bool useWhitespaceBetweenHex, bool useLineNumbers) 
        {
            string  hexAll = "";

            foreach(rpnprogram rpnprgm in programs)
            {
                hexAll += rpnprgm.getHex(eol, useLineNumbers);
            }

            if (!useWhitespaceBetweenHex)
            {
                //!! regex a utiliser 
                hexAll = hexAll.Replace("/ / g", "");
            }
            return hexAll.Trim();
        }

         // retourne le programme RPN 
        public string getRpn(string eol, bool useLineNumbers) 
        {
            string  rpn = "";
            foreach (rpnprogram prgm in programs)
            {
                rpn += prgm.getRpn(eol, useLineNumbers);
            }
            return rpn.Trim();
        }

        public int getSize()
        {
            int size = 0;
            foreach (rpnprogram prgm in programs)
            {
                size += prgm.getSize();
            }
            return size;
        }
       
    }

}
