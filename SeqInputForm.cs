using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PoleSequenceEditor
{
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
            int _itemHeight = 12;

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Lat Long Input Sheet";

            var mainLayout = new FlowLayoutPanel() { AutoSize = true, FlowDirection = FlowDirection.TopDown };

            var headerLayout = new FlowLayoutPanel() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            
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
            var currSeqLabel = new Label() { Width = _itemWidth, Text = "Current Item #", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
            var poleLabel = new Label() { Width = _itemWidth + 32, Text = "Pole Tag", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
            var newSeqLabel = new Label() { Width = _itemWidth, Text = "New Item #", BorderStyle = BorderStyle.Fixed3D, TextAlign = ContentAlignment.MiddleCenter };
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

                cList.Add(new Label() { Height = _itemHeight,  Width = _itemWidth, Text = seq, TextAlign = ContentAlignment.MiddleCenter });
                pList.Add(new Label() { Height = _itemHeight, Width = _itemWidth + 32, Text = pole, TextAlign = ContentAlignment.MiddleCenter });
                nList.Add(new TextBox() { Height = _itemHeight, Width = _itemWidth, Text = seq });
            }

            for (int i = 0; i < keyList.Count; i++)
            {
                _poleSeqNameTable.Controls.Add(cList[i], 0, i + 1);
                _poleSeqNameTable.Controls.Add(pList[i], 1, i + 1);
                _poleSeqNameTable.Controls.Add(nList[i], 2, i + 1);
            }

            var confirmBtn = new Button() { Text = "Confirm", Dock = DockStyle.Fill };

            var footerLayout = new TableLayoutPanel() { AutoSize = true, Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1 };
            var closeBtn = new Button() { Height = 30, Text = "Close", Dock = DockStyle.Right, DialogResult = DialogResult.Cancel };
            footerLayout.Controls.Add(closeBtn, 3, 0);

            mainLayout.Controls.Add(_poleSeqNameTable);
            mainLayout.Controls.Add(confirmBtn);
            mainLayout.Controls.Add(footerLayout);
            Controls.Add(mainLayout);

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
}
