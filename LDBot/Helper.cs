﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

using MimeKit;
using MailKit;
using MailKit.Net.Imap;
using System.Collections.Generic;
using System.Web;
using System.IO;
using MailKit.Net.Proxy;
using System.Threading;

namespace LDBot
{
    public class Helper
    {
        public static event dlgUpdateMainStatus onUpdateMainStatus;
        public static event dlgErrorMessage onErrorMessage;
        public static event dlgUpdateLDStatus onUpdateLDStatus;
        public static event dlgWriteLog onWriteLog;
        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                raiseOnUpdateMainStatus(String.Format("Configuration {0} = {1} has been saved", key, value));
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                raiseOnUpdateMainStatus("Error writing app settings");
            }
        }

        public static string CreateRandomStringNumber(int lengText, Random rd = null)
        {
            string text = "";
            bool flag = rd == null;
            bool flag2 = flag;
            if (flag2)
            {
                rd = new Random();
            }
            string text2 = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < lengText; i++)
            {
                text += text2[rd.Next(0, text2.Length)].ToString();
            }
            return text;
        }

        public static string CreateRandomNumber(int leng, Random rd = null)
        {
            string text = "";
            bool flag = rd == null;
            bool flag2 = flag;
            if (flag2)
            {
                rd = new Random();
            }
            string text2 = "0123456789";
            for (int i = 0; i < leng; i++)
            {
                text += text2[rd.Next(0, text2.Length)].ToString();
            }
            return text;
        }

        public static string Md5Encode(string text, string type = "X2")
        {
            MD5 md = MD5.Create();
            byte[] array = md.ComputeHash(Encoding.UTF8.GetBytes(text));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString(type));
            }
            return stringBuilder.ToString();
        }

        public static string URLDecode(string url)
        {
            return HttpUtility.UrlDecode(url);
        }

        public static void raiseOnUpdateMainStatus(string stt)
        {
            if (onUpdateMainStatus == null)
                return;
            else
                onUpdateMainStatus(stt);
        }

        public static void raiseOnErrorMessage(Exception err)
        {
            if (onErrorMessage == null)
                return;
            else
                onErrorMessage(err);
        }

        public static void raiseOnUpdateLDStatus(int ldIndex, string stt)
        {
            if (onUpdateLDStatus == null)
                return;
            else
                onUpdateLDStatus(ldIndex, stt);
        }

        public static void raiseOnWriteLog(string log)
        {
            if (onWriteLog == null)
                return;
            else
                onWriteLog(log);
        }

        public static string runCMD(string fileName, string arg)
        {
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arg,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };
                process.Start();
                process.WaitForExit(5000);
                string result = process.StandardOutput.ReadToEnd();
                process.Close();
                return result.Trim();
            }
            catch(Exception e)
            {
                if(e.GetType().Name == "Win32Exception")
                    raiseOnErrorMessage(new Exception("LD Player folder not found.\nPlease select the directory containing the file ldconsole.exe"));
                else
                    raiseOnErrorMessage(e);
                return "";
            }
        }

        public static List<MimeMessage> readMailIMAP(string mailServer, int port, string email, string password, string proxyInfo = "")
        {
            using (var client = new ImapClient())
            {
                List<MimeMessage> listMail = new List<MimeMessage>();
                if(proxyInfo != "")
                {
                    string[] proxy = proxyInfo.Split(':');
                    client.ProxyClient = new HttpProxyClient(proxy[0], int.Parse(proxy[1]));
                }    
                client.Connect(mailServer, port, true);

                client.Authenticate(email, password);

                // The Inbox folder is always available on all IMAP servers...
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                for (int i = 0; i < inbox.Count; i++)
                {
                    listMail.Add(inbox.GetMessage(i));
                }

                var junk = GetJunkFolder(client, new CancellationToken());
                junk.Open(FolderAccess.ReadOnly);

                for (int i = 0; i < junk.Count; i++)
                {
                    listMail.Add(junk.GetMessage(i));
                }

                client.Disconnect(true);
                return listMail;
            }
        }

        private static IMailFolder GetJunkFolder(ImapClient client, CancellationToken cancellationToken)
        {
            var personal = client.GetFolder(client.PersonalNamespaces[0]);

            foreach (var folder in personal.GetSubfolders(false, cancellationToken))
            {
                if (folder.Name == "Junk")
                    return folder;
            }

            return null;
        }
        public static string getFileNameByPath(string filePath)
        {
            string[] filePathArr = filePath.Split('\\');
            return filePathArr[filePathArr.Length - 1];
        }
    }

    public class Prompt : IDisposable
    {
        private Form prompt { get; set; }
        public string Result { get; }

        public Prompt(string text, string caption, string hint = "")
        {
            Result = ShowDialog(text, caption, hint);
        }
        //use a using statement
        private string ShowDialog(string text, string caption, string hint)
        {
            prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Text = text, Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter };
            TextBox textBox = new TextBox() { Left = 10, Top = 30, Width = 265, AllowDrop = true, Text = hint };
            Button confirmation = new Button() { Text = "Ok", Left = 180, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            textBox.DragOver += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Link;
                else
                    e.Effect = DragDropEffects.None;
            };
            textBox.DragDrop += (sender, e) =>
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                textBox.Text = files.First();
            };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        public void Dispose()
        {
            //See Marcus comment
            if (prompt != null)
            {
                prompt.Dispose();
            }
        }
    }

    public class ImageHandle
    {
        public static string ImageToBase64String(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            byte[] arr = new byte[ms.Length];

            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();

            string strBase64 = Convert.ToBase64String(arr);

            return strBase64;
        }
        public static string ImageToBase64String(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            byte[] arr = new byte[ms.Length];

            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();

            string strBase64 = Convert.ToBase64String(arr);

            return strBase64;
        }
        public static Bitmap Base64StringToBitmap(string imgStr)
        {
            byte[] bytes = Convert.FromBase64String(imgStr);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return (Bitmap)Image.FromStream(ms);
            }
        }
    }
}
