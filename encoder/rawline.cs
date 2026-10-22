using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DM42; 

namespace DM42.encoder
{
    public class RawLine
    {

        public int  codeLineNo = 0;
        public int docLine = -1;

  /** original code from the document */
        public string docCode =  "";
        public Params parames = new Params();

        public bool ignored = false;
        public string raw ;
        public CodeError error=null ;

        private List<string> _tokens = new List<string>();

        private string _workCode = "";
        private int _tokenlengh;
        private string _token = ""; 


        /** working code */
        public  string workCode
        {

            get { return _workCode; }
            set {
                _workCode = value;
                tokens = _workCode.Split(' ').ToList<string>();
            }
    
         }

        public List<string> tokens
        {
            get {
                return _tokens; 
            }
            set {
                _tokens = value;
                _tokenlengh = _tokens.Count;
                if (_tokenlengh > 0) _token = _tokens[0]; 
            }
        }

        public string token
        {
            get { return _token; }
        }

        public int tokenlengh
        {
            get { return _tokenlengh; }
        }

        public  RawLine()
        {

        }

        public bool hasError()
        {
            return !(this.error == null);
        }

        override public string ToString()
        {
            return (docLine > -1 ? ", " + docLine : "") + (codeLineNo!=0 ? ", " + codeLineNo : "") + this.raw + (error!=null ? ", " + this.error : "");

        }




    }
}
