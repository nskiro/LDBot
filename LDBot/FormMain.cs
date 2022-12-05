using System;
using System.Configuration;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KAutoHelper;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace LDBot
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public FormMain()
        {
            InitializeComponent();
            this.Location = new Point(Screen.GetBounds(this).Right - this.Width, 0);
            this.Size = new Size(445, Screen.GetBounds(this).Height - 35);
            Helper.onUpdateMainStatus += ((stt) => updateStatus(stt));
            Helper.onErrorMessage += ((err) => showError(err));
            Helper.onUpdateLDStatus += ((ldIndex, stt) => updateLDStatus(ldIndex, stt));
            Helper.onWriteLog += ((log) => writeLog(log));
            Helper.onLoadListLD += (() => loadEmulatorListView());
            Helper.onGetLDStatus += ((index) => getLDStatus(index));
        }

        private void loadConfig()
        {
            lbl_BrowseLDFolder.Text = ConfigurationManager.AppSettings["LDPath"];
            ADBHelper.SetADBFolderPath(ConfigurationManager.AppSettings["LDPath"]);
            txt_DefaultLDWidth.Value = ConfigurationManager.AppSettings["DefaultWidth"] != null ? Decimal.Parse(ConfigurationManager.AppSettings["DefaultWidth"]) : 320;
            txt_DefaultLDHeight.Value = ConfigurationManager.AppSettings["DefaultHeight"] != null ? Decimal.Parse(ConfigurationManager.AppSettings["DefaultHeight"]) : 480;
        }

        private void updateStatus(string stt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => this.updateStatus(stt)));
            }
            else
            {
                if (stt.Length > 0)
                    stt_main.Text = stt;
            }
        }

        private void showError(Exception err)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => this.showError(err)));
            }
            else
            {
                    MessageBox.Show(string.Format("{0}\nTarget: {1}\nType: {2}", err.Message, err.TargetSite?.Name, err.GetType().Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void updateLDStatus(int ldIndex, string status)
        {
            if(this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => updateLDStatus(ldIndex, status)));
            }    
            else
            {
                ListViewItem[] listViewItemArray = this.list_Emulator.Items.Find(ldIndex.ToString(), false);
                if(listViewItemArray.Length != 0)
                {
                    listViewItemArray[0].SubItems[2].Text = status;
                }    
            }    
        }

        private string getLDStatus(int ldIndex)
        {
            if (this.InvokeRequired)
            {
                return (string)this.Invoke(new MethodInvoker(() => getLDStatus(ldIndex)));
            }
            else
            {
                ListViewItem[] listViewItemArray = this.list_Emulator.Items.Find(ldIndex.ToString(), false);
                if (listViewItemArray.Length != 0)
                {
                    return listViewItemArray[0].SubItems[2].Text.Trim();
                }
                return "";
            }
        }

        private void writeLog(string log)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => writeLog(log)));
            }
            else
            {
                rtb_log.AppendText(string.Format("[{0}]{1}{2}", DateTime.Now.ToString("HH:mm:ss"), log.Trim(), Environment.NewLine));
                rtb_log.ScrollToCaret();
            }    
        }

        private void pasteScript(LDEmulator ld)
        {
            if(Clipboard.ContainsFileDropList())
            {
                StringCollection copiedScripts = Clipboard.GetFileDropList();
                foreach(string script in copiedScripts)
                {
                    File.Copy(script, ld.ScriptFolder + "\\" + Helper.getFileNameByPath(script), true);
                    writeLog(string.Format("Copy {0} to {1}", Helper.getFileNameByPath(script), ld.ScriptFolder));
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
                            JToken configFileContent = JToken.Parse(File.ReadAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index)));
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
                            //if (configFileContent["advancedSettings.resolution"] == null || configFileContent["advancedSettings.resolution"]["width"].ToString() != ConfigurationManager.AppSettings["DefaultWidth"])
                            //{
                            //    configFileContent["advancedSettings.resolution"] = JToken.Parse(string.Format("{{ \"width\": {0}, \"height\": {1} }}", ConfigurationManager.AppSettings["DefaultWidth"], ConfigurationManager.AppSettings["DefaultHeight"]));
                            //    configFileContent["advancedSettings.resolutionDpi"] = 120;
                            //    //configFileContent["basicSettings.width"] = -1;
                            //    //configFileContent["basicSettings.height"] = -1;
                            //    //configFileContent["basicSettings.realHeigh"] = -1;
                            //    //configFileContent["basicSettings.realWidth"] = -1;
                            //    isNeedEdit = true;
                            //}
                            if (configFileContent["basicSettings.rightToolBar"] == null || bool.Parse(configFileContent["basicSettings.rightToolBar"].ToString()) == true)
                            {
                                configFileContent["basicSettings.rightToolBar"] = false;
                                isNeedEdit = true;
                            }
                            if (isNeedEdit)
                            {
                                string rs = configFileContent.ToString();
                                File.WriteAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index), rs);
                            }
                            ListViewItem listViewItem = new ListViewItem(ld.Index.ToString())
                            {
                                Name = ld.Index.ToString(),
                                SubItems = {
                                ld.Name + isRooted,
                                ld.isRunning ? "Running" : "Stop"
                            },
                                UseItemStyleForSubItems = false
                            };
                            listViewItem.Tag = ld;
                            this.list_Emulator.Items.Add(listViewItem);
                        }
                    }
                }
                catch (Exception e)
                {
                    showError(e);
                }
            }
        }

        private void lbl_BrowseLDFolder_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                lbl_BrowseLDFolder.Text = folderBrowserDialog1.SelectedPath;
                if (!File.Exists(lbl_BrowseLDFolder.Text + "\\ldconsole.exe"))
                {
                    showError(new Exception("Can not find ldconsole.exe. Please browse LD Player again!"));
                    btn_SaveGeneralConfig.Enabled = false;
                }
                else
                {
                    updateStatus("File ldconsole.exe found.");
                    btn_SaveGeneralConfig.Enabled = true;
                }
            }    
        }

        private void btn_SaveGeneralConfig_Click(object sender, EventArgs e)
        {
            if (lbl_BrowseLDFolder.Text.Length > 0)
            {
                Helper.AddOrUpdateAppSettings("LDPath", lbl_BrowseLDFolder.Text);
                ADBHelper.SetADBFolderPath(lbl_BrowseLDFolder.Text);
            }
            if(txt_DefaultLDWidth.Value > 0)
                Helper.AddOrUpdateAppSettings("DefaultWidth", txt_DefaultLDWidth.Value.ToString());
            if (txt_DefaultLDHeight.Value > 0)
                Helper.AddOrUpdateAppSettings("DefaultHeight", txt_DefaultLDHeight.Value.ToString());

            MessageBox.Show("Please re-open the application to load new configuration!", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void createNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Input new LD Player name", "Create New LDPlayer"))
            {
                if (prompt.Result.Length > 0)
                {
                    LDManager.createLD(prompt.Result);
                    loadEmulatorListView();
                }
            }
            
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();
            loadConfig();
            loadEmulatorListView();
            if(!Directory.Exists(ConfigurationManager.AppSettings["LDPath"] + "\\Scripts"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["LDPath"] + "\\Scripts");
            }    
        }

        private void featuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ft = File.ReadAllText("DATA\\features.txt");
            MessageBox.Show(ft, "Features",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private async void runSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    new Task(delegate
                    {
                        LDManager.runLD(ld);
                    }).Start();
                    await Task.Delay(5000);
                }
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void quitAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LDManager.quitAll();
        }

        private void list_Emulator_DoubleClick(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                SetForegroundWindow(ld.TopHandle);
            }
        }

        private void list_Emulator_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                var selectedItem = list_Emulator.FocusedItem;
                if(selectedItem != null && selectedItem.Bounds.Contains(e.Location))
                {
                    listLDContextMenuStrip1.Show(Cursor.Position);
                }    
            }    
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                new Task(delegate
                {
                    LDManager.runLD(ld);
                }).Start();
            }
        }

        private void rebootToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                LDManager.restartLD(ld);
            }
        }

        private void closeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                LDManager.quitLD(ld.Index);
                ld.isRunning = false;
            }
        }

        private void cloneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                using (Prompt prompt = new Prompt("Input new LD Player name", "Clone LDPlayer"))
                {
                    if (prompt.Result.Length > 0)
                    {
                        LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                        LDManager.cloneLD(prompt.Result, ld.Index, ld.Name);
                        loadEmulatorListView();
                    }
                }
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void changeInfoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                LDManager.changeLDInfo(ld.Index);
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this player?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                    LDManager.removeLD(ld);
                    list_Emulator.SelectedItems[0].Remove();
                }
                else
                {
                    showError(new Exception("Select source player first!"));
                }
            }
        }

        private void loadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                LDManager.loadScript(ld);
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void startScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                ld.botAction.isRunning = true;
                LDManager.startScript(ld);
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void loadScriptSelectedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    LDManager.loadScript(ld);
                }
            }
        }

        private void startScriptSelectedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    LDManager.startScript(ld);
                }
            }
        }

        private void list_Emulator_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                if (ld != null)
                    updateStatus(ld.TopHandle.ToString());
            }
            catch{ }
        }

        private void stopScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                LDManager.stopScript(ld);
            }
            else
            {
                showError(new Exception("Select source player first!"));
            }
        }

        private void stopScriptSelectedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    LDManager.stopScript(ld);
                }
            }
        }

        private void installAPKSelectedsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Select APK file to install", "Install APK"))
            {
                if (prompt.Result.Length > 0)
                {
                    if (list_Emulator.SelectedItems.Count > 0)
                    {
                        foreach (object selectedLD in list_Emulator.SelectedItems)
                        {
                            LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                            LDManager.installAPK(ld.Index, prompt.Result);
                        }
                    }
                }
            }
        }

        private void installAPKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Select APK file to install", "Install APK","Drag and drop APK here to install"))
            {
                if (prompt.Result.Length > 0)
                {
                    if (list_Emulator.SelectedItems.Count > 0)
                    {
                        LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                        LDManager.installAPK(ld.Index, prompt.Result);
                    }
                }
            }
        }

        private void changeProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Input HTTP proxy info. Ex: proxy:port", "Change Proxy"))
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                    LDManager.changeProxy(ld, prompt.Result);
                }
            }
        }

        private void changeProxyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Input HTTP proxy info. Ex: proxy:port", "Change Proxy"))
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

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                Helper.runCMD("explorer.exe", ld.ScriptFolder);
            }
        }

        private void changeHardwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                new FormLDHardware(new List<LDEmulator>{ld}).Show();
            }
            
        }

        private void deletesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete selected LDs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.removeLD(ld);
                        ((ListViewItem)selectedLD).Remove();
                    }
                }
            }
        }

        private void rebootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to reboot selected LDs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.restartLD(ld);
                    }
                }
            }
        }

        private void closesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to shutdown selected LDs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.quitLD(ld.Index);
                    }
                }
            }
        }

        private void changeInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to change phone model of selected LDs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        LDManager.changeLDInfo(ld.Index);
                    }
                }
            }
        }

        private void changeHardwareToolStripMenuItem1_Click(object sender, EventArgs e)
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

        private void captureGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                new FormGuideCapture(ld).Show();
            }
            
        }

        private void changeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string changeLog = string.Format("1.1.4::\n-(New) Root/Unroot emulators with one click.\n-(Update) getInstalledPackages(bool isShowDebug = false)\n-(Update) getCurrentIP()\n-(Update) Emulator list shows root/unroot. Emulator name suffix by (R) means Rooted.\n-(Fixed) Bug changeProxy() in script.\n\n1.1.3:\n- (New) Schedule a timer to run the script.\n- (Fixed) Remove sort emulator when start/reboot LD.\n- (Fixed) \"Stop script\" works more stable.\n\n1.1.2:\n- (New) Copy/Paste script from LD to other LD.\n\n1.1.1:\n- (Fixed) List view bug when create/clone/delete LD.\n- (Fixed) Delete script directory when LD deleted.\n\n1.1:\n- (New) Capture guide.");
            MessageBox.Show(changeLog,"Change Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void copyScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                string[] fileScript = Directory.GetFiles(ld.ScriptFolder, "*.cs");
                StringCollection filePaths = new StringCollection();
                foreach(string file in fileScript)
                {
                    filePaths.Add(file);
                }    
                if(fileScript.Length > 0)
                {
                    Clipboard.SetFileDropList(filePaths);
                    writeLog("Copy " + fileScript.Length + " script file to clipboard");
                }    
            }
        }

        private void pasteScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to overwrite script of selected LD?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                    pasteScript(ld);
                    Clipboard.Clear();
                }
            }
        }

        private void pasteScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to overwrite script of selected LDs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (list_Emulator.SelectedItems.Count > 0)
                {
                    foreach (object selectedLD in list_Emulator.SelectedItems)
                    {
                        LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                        pasteScript(ld);
                    }
                    Clipboard.Clear();
                }
            }
        }

        private void scheduleScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Enter the time to run the script", "Schedule Script", DateTime.Now.ToString()))
            {
                if (prompt.Result != "")
                {
                    if ((DateTime.Now - DateTime.Parse(prompt.Result)).TotalSeconds >= 0)
                    {
                        MessageBox.Show("Schedule time must be greater than current time!","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (list_Emulator.SelectedItems.Count > 0)
                    {
                        LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                        LDManager.scheduleScript(ld, DateTime.Parse(prompt.Result));
                    }
                }
            }
        }

        private void scheduleScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Enter the time to run the script", "Schedule Script", DateTime.Now.ToString()))
            {
                if (prompt.Result != "")
                {
                    if ((DateTime.Now - DateTime.Parse(prompt.Result)).TotalSeconds >= 0)
                    {
                        MessageBox.Show("Schedule time must be greater than current time!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void toggleRoot(LDEmulator ld)
        {
            if (ld.isRunning)
            {
                LDManager.quitLD(ld.Index);
            }
            JToken configFileContent = JToken.Parse(File.ReadAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index)));
            configFileContent["basicSettings.rootMode"] = (configFileContent["basicSettings.rootMode"] == null) ? true : !bool.Parse(configFileContent["basicSettings.rootMode"].ToString());
            string rs = configFileContent.ToString();
            File.WriteAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index), rs);
            ListViewItem[] listViewItemArray = this.list_Emulator.Items.Find(ld.Index.ToString(), false);
            if (listViewItemArray.Length != 0)
            {
                listViewItemArray[0].SubItems[1].Text = bool.Parse(configFileContent["basicSettings.rootMode"].ToString()) ? ld.Name + " (R)" : ld.Name;
                if(listViewItemArray[0].SubItems[1].Text.Contains("(R)"))
                {
                    Helper.raiseOnUpdateLDStatus(ld.Index, "Root OK");
                }
                else
                {
                    Helper.raiseOnUpdateLDStatus(ld.Index, "Unroot OK");
                }    
            }
            
        }

        private void enableRootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                LDEmulator ld = list_Emulator.SelectedItems[0].Tag as LDEmulator;
                toggleRoot(ld);
            }
        }

        private void rootUnrootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (list_Emulator.SelectedItems.Count > 0)
            {
                foreach (object selectedLD in list_Emulator.SelectedItems)
                {
                    LDEmulator ld = ((ListViewItem)selectedLD).Tag as LDEmulator;
                    toggleRoot(ld);
                }
            }
        }
    }
}
