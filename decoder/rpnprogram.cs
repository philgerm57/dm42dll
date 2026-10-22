using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42.decoder
{

    public class rpnprogram
    {

        public List<RpnLine> rpnLines = new List<RpnLine>();
        public int size;

        public bool succeeded()
        {
                return !(this.getFirstError() != null);
        }

        public CodeError getFirstError()
        {
            List<CodeError> lErrors = this.getError();
            if (lErrors.Count>0)
            {
                return lErrors[0]; 
            }

            return null; 
        }

        public List<CodeError> getError()
        {
            List<CodeError> lListError = new List<CodeError>(); 
            foreach (RpnLine line in rpnLines)
            {
                if (line.error != null)
                {
                    lListError.Add(line.error); 
                }
            }

            return lListError; 
        }

        public string getHex(string eol, bool useLineNumbers)
        {
            string lRawAll = "";
            foreach (RpnLine line in rpnLines)
            {
                lRawAll += (useLineNumbers ? (line.codeLineNo < 10 ? '0' + line.codeLineNo : line.codeLineNo) + ": " : "") + line.docRaw + eol;
            }

            return lRawAll.Trim(); 

        }

        public string getRpn(string eol, bool useLineNumbers)
        {
            string lRpnAll = "";
            foreach (RpnLine line in rpnLines)
            {
                lRpnAll += (useLineNumbers ? (line.codeLineNo < 10 ? '0' + line.codeLineNo : line.codeLineNo) + " " : "") + line.workCode + eol;
            }

            return lRpnAll.Trim();

        }

        public int getSize()
        {
            return this.size;
        }

        public void addLine(RpnLine rpnLine)
        {
            this.rpnLines.Add(rpnLine);
        }


    }
}
