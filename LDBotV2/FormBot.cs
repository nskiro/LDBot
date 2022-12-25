using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KAutoHelper;
using LDBotV2.Core;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace LDBotV2
{
    public partial class FormBot : Form
    {
        public FormBot()
        {
            InitializeComponent();
            Size = new Size(500, Screen.GetBounds(this).Height - 35);
            Location = new Point(Screen.GetBounds(this).Right - this.Width, 0);
            Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ToolHelper.onWriteMainStatus += ((stt) => writeSttMain(stt));
            ToolHelper.onWriteError += ((err) => writeError(err));
            ToolHelper.onWriteLog += ((log) => writeLog(log));
            LDHelper.onUpdateLDStatus += ((ldIndex, stt) => updateLDStatus(ldIndex, stt));
            LDHelper.onLoadListLD += (() => loadEmulatorListView());
            //LDHelper.onGetLDStatus += ((index) => getLDStatus(index));
        }

        private void FormBot_Load(object sender, EventArgs e)
        {
            ToolHelper.checkValidConfigurations();
            ToolHelper.startADBServer();
            loadEmulatorListView();   
        }

        private void writeSttMain(string stt)
        {
            if(InvokeRequired)
            {
                Invoke(new MethodInvoker(() => writeSttMain(stt)));
            }
            else
            {
                stt_main.Text = stt;
            }    
        }
        private void writeLog(string log)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => writeLog(log)));
            }
            else
            {
                rtb_log.SelectionStart = rtb_log.Text.Length;
                rtb_log.SelectionColor = Color.Black;
                rtb_log.AppendText(string.Format("[{0}] {1}{2}", DateTime.Now.ToString("HH:mm:ss"), log.Trim(), Environment.NewLine));
                rtb_log.ScrollToCaret();
            }
        }
        private void writeError(Exception err)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => this.writeError(err)));
            }
            else
            {
                //MessageBox.Show(string.Format("{0}\nTarget: {1}\nType: {2}", err.Message, err.TargetSite?.Name, err.GetType().Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                rtb_error.AppendText(string.Format("[{0}]\n{1}\n=================================\n\n", DateTime.Now.ToString("d/M/y HH:mm:ss"), err.ToString()));
                rtb_error.ScrollToCaret();
                rtb_log.SelectionStart = rtb_log.Text.Length;
                rtb_log.SelectionColor = Color.Red;
                rtb_log.AppendText(string.Format("[{0}] Lỗi: {1}\n", DateTime.Now.ToString("HH:mm:ss"), err.Message));
                rtb_log.ScrollToCaret();
            }
        }
        private void updateLDStatus(int ldIndex, string status)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => updateLDStatus(ldIndex, status)));
            }
            else
            {
                ListViewItem[] listViewItemArray = list_Emulator.Items.Find(ldIndex.ToString(), false);
                if (listViewItemArray.Length != 0)
                {
                    listViewItemArray[0].SubItems[2].Text = status;
                }
            }
        }
        

        private void loadEmulatorListView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => loadEmulatorListView()));
            }
            else
            {
                try
                {
                    LDManager.getAllLD();
                    foreach (LDEmulator ld in LDManager.listEmulator)
                    {
                        if (!this.list_Emulator.Items.ContainsKey(ld.Index.ToString()))
                        {
                            JToken configFileContent = JToken.Parse(File.ReadAllText(string.Format(@"{0}vms\config\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index)));
                            bool isNeedEdit = false;
                            string isRooted = "";
                            if (configFileContent["basicSettings.rootMode"] != null && bool.Parse(configFileContent["basicSettings.rootMode"].ToString()) == true)
                            {
                                isRooted = " (R)";
                            }
                            if (configFileContent["basicSettings.adbDebug"] == null || configFileContent["basicSettings.adbDebug"].ToString() == "0")
                            {
                                configFileContent["basicSettings.adbDebug"] = 1;
                                isNeedEdit = true;
                            }
                            if (configFileContent["basicSettings.rightToolBar"] == null || bool.Parse(configFileContent["basicSettings.rightToolBar"].ToString()) == true)
                            {
                                configFileContent["basicSettings.rightToolBar"] = false;
                                isNeedEdit = true;
                            }
                            if (isNeedEdit)
                            {
                                string rs = configFileContent.ToString();
                                File.WriteAllText(string.Format(@"{0}vms\config\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index), rs);
                            }
                            ListViewItem listViewItem = new ListViewItem(ld.Index.ToString())
                            {
                                Name = ld.Index.ToString(),
                                SubItems = {
                                ld.Name + isRooted,
                                ld.isRunning ? "Đang chạy" : ""
                            },
                                UseItemStyleForSubItems = false
                            };
                            listViewItem.Tag = ld;
                            list_Emulator.Items.Add(listViewItem);
                        }
                    }
                }
                catch (Exception e)
                {
                    writeError(e);
                }
            }
        }

        private void FormBot_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    using (Prompt prompt = new Prompt("Nhập tên cho giả lập mới", "Tạo mới giả lập"))
                    {
                        if (prompt.Result.Length > 0)
                        {
                            LDManager.createLD(prompt.Result);
                        }
                        else
                        {
                            writeError(new Exception("Tên giả lập không được để trống"));
                        }
                    }
                    break;
                case Keys.F6:
                    if (list_Emulator.SelectedItems.Count > 0)
                    {
                        foreach (object selectedLD in list_Emulator.SelectedItems)
                        {
                            LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                            LDManager.loadScript(ld);
                        }
                    }
                    else
                    {
                        writeError(new Exception("Chọn các giả lập muốn load script"));
                    }
                    break;
                case Keys.F7:
                    if (list_Emulator.SelectedItems.Count > 0)
                    {
                        foreach (object selectedLD in list_Emulator.SelectedItems)
                        {
                            LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                            LDManager.startScript(ld);
                        }
                    }
                    else
                    {
                        writeError(new Exception("Chọn các giả lập muốn chạy script"));
                    }
                    break;
                case Keys.F8:
                    if (list_Emulator.SelectedItems.Count > 0)
                    {
                        foreach (object selectedLD in list_Emulator.SelectedItems)
                        {
                            LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                            LDManager.stopScript(ld);
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Đã dừng script");
                        }
                    }
                    else
                    {
                        writeError(new Exception("Chọn các giả lập muốn dừng script"));
                    }
                    break;
            }
        }

        private void nhânBảnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count == 1)
            {
                using (Prompt prompt = new Prompt("Nhập tên cho giả lập mới", "Nhân bản giả lập"))
                {
                    if (prompt.Result.Length > 0)
                    {
                        LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                        LDManager.cloneLD(prompt.Result, ld.Index, ld.Name);
                    }
                    else
                    {
                        writeError(new Exception("Tên giả lập không được để trống"));
                    }
                }
            }
            else
            {
                writeError(new Exception("Chọn một và chỉ một giả lập để nhân bản"));
            }
        }

        private void đổiThôngTinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0) 
            {
                if (MessageBox.Show("Bạn muốn đổi thông tin những giả lập đã chọn?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.changeLDInfo(ld.Index);
                    }
                }
            }
            else
            {
                writeError(new Exception("Chọn các giả lập cần đổi thông tin"));
            }
        }

        private void FormBot_FormClosing(object sender, FormClosingEventArgs e)
        {
            const string message = "Bạn chắc chắn muốn thoát chương trình?";
            const string caption = "Thoát";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                ToolHelper.killADBServer();
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }    
        }

        private void khởiĐộngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    if (!ld.isRunning)
                        LDManager.startLD(ld);
                    else
                        LDHelper.raiseOnUpdateLDStatus(ld.Index, "Giả lập đang chạy");
                }
            }
            else
            {
                writeError(new Exception("Chọn các giả lập muốn khởi động"));
            }
        }

        private void xóaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Bạn muốn xóa các giả lập đã chọn?\nLưu ý: Xóa giả lập sẽ xóa thư mục chứa Script của giả lập", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        if(!ld.isRunning)
                        {
                            LDManager.removeLD(ld);
                            (selectedLD as ListViewItem).Remove();
                        } 
                        else
                        {
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Thoát giả lập trước khi xóa");
                        }    
                        
                    }
                }
            }
            else
            {
                writeError(new Exception("Chọn các giả lập cần xóa"));
            }
        }

        private void khởiĐộngLạiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Bạn muốn khởi động lại các giả lập đã chọn?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        if (ld.isRunning)
                        {
                            LDManager.restartLD(ld);
                        }
                        else
                        {
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Giả lập chưa khởi động");
                        }    
                    }
                }
            }
            else
            {
                writeError(new Exception("Chọn các giả lập cần khởi động lại"));
            }
        }

        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Bạn muốn thoát các giả lập đã chọn?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        if (ld.isRunning)
                        {
                            LDManager.quitLD(ld);
                        }
                        else
                        {
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Giả lập chưa khởi động");
                        }
                    }
                }
            }
            else
            {
                writeError(new Exception("Chọn các giả lập cần thoát"));
            }
        }

        private void mởThưMụcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                ToolHelper.runCMD("explorer.exe", ld.ScriptFolder);
            }
        }

        private void saoChépToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                string[] fileScript = Directory.GetFiles(ld.ScriptFolder, "*.cs");
                StringCollection filePaths = new StringCollection();
                foreach (string file in fileScript)
                {
                    filePaths.Add(file);
                }
                if (fileScript.Length > 0)
                {
                    Clipboard.SetFileDropList(filePaths);
                    writeLog("Chép " + fileScript.Length + " file vào clipboard");
                }
            }
        }

        private void dánToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chép đè script vào các giả lập này?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        ToolHelper.pasteScript(ld);
                    }
                    Clipboard.Clear();
                }
            }
        }

        private void hẹnGiờToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Nhập thời gian bắt đầu script", "Hẹn giờ chạy script", DateTime.Now.ToString()))
            {
                if (prompt.Result != "")
                {
                    if ((DateTime.Now - DateTime.Parse(prompt.Result)).TotalSeconds >= 0)
                    {
                        ToolHelper.raiseOnWriteError(new Exception("Thời gian bắt đầu phải lớn hơn hiện tại!"));
                    }
                    else if (list_Emulator.SelectedItems.Count > 0)
                    {
                        foreach (object selectedLD in list_Emulator.SelectedItems)
                        {
                            LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                            LDManager.scheduleScript(ld, DateTime.Parse(prompt.Result));
                        }
                    }
                }
            }
        }

        private void proxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Nhập thông tin proxy theo định dạng proxy:port", "Proxy"))
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.changeProxy(ld, prompt.Result);
                    }
                }
            }
        }

        private void rootUnRootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    bool isRooted = LDHelper.toggleRoot(ld);
                    ListViewItem[] listViewItemArray = this.list_Emulator.Items.Find(ld.Index.ToString(), false);
                    if (listViewItemArray.Length != 0)
                    {
                        listViewItemArray[0].SubItems[1].Text = isRooted ? ld.Name + " (R)" : ld.Name;
                        if (listViewItemArray[0].SubItems[1].Text.Contains("(R)"))
                        {
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Root OK");
                        }
                        else
                        {
                            LDHelper.raiseOnUpdateLDStatus(ld.Index, "Unroot OK");
                        }
                    }
                }
            }
            
        }

        private void đổiPhầnCứngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                List<LDEmulator> selectedList = new List<LDEmulator>();
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    selectedList.Add(ld);
                }
                new FormLDHardware(selectedList).Show();
            }
        }
    }
}
