using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42.encoder
{
    class EncoderResult
    {

         public List<RawProgram> programs = new List<RawProgram>();


        bool  succeeded() 
        {
             bool  succeeded = true;
            foreach (RawProgram prgm in programs)
            {
                succeeded = succeeded && prgm.succeeded();
            }

            return succeeded;
        }


        public CodeError getFirstError()
        {
            CodeError error = null;
            for (int index = 0; index < this.programs.Count; index++)
            {
                RawProgram program = this.programs[index];
                error = program.getFirstError();
            }

            return error;
        }


        string getHex(string eol, bool useWhitespaceBetweenHex, bool useLineNumbers)
        {
            string hexAll = "";
            foreach (RawProgram rprgm in programs)
            {
                hexAll += rprgm.getHex(eol, useLineNumbers);
            }

            if (!useWhitespaceBetweenHex)
            {
                hexAll = hexAll.Replace(" ", "");
            }

            return hexAll.Trim();
        }

        string getRaw() 
        {
         string  raw = "";
            foreach (RawProgram rprgm in programs)
            {
                raw += rprgm.getRaw() + " ";
            }
           
            return raw.Trim();
        }

        public int getSize()
        {
            int  size = 0;
            foreach (RawProgram rprgm in programs)
            {
                size += rprgm.getSize();
            }
          
            return size;
        }
  
    }
}
