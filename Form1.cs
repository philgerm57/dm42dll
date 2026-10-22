using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using DM42.encoder;
using DM42.decoder;
using DM42;


namespace DM42
{
    public partial class Form1 : Form
    {

        decoderresult lResult;  //  retourne le resultat du decodage 

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();  
        }


        private  void AffichePrgm(decoderresult decodeR)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            foreach (rpnprogram prgm in decodeR.programs)
            {
                foreach(RpnLine line in prgm.rpnLines)
                {
                    listBox1.Items.Add(line.codeLineNo + " : " + line.workCode);
                    listBox2.Items.Add(line.codeLineNo + " : " + line.docRaw); 
                }
            }
        }



        private void AffichePrgmEx(decoderresult decodeR)
        {
            string lPrgm=""; 
            foreach (rpnprogram prgm in decodeR.programs)
            {

                lPrgm = prgm.getRpn("\n", true);
                listBox1.Items.Add(lPrgm);

            }

          

        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            // Recupere la ligne 
            int lIdx = ((ListBox)sender).SelectedIndex;
            if (lIdx >= 0)
            {
                try
                {
                    if (listBox1.Items.Count >= lIdx) listBox2.SelectedIndex = lIdx;
                if (listBox4.Items.Count >= lIdx) listBox4.SelectedIndex = lIdx;

                string lLigne = ((ListBox)sender).Items[lIdx].ToString();

                // Extrait le numero de ligne 
                // recupere le numero de programme 

                //!!!!!!
                
                    Params lParam = lResult.programs[0].rpnLines[lIdx].Parames;

                    listBox3.Items.Clear();

                    if (lParam.dig != null) listBox3.Items.Add("Dig: " + lParam.dig);
                    if (lParam.digno != null) listBox3.Items.Add("DigNo: " + lParam.digno);
                    if (lParam.flg != null) listBox3.Items.Add("Flg: " + lParam.flg);
                    if (lParam.flgno != null) listBox3.Items.Add("FlgNo: " + lParam.flgno);
                    if (lParam.key != null) listBox3.Items.Add("Key: " + lParam.key);
                    if (lParam.keyno != null) listBox3.Items.Add("KeyNo: " + lParam.keyno);
                    if (lParam.lbl != null) listBox3.Items.Add("Lbl: " + lParam.lbl);
                    if (lParam.lblno != null) listBox3.Items.Add("LblNo: " + lParam.lbll);
                    if (lParam.nam != null) listBox3.Items.Add("Name: " + lParam.nam);
                    if (lParam.naml != null) listBox3.Items.Add("NameNo: " + lParam.naml);
                    if (lParam.num != null) listBox3.Items.Add("Num: " + lParam.num);
                    if (lParam.numno != null) listBox3.Items.Add("NumNo: " + lParam.numno);

                    if (lParam.reg != null) listBox3.Items.Add("Reg: " + lParam.reg);
                    if (lParam.regno != null) listBox3.Items.Add("RegNo: " + lParam.regno);
                    if (lParam.siz != null) listBox3.Items.Add("Size: " + lParam.siz);
                    if (lParam.sizno != null) listBox3.Items.Add("SizeNo: " + lParam.sizno);
                    if (lParam.stk != null) listBox3.Items.Add("Stk: " + lParam.stk);
                    if (lParam.stkno != null) listBox3.Items.Add("StakNo: " + lParam.stkno);
                    if (lParam.str != null) listBox3.Items.Add("Str:" + lParam.str);
                    if (lParam.strl != null) listBox3.Items.Add("StrLen: " + lParam.strl);

                    if (lParam.ton != null) listBox3.Items.Add("Ton: " + lParam.ton);
                    if (lParam.tonno != null) listBox3.Items.Add("TonNo: " + lParam.tonno);

                }  catch(Exception ex)
                {

                }


                
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open DM42s Raw File";
            //openFileDialog.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "RAW files (*.raw)|*.raw|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                label3.Text = filePath; 

                string RawContent = "";

                using (Stream StreamReader = File.OpenRead(filePath))
                {
                   while(true)
                    {
                        int value = StreamReader.ReadByte();
                        if (value == -1) break;

                        RawContent += string.Format("{0:X2}", value) + " "; 
                    }

                }

                dm42decoder lDecod = new dm42decoder(); 

                RawContent.Trim();
               // lEditor.SetDocument(RawContent);
                Decoder42.initialize();
                lDecod.SetDocument(RawContent); 
                lResult = lDecod.decode();
                int size = lResult.getSize();
                AffichePrgm(lResult);

               // AffichePrgmEx(lResult);

            }

        }



        private void button3_Click(object sender, EventArgs e)
        {
            // Charge un fichier source 
            openFileDialog1.Title = "Open DM42s Source File";
            //openFileDialog.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "DM42 files (*.dm42)|*.dm42|free42 files (*.free42)|*.free42|HP41 files (*.hp41)|*.hp41| All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                listBox4.Items.Clear();

                string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);

                foreach (string line in lines)
                {
                    listBox4.Items.Add(line);
                }

            }

        }

        private void listBox4_Click(object sender, EventArgs e)
        {

            int lIdx = ((ListBox)sender).SelectedIndex;
            if (lIdx >= 0)
            {

               if( listBox1.Items.Count>= lIdx) listBox1.SelectedIndex = lIdx;
               if (listBox2.Items.Count >= lIdx) listBox2.SelectedIndex = lIdx;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Encode 
            lblError.Text = ""; 

            Encoder42.initialize();

            RpnParser Parser = new RpnParser();
            RawLine RL = new RawLine();

          //  string[] lResu= FormattingEdit.FormatDoc(textBox1.Text, false);
          //  textBox1.Text = lResu[1]; 
            RL = Parser.parseLine(0, Encoder42.FormatRpnLine(textBox1.Text));     // ligne de commande ");



            Encoder42.toRaw(RL, "free42");

            textBox2.Text = RL.raw;

            if (RL.hasError()) lblError.Text = "Error"; 


        }

        /// <summary>
        ///  encode programme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            List<string> lDoc = new List<string>();

            // encode programme 
            Encoder42.initialize();

            RpnParser Parser = new RpnParser();
           
            foreach(string   line in listBox4.Items )
            {
                lDoc.Add(Encoder42.FormatRpnLine(line)); 
            }


            Parser.SetDocument(lDoc); 
            Parser.parse();  // encodage 


            listBox2.Items.Clear();

            listBox2.Items.Add(" "); //!! a revoir premiere ligne vide {rttrt}

            for (int lIdx = 0; lIdx < Parser.programs.Count; lIdx++)
            {
                foreach (RawLine line in Parser.programs[lIdx].rawLines)
                {
                    RawLine lLine = line; 
                    Encoder42.toRaw(lLine, "free42");
                    if (lLine.raw != null)
                    {
                        listBox2.Items.Add(lLine.raw);
                    } else
                    {
                        listBox2.Items.Add(lLine.error.ToString());
                    }
                }
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Save raw File 
            string lRawLine;
            string[] strByte; 
            List<byte> lByteList= new List<byte>(); 

            saveFileDialog1.Title = "Save free42 raw File";

            saveFileDialog1.Filter = "RAW file (*.raw)|*.raw";
            saveFileDialog1.FilterIndex = 1;

            saveFileDialog1.DefaultExt = ".raw";
            //saveFileDialog1.InitialDirectory = ""

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                if (saveFileDialog1.FileName != "")
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(saveFileDialog1.FileName, FileMode.Create)))
                    {

                        for (int lIdx = 0; lIdx < listBox2.Items.Count; lIdx++)
                        {
                            lRawLine = listBox2.Items[lIdx].ToString().Trim();
                            if (lRawLine != "")
                            {
                                strByte = lRawLine.Split(' ');

                                if (strByte.Length > 0)
                                {
                                    foreach (string str in strByte)
                                    {
                                        lByteList.Add(byte.Parse(str, System.Globalization.NumberStyles.HexNumber));
                                    }
                                }
                            }

                        }

                        writer.Write(lByteList.ToArray());
                    }

                }
            }


        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Decode une seule ligne RAW octet hexa separés par un espace 

            string RawContent = textBox3.Text.Trim();

            if (!RawContent.Contains(' '))
            {
                RawContent = Regex.Replace(RawContent, @"\B(?=(?:[0-9A-F]{2})+\b)", " "); // place des espaces entre octets 
            }

            RawContent += " C0 00 0D";
            // string[] lStrBytes = raw.Split(' '); 
            // conversion en byte

            dm42decoder lDecod = new dm42decoder();

            lDecod.SetDocument(RawContent); 

            Decoder42.initialize();

            lResult = lDecod.decode();

            textBox4.Text = lResult.programs[0].rpnLines[1].codeLineNo + " : " + lResult.programs[0].rpnLines[1].workCode; 

        }

        private void listBox4_DoubleClick(object sender, EventArgs e)
        {
            listBox4.Items.Clear(); 
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            listBox2.Items.Clear(); 
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            // affiche ou pas les lignes 

          
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string lRpnData = "";
            // format 
            foreach (string line in listBox4.Items)
            {
                lRpnData += line + "\r\n"; 
            }




         string[] lText =  FormattingEdit.FormatDoc(lRpnData, checkBox1.Checked);

            listBox4.Items.Clear();
            
            foreach (String str in lText)
            {
                listBox4.Items.Add(str); 
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Sauvegarde source file 
            saveFileDialog1.Title = "Save DM42s Source File";
            //openFileDialog.InitialDirectory = "c:\\";
            saveFileDialog1.Filter = "Src files (*.dm42)|*.free42|*.free42)|*.dm42| All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog1.FileName;

                label3.Text = filePath;

                string SrcContent = "";
                System.IO.StreamWriter SaveFile = new StreamWriter(filePath);
                foreach (var item in listBox4.Items)
                {
                    SaveFile.WriteLine(item.ToString());
                }
                SaveFile.Close();
                MessageBox.Show("Programs saved!");



                // AffichePrgmEx(lResult);

            }

        }
    }


}
