using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace PoleSequenceEditor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            string oFolderPath = Utility.OpenDirectoryDialog();
            string[] oFileList = Directory.GetFiles(oFolderPath, "*.pplx");
            var poleFilepathDict = new Dictionary<string, string>();
            var seqPoleDict = new Dictionary<string, string>();
            foreach (string oFile in oFileList)
            {
                string oFileName = new DirectoryInfo(oFile).Name;
                if (oFileName.Contains(" - "))
                {
                    string[] splitFileName = oFileName.Split('-');
                    string number = splitFileName[0].Contains("P") ? splitFileName[0].Replace(" ", "").TrimStart('P') : splitFileName[0].Replace(" ", "");
                    seqPoleDict[number] = splitFileName[1].Trim();
                    poleFilepathDict[String.Concat(number, splitFileName[1].Trim())] = oFile;
                }
            }
            var sif = new SeqInputForm(seqPoleDict);
            XmlDocument xPoleDoc = new XmlDocument();
            if (sif.ShowDialog() == DialogResult.OK)
            {
                string grid = "";
                foreach (KeyValuePair<string, string> kvp in sif.NewSeqPoleDict)
                {
                    string id = String.Concat(kvp.Key, kvp.Value);
                    string newSeq = kvp.Key.Split('-')[1];
                    string oldSeqPole = sif.ReferenceNewtoOld[id];
                    xPoleDoc.Load(poleFilepathDict[oldSeqPole]);
                    foreach (XmlNode element in xPoleDoc.SelectNodes("//PPL/PPLScene/PPLChildElements/WoodPole/ATTRIBUTES/VALUE"))
                    {
                        foreach (XmlAttribute atr in element.Attributes)
                        {
                            if (atr.Value == "Aux Data 1")
                            {
                                element.InnerText = String.Concat("ITEM # ", newSeq);
                            }
                            if (atr.Value == "Aux Data 2")
                            {
                                grid = element.InnerText;
                            }
                        }
                    }
                    string destination = Path.Combine(oFolderPath, grid);
                    if (!Directory.Exists(destination))
                    {
                        Directory.CreateDirectory(destination);
                    }
                    xPoleDoc.Save(Path.Combine(destination, String.Concat("P", newSeq, " - ", kvp.Value)));
                }
                MessageBox.Show("Resequencing Complete!");
            }
        }
    }
}