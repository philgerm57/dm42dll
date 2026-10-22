using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM42.encoder
{
    public class dm42encoder
    {

      public  dm42encoder()
      {
         Encoder42.initialize();
      }

        EncoderResult encode(string languageId, List<string> Rpnlignes)
        {
            RpnParser parser = new RpnParser();
            //!! parser.document = editor.document;
            //!! parser.config = new Configuration(true);

            parser.parse();


            foreach (RawProgram prgm in parser.programs)
            {
                foreach (RawLine rl in prgm.rawLines)
                {
                    Encoder42.toRaw(rl, languageId);
                }

            }

            // return result
            EncoderResult result = new EncoderResult();
            result.programs = parser.programs;

            return result;
        }
             
           

       
    }

}
