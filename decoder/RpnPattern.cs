using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

namespace DM42.decoder
{
    public class RpnPattern
    {

        public Regex regex ;
        public int len = 0;
        public string rpn= "";
        public string Params=""; // like names of regex named groups

        public RpnPattern()
        {
            this.regex = new Regex(" ");
        }

        public RpnPattern(string reg, int len, string rpn, string pa = "")
        {
            this.regex = new Regex(reg);
            this.len = len;
            this.rpn = rpn;
            this.Params = pa;


        }

    }
}
