using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LDBotV2
{
    public partial class FormLDHardware : Form
    {
        private List<LDEmulator> _ld;
        public FormLDHardware(List<LDEmulator> ld)
        {
            InitializeComponent();
            _ld = ld;
        }

        private void btn_saveLDHardware_Click(object sender, EventArgs e)
        {
            try
            {
                ToolHelper.raiseOnWriteLog(string.Format("CPU: {0} - RAM: {1} - Width: {2} - Height: {3} - DPI: {4}", txt_CPU.Text, txt_RAM.Text, txt_Width.Text, txt_Height.Text, txt_DPI.Text));
                foreach (LDEmulator ld in _ld)
                {
                    if (!ld.isRunning)
                    {
                        string command = string.Concat(new string[]
                        {
                        "modify --index ", ld.Index.ToString(),
                        " --resolution ", txt_Width.Text,",", txt_Height.Text,",", txt_DPI.Text,
                        " --cpu ", txt_CPU.Text, " --memory ", txt_RAM.Text
                        });
                        LDManager.executeLdConsole(command);
                        LDHelper.raiseOnUpdateLDStatus(ld.Index, "Cài đặt phần cứng thành công");
                    }
                    else
                    {
                        LDHelper.raiseOnUpdateLDStatus(ld.Index, "Hãy tắt giả lập trước");
                    }
                }
                
            }
            catch(Exception ex)
            {
                ToolHelper.raiseOnWriteError(ex);
            }
            finally
            {
                Close();
            }
		}
    }
}
