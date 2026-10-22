using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42.encoder
{

    /// <summary>
    ///  Classe qui contient l'ensemble des elements d'un programme qui se termine par .END.
    /// </summary>
    public class RawProgram
    {
        /// <summary>
        ///  Ensemble des ligne de code brut ( byte) 
        /// </summary>
        public List<RawLine>  rawLines = new List<RawLine>();
        int   size = 0;
        public int startDocLine  = -1;


        /// <summary>
        ///  initialise la ligne de départ du code 
        /// </summary>
        /// <param name="startDocLine"></param>
        public RawProgram(int startDocLine)
        {
            this.startDocLine = startDocLine;
        }

        public string getRaw() 
        {
            string  rawAll = "";
            foreach (RawLine rl in rawLines)
            {
                rawAll += rl.raw + ' ';
            }
            
        return rawAll.Trim();
        }

        public string   getHex(string eol,bool  useLineNumbers )
        {
            string  hexAll = "";
            foreach (RawLine rl in rawLines)
            {
                hexAll += (useLineNumbers ? (rl.codeLineNo < 10 ? '0' + rl.codeLineNo : rl.codeLineNo) + ": " : "") + rl.raw + eol;
            }
            return hexAll; //no trim() !
         }

        public bool  succeeded()
        {
            return !(getFirstError() != null);
        }

        public CodeError getFirstError()
        {
            List<CodeError> errors = this.getErrors();
            if (errors.Count>0)
            {
                return errors[0];
            }
            return null;
        }

        public List<CodeError> getErrors()
        {
            List<CodeError> errors = new List<CodeError>();

            foreach (RawLine rl in rawLines)
            {
                if (rl.error!= null)
                {
                    // push error ...
                    errors.Add(rl.error);
                }
            }
 
            return errors;
        }

        public int  getSize() 
        {
            // calculate raw program size ...
            // when END = 'C0 00 0D' at the end, ...
            string  rawAll = this.getRaw();
            if (rawAll.Contains("C0 00 0D"))
            {
                // ignore last END, substract 3 bytes
                this.size = Convert.ToByte(rawAll.Length) - 3;
            }
            else
            {
                this.size = Convert.ToByte(rawAll.Length);
            }

            return size;
        }

        void addLine(RawLine rawLine ) {
            this.rawLines.Add(rawLine);
        }


    }
}
