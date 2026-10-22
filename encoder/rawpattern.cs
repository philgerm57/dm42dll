using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DM42.encoder
{
    public class rawpattern
    {

        public string  regex;
        public string raw= "";
        public string parames=""; // like names of regex named groups

        rawpattern()
        {
            regex = ""; //  new Regex(" ");
        }

        public rawpattern(string reg, string raw, string pa = "")
        {
            regex = reg;
            this.raw = raw;
            parames = pa;
        }


    }
}
