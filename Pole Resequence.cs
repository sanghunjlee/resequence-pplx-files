using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using PPL_Lib;

public class SeqInputForm : Form
{
    public Dictionary<string, string> NewSeqPoleDict = new Dictionary<string, string>();
    public Dictionary<string, string> ReferenceNewtoOld = new Dictionary<string, string>();
    private TableLayoutPanel _poleSeqNameTable;
    private Dictionary<string, string> _seqFileDict;
    public SeqInputForm(Dictionary<string, string> seqFilePairs)
    {
        _seqFileDict = seqFilePairs;
        InitializeUI();
    }
    private void InitializeUI()
    {
        int _itemWidth = 84;

        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Lat Long Input Sheet";

        FlowLayoutPanel MainLayout = new FlowLayoutPanel() { AutoSize = true, FlowDirection = FlowDirection.TopDown };

        _poleSeqNameTable = new TableLayoutPanel()
        {
            MinimumSize = new Size(324, 384),
            AutoScroll = true,
            Dock = DockStyle.Fill,
            RowCount = 1 + _seqFileDict.Count,
            ColumnCount = 3
        };
        _poleSeqNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1 / 3));
        _poleSeqNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1 / 3));
        _poleSeqNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1 / 3));
        Label currSeqLabel = new Label() { Width = _itemWidth, Text = "Current Item #", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
        Label poleLabel = new Label() { Width = _itemWidth + 32, Text = "Pole Tag", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
        Label newSeqLabel = new Label() { Width = _itemWidth, Text = "New Item #", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
        _poleSeqNameTable.Controls.Add(currSeqLabel, 0, 0);
        _poleSeqNameTable.Controls.Add(poleLabel, 1, 0);
        _poleSeqNameTable.Controls.Add(newSeqLabel, 2, 0);

        List<string> keyList = _seqFileDict.Keys.ToList();
        var cList = new List<Label>();
        var pList = new List<Label>();
        var nList = new List<TextBox>();

        foreach (string key in keyList)
        {
            string seq = key;
            string pole = _seqFileDict[key];

            cList.Add(new Label() { Width = _itemWidth, Text = seq, TextAlign = ContentAlignment.MiddleCenter });
            pList.Add(new Label() { Width = _itemWidth + 32, Text = pole, TextAlign = ContentAlignment.MiddleCenter });
            nList.Add(new TextBox() { Width = _itemWidth, Text = seq });
        }

        for (int i = 0; i < keyList.Count; i++)
        {
            _poleSeqNameTable.Controls.Add(cList[i], 0, i + 1);
            _poleSeqNameTable.Controls.Add(pList[i], 1, i + 1);
            _poleSeqNameTable.Controls.Add(nList[i], 2, i + 1);
        }

        Button confirmBtn = new Button() { Text = "Confirm", Dock = DockStyle.Fill };

        TableLayoutPanel footerLayout = new TableLayoutPanel() { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
        Button closeBtn = new Button() { Height = 30, Text = "Close", Dock = DockStyle.Right, DialogResult = DialogResult.Cancel };
        footerLayout.Controls.Add(closeBtn, 3, 0);

        MainLayout.Controls.Add(_poleSeqNameTable);
        MainLayout.Controls.Add(confirmBtn);
        MainLayout.Controls.Add(footerLayout);
        Controls.Add(MainLayout);

        confirmBtn.Click += new EventHandler(ConfirmChange);
    }

    private void ConfirmChange(Object sender, EventArgs e)
    {
        if (sender is Button)
        {
            string oldSeq;
            string poleTag;
            string newSeq;
            for (int i = 0; i < _seqFileDict.Count; i++)
            {
                Label c = _poleSeqNameTable.GetControlFromPosition(0, i + 1) as Label;
                Label p = _poleSeqNameTable.GetControlFromPosition(1, i + 1) as Label;
                TextBox n = _poleSeqNameTable.GetControlFromPosition(2, i + 1) as TextBox;
                
                oldSeq = c.Text;
                poleTag = p.Text;
                newSeq = i.ToString() + "-" + n.Text;

                NewSeqPoleDict.Add(newSeq, poleTag);
                ReferenceNewtoOld.Add(String.Concat(newSeq, poleTag), String.Concat(oldSeq, poleTag));
            }
            DialogResult = DialogResult.OK;
        }
    }
}

public class Script
{
    public void Execute(PPLMain pPPLMain)
    {
        if (!String.IsNullOrWhiteSpace(pPPLMain.LoadedPolePath))
        {
            pPPLMain.DoSave(false, false);
            string oFolderPath = new DirectoryInfo(pPPLMain.LoadedPolePath).Parent.FullName;
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