using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DM42;

namespace DM42.decoder
{
    public class RpnLine
    {

        public int codeLineNo = 0;
        public string docRaw = "";

        /** working code */
        public string workCode="";
        public CodeError error= null ;
        public int? rawLength = 0;
        public Params Parames= new Params() ;
        
        public bool hasError()
        {
            return (this.error != null); 
        }

        public override string ToString()
        {
            return this.codeLineNo.ToString() + ":" + this.docRaw + ", " + this.workCode + (this.error!=null ? ", " + this.error : ""); 
        }


    }
}
