using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LDBotV2
{
    public partial class FormError : Form
    {
        private Dictionary<string, CompilerErrorCollection> listError;
        public FormError(Dictionary<string, CompilerErrorCollection> obj0)
        {
            InitializeComponent();
            listError = obj0;
        }

        private void FormError_Load(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, CompilerErrorCollection> keyValuePair in this.listError)
            {
                foreach (CompilerError compilerError in (CollectionBase)keyValuePair.Value)
                {
                    this.listView1.Items.Add(new ListViewItem()
                    {
                        Text = compilerError.FileName,
                        SubItems = {
                            compilerError.ErrorNumber,
                            compilerError.Line.ToString(),
                            compilerError.Column.ToString(),
                            compilerError.ErrorText
                        }
                    });
                }
            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count <= 0)
                return;
            this.richTextBox1.Text = this.listView1.SelectedItems[0].SubItems[4].Text;
        }
    }
}
