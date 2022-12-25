
namespace LDBotV2
{
    partial class FormBot
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBot));
            this.list_Emulator = new System.Windows.Forms.ListView();
            this.Idx = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LDName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.khởiĐộngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.khởiĐộngLạiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thoátToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.nhânBảnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xóaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mởThưMụcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saoChépToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dánToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hẹnGiờToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.đổiThôngTinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.proxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rootUnRootToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.đổiPhầnCứngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.stt_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtb_log = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtb_error = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // list_Emulator
            // 
            this.list_Emulator.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Idx,
            this.LDName,
            this.Status});
            this.list_Emulator.ContextMenuStrip = this.contextMenuStrip1;
            this.list_Emulator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_Emulator.FullRowSelect = true;
            this.list_Emulator.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.list_Emulator.HideSelection = false;
            this.list_Emulator.Location = new System.Drawing.Point(0, 0);
            this.list_Emulator.Margin = new System.Windows.Forms.Padding(8, 16, 8, 16);
            this.list_Emulator.Name = "list_Emulator";
            this.list_Emulator.Size = new System.Drawing.Size(484, 417);
            this.list_Emulator.TabIndex = 0;
            this.list_Emulator.UseCompatibleStateImageBehavior = false;
            this.list_Emulator.View = System.Windows.Forms.View.Details;
            // 
            // Idx
            // 
            this.Idx.Text = "Idx";
            this.Idx.Width = 35;
            // 
            // LDName
            // 
            this.LDName.Text = "Tên Giả Lập";
            this.LDName.Width = 120;
            // 
            // Status
            // 
            this.Status.Text = "Trạng Thái";
            this.Status.Width = 324;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.khởiĐộngToolStripMenuItem,
            this.khởiĐộngLạiToolStripMenuItem,
            this.thoátToolStripMenuItem,
            this.toolStripSeparator1,
            this.nhânBảnToolStripMenuItem,
            this.xóaToolStripMenuItem,
            this.toolStripSeparator3,
            this.scriptToolStripMenuItem,
            this.toolStripSeparator2,
            this.đổiThôngTinToolStripMenuItem,
            this.proxyToolStripMenuItem,
            this.rootUnRootToolStripMenuItem,
            this.đổiPhầnCứngToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 242);
            // 
            // khởiĐộngToolStripMenuItem
            // 
            this.khởiĐộngToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.power_on__1_;
            this.khởiĐộngToolStripMenuItem.Name = "khởiĐộngToolStripMenuItem";
            this.khởiĐộngToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.khởiĐộngToolStripMenuItem.Text = "Khởi động";
            this.khởiĐộngToolStripMenuItem.Click += new System.EventHandler(this.khởiĐộngToolStripMenuItem_Click);
            // 
            // khởiĐộngLạiToolStripMenuItem
            // 
            this.khởiĐộngLạiToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.refresh;
            this.khởiĐộngLạiToolStripMenuItem.Name = "khởiĐộngLạiToolStripMenuItem";
            this.khởiĐộngLạiToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.khởiĐộngLạiToolStripMenuItem.Text = "Khởi động lại";
            this.khởiĐộngLạiToolStripMenuItem.Click += new System.EventHandler(this.khởiĐộngLạiToolStripMenuItem_Click);
            // 
            // thoátToolStripMenuItem
            // 
            this.thoátToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.power_on;
            this.thoátToolStripMenuItem.Name = "thoátToolStripMenuItem";
            this.thoátToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.thoátToolStripMenuItem.Text = "Thoát";
            this.thoátToolStripMenuItem.Click += new System.EventHandler(this.thoátToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // nhânBảnToolStripMenuItem
            // 
            this.nhânBảnToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.visualization;
            this.nhânBảnToolStripMenuItem.Name = "nhânBảnToolStripMenuItem";
            this.nhânBảnToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.nhânBảnToolStripMenuItem.Text = "Nhân bản";
            this.nhânBảnToolStripMenuItem.Click += new System.EventHandler(this.nhânBảnToolStripMenuItem_Click);
            // 
            // xóaToolStripMenuItem
            // 
            this.xóaToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.delete;
            this.xóaToolStripMenuItem.Name = "xóaToolStripMenuItem";
            this.xóaToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.xóaToolStripMenuItem.Text = "Xóa";
            this.xóaToolStripMenuItem.Click += new System.EventHandler(this.xóaToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
            // 
            // scriptToolStripMenuItem
            // 
            this.scriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mởThưMụcToolStripMenuItem,
            this.saoChépToolStripMenuItem,
            this.dánToolStripMenuItem,
            this.hẹnGiờToolStripMenuItem});
            this.scriptToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_apk_16;
            this.scriptToolStripMenuItem.Name = "scriptToolStripMenuItem";
            this.scriptToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.scriptToolStripMenuItem.Text = "Script";
            // 
            // mởThưMụcToolStripMenuItem
            // 
            this.mởThưMụcToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.folder;
            this.mởThưMụcToolStripMenuItem.Name = "mởThưMụcToolStripMenuItem";
            this.mởThưMụcToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.mởThưMụcToolStripMenuItem.Text = "Mở thư mục";
            this.mởThưMụcToolStripMenuItem.Click += new System.EventHandler(this.mởThưMụcToolStripMenuItem_Click);
            // 
            // saoChépToolStripMenuItem
            // 
            this.saoChépToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_copy_64;
            this.saoChépToolStripMenuItem.Name = "saoChépToolStripMenuItem";
            this.saoChépToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.saoChépToolStripMenuItem.Text = "Sao chép";
            this.saoChépToolStripMenuItem.Click += new System.EventHandler(this.saoChépToolStripMenuItem_Click);
            // 
            // dánToolStripMenuItem
            // 
            this.dánToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_paste_30;
            this.dánToolStripMenuItem.Name = "dánToolStripMenuItem";
            this.dánToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.dánToolStripMenuItem.Text = "Dán";
            this.dánToolStripMenuItem.Click += new System.EventHandler(this.dánToolStripMenuItem_Click);
            // 
            // hẹnGiờToolStripMenuItem
            // 
            this.hẹnGiờToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_clock_64__1_;
            this.hẹnGiờToolStripMenuItem.Name = "hẹnGiờToolStripMenuItem";
            this.hẹnGiờToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.hẹnGiờToolStripMenuItem.Text = "Hẹn giờ";
            this.hẹnGiờToolStripMenuItem.Click += new System.EventHandler(this.hẹnGiờToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // đổiThôngTinToolStripMenuItem
            // 
            this.đổiThôngTinToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.smartphone__1_;
            this.đổiThôngTinToolStripMenuItem.Name = "đổiThôngTinToolStripMenuItem";
            this.đổiThôngTinToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.đổiThôngTinToolStripMenuItem.Text = "Đổi thông tin";
            this.đổiThôngTinToolStripMenuItem.Click += new System.EventHandler(this.đổiThôngTinToolStripMenuItem_Click);
            // 
            // proxyToolStripMenuItem
            // 
            this.proxyToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.world;
            this.proxyToolStripMenuItem.Name = "proxyToolStripMenuItem";
            this.proxyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.proxyToolStripMenuItem.Text = "Proxy";
            this.proxyToolStripMenuItem.Click += new System.EventHandler(this.proxyToolStripMenuItem_Click);
            // 
            // rootUnRootToolStripMenuItem
            // 
            this.rootUnRootToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_powershell_a_task_based_command_line_shell_and_scripting_language_32;
            this.rootUnRootToolStripMenuItem.Name = "rootUnRootToolStripMenuItem";
            this.rootUnRootToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rootUnRootToolStripMenuItem.Text = "Root/UnRoot";
            this.rootUnRootToolStripMenuItem.Click += new System.EventHandler(this.rootUnRootToolStripMenuItem_Click);
            // 
            // đổiPhầnCứngToolStripMenuItem
            // 
            this.đổiPhầnCứngToolStripMenuItem.Image = global::LDBotV2.Properties.Resources.icons8_microchip_16;
            this.đổiPhầnCứngToolStripMenuItem.Name = "đổiPhầnCứngToolStripMenuItem";
            this.đổiPhầnCứngToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.đổiPhầnCứngToolStripMenuItem.Text = "Đổi phần cứng";
            this.đổiPhầnCứngToolStripMenuItem.Click += new System.EventHandler(this.đổiPhầnCứngToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.Highlight;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stt_main,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 707);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(484, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // stt_main
            // 
            this.stt_main.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stt_main.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.stt_main.Name = "stt_main";
            this.stt_main.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel1.ForeColor = System.Drawing.Color.Khaki;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(350, 17);
            this.toolStripStatusLabel1.Text = "F2: Tạo mới - F6: Load Script - F7: Chạy Script - F8: Dừng script";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.list_Emulator);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(484, 707);
            this.splitContainer1.SplitterDistance = 417;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(484, 286);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtb_log);
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(476, 255);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtb_log
            // 
            this.rtb_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_log.Location = new System.Drawing.Point(3, 3);
            this.rtb_log.Name = "rtb_log";
            this.rtb_log.ReadOnly = true;
            this.rtb_log.Size = new System.Drawing.Size(470, 249);
            this.rtb_log.TabIndex = 0;
            this.rtb_log.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.rtb_error);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(476, 260);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Error";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // rtb_error
            // 
            this.rtb_error.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_error.Location = new System.Drawing.Point(3, 3);
            this.rtb_error.Name = "rtb_error";
            this.rtb_error.ReadOnly = true;
            this.rtb_error.Size = new System.Drawing.Size(470, 254);
            this.rtb_error.TabIndex = 1;
            this.rtb_error.Text = "";
            // 
            // FormBot
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(484, 729);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "FormBot";
            this.Text = "LD Bot ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormBot_FormClosing);
            this.Load += new System.EventHandler(this.FormBot_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormBot_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView list_Emulator;
        private System.Windows.Forms.ColumnHeader Idx;
        private System.Windows.Forms.ColumnHeader LDName;
        private System.Windows.Forms.ColumnHeader Status;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox rtb_log;
        private System.Windows.Forms.RichTextBox rtb_error;
        private System.Windows.Forms.ToolStripStatusLabel stt_main;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem nhânBảnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem đổiThôngTinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem khởiĐộngToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem xóaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem khởiĐộngLạiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thoátToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mởThưMụcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saoChépToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dánToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hẹnGiờToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem proxyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rootUnRootToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem đổiPhầnCứngToolStripMenuItem;
    }
}