using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42
{
    public class CodeError
    {
        int  docLine = -1; //index 0,... but document line numer starts at 1 !!
        int codeLineNo = -1;
        string code;
        string message= "";


        public string Message
        {
            get { return message;  }
        }

        public CodeError(int docLine, int codeLineNo, string code, string message)
        {
            this.docLine = docLine;
            this.codeLineNo = codeLineNo;
            this.code = code;
            this.message = message;

        }

        public  string ToString(bool useHex)
        {
            string  lDocLine = (this.docLine > -1 ? (this.docLine + 1) + ", " : "");
            string  lCodeLine = (this.codeLineNo > -1 ? (useHex ? this.toHex(this.codeLineNo, 8) : (this.codeLineNo < 10 ? "0" + this.codeLineNo.ToString() : this.codeLineNo.ToString())) : "");
            return (
              "Error [" +
              lDocLine +
              lCodeLine +
              "]! Code: '" +
              this.code +
              "'; Message: " +
              this.message
            );

        }

        private string toHex(int  number, int digits)
        {
 
            return number.ToString("X" + digits.ToString());
        }



    }
}
