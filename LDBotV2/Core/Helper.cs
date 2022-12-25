using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KAutoHelper;
using LDBotV2.Entity;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Proxy;
using Microsoft.Win32;
using MimeKit;
using Newtonsoft.Json.Linq;
using Tesseract;

namespace LDBotV2.Core
{
    public class LDHelper
    {
        public static event dlgUpdateLDStatus onUpdateLDStatus;
        public static event dlgLoadListLD onLoadListLD;
        public static event dlgGetLDStatus onGetLDStatus;

        public static void raiseOnUpdateLDStatus(int ldIndex, string stt)
        {
            if (onUpdateLDStatus == null)
                return;
            else
                onUpdateLDStatus(ldIndex, stt);
        }
        public static void raiseOnLoadListLD()
        {
            if (onLoadListLD == null)
                return;
            else
                onLoadListLD();
        }

        public static string raiseOnGetLDStatus(int index)
        {
            if (onGetLDStatus == null)
                return "";
            else
                return onGetLDStatus(index);
        }
        public static bool toggleRoot(LDEmulator ld)
        {
            if (ld.isRunning)
            {
                LDManager.quitLD(ld);
            }
            JToken configFileContent = JToken.Parse(File.ReadAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index)));
            configFileContent["basicSettings.rootMode"] = (configFileContent["basicSettings.rootMode"] == null) ? true : !bool.Parse(configFileContent["basicSettings.rootMode"].ToString());
            string rs = configFileContent.ToString();
            File.WriteAllText(string.Format("{0}\\vms\\config\\leidian{1}.config", ConfigurationManager.AppSettings["LDPath"], ld.Index), rs);
            return bool.Parse(configFileContent["basicSettings.rootMode"].ToString());
        }
    }

    public class ToolHelper
    {
        public static event dlgUpdateMainStatus onWriteMainStatus;
        public static event dlgWriteLog onWriteLog;
        public static event dlgErrorMessage onWriteError;

        public static void checkValidConfigurations()
        {
            //Kiểm tra xem đã cài đặt LD Player chưa
            if (ConfigurationManager.AppSettings["LDPath"] == null)
            {
                Task.Run(() =>
                {
                    try
                    {
                        RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\XuanZhi\LDPlayer");
                        string ldPlayerInstallPath = RegKey.GetValue("InstallDir").ToString();
                        if (ldPlayerInstallPath.Contains(@"LDPlayer\LDPlayer"))
                        {
                            //Cập nhật đường dẫn vào file Config nếu chạy tool lần đầu
                            addOrUpdateAppSettings("LDPath", ldPlayerInstallPath);
                            //Cập nhật đường dẫn vào file adb.exe của LD Player
                            ADBHelper.SetADBFolderPath(ldPlayerInstallPath);
                        }
                        else
                        {
                            throw new Exception("Please install LD Player before using this tool!");
                        }
                    }
                    catch (Exception e)
                    {
                        raiseOnWriteError(e);
                    }
                });
            }
            else
            {
                ADBHelper.SetADBFolderPath(ConfigurationManager.AppSettings["LDPath"]);
                //Kiểm tra có thư mục Script
                if (!Directory.Exists(ConfigurationManager.AppSettings["LDPath"] + "Scripts"))
                {
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["LDPath"] + "Scripts");
                }
            }
        }
        public static void addOrUpdateAppSettings(string key, string value)
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
                raiseOnWriteLog(String.Format("Configuration {0} = {1} has been saved", key, value));
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (Exception e)
            {
                raiseOnWriteError(e);
            }
        }
        public static void startADBServer()
        {
            if (Process.GetProcessesByName("adb").Length == 0)
            {
                Task.Run(() =>
                {
                    runCMD(ConfigurationManager.AppSettings["LDPath"] + "adb.exe", "start-server");
                });
            }
        }
        public static void killADBServer()
        {
            if (Process.GetProcessesByName("adb").Length > 0)
            {
                runCMD(ConfigurationManager.AppSettings["LDPath"] + "adb.exe", "kill-server");
            }
        }
        public static void runCMD(string fileName, string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = command;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();
                process.Close();
            }
            catch (Exception e)
            {
                raiseOnWriteError(e);
            }
        }

        public static string runCMDForResult(string fileName, string command, int timeout = 10000, int retry = 2)
        {
            string result;
            try
            {
                var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = command,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };
                // process.WaitForExit();

                while (retry >= 0)
                {
                    retry--;
                    process.Start();
                    if (!process.WaitForExit(timeout))
                    {
                        process.Kill();
                    }
                    else
                    {
                        break;
                    }
                }

                var text = process.StandardOutput.ReadToEnd();
                result = text;
            }
            catch (Exception e)
            {
                raiseOnWriteError(e);
                result = null;
            }

            return result;
        }

        public static void raiseOnWriteMainStatus(string stt)
        {
            if (onWriteMainStatus == null)
                return;
            else
                onWriteMainStatus(stt);
        }

        public static void raiseOnWriteLog(string log)
        {
            if (onWriteLog == null)
                return;
            else
                onWriteLog(log);
        }

        public static void raiseOnWriteError(Exception err)
        {
            if (onWriteError == null)
                return;
            else
                onWriteError(err);
        }
        public static void pasteScript(LDEmulator ld)
        {
            if (Clipboard.ContainsFileDropList())
            {
                StringCollection copiedScripts = Clipboard.GetFileDropList();
                foreach (string script in copiedScripts)
                {
                    File.Copy(script, ld.ScriptFolder + "\\" + DataHelper.getFileNameByPath(script), true);
                    raiseOnWriteLog(string.Format("Chép {0} vào {1}", DataHelper.getFileNameByPath(script), ld.ScriptFolder));
                }

            }
        }
    }

    public class DataHelper
    {
        private static readonly string[] VietnameseSigns = new string[]
        {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"

        };
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
        public static string RemoveSign4VietnameseString(string str)
        {
            //Tiến hành thay thế , lọc bỏ dấu cho chuỗi
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }
        public static string getFileNameByPath(string filePath)
        {
            string[] filePathArr = filePath.Split('\\');
            return filePathArr[filePathArr.Length - 1];
        }
    }

    public class ImageHelper
    {
        public static string getTextFromImage(string imgPath)
        {
            Bitmap img = (Bitmap)Image.FromFile(imgPath);
            string text = getTextFromImage(img);
            img.Dispose();
            return text;
        }
        public static string getTextFromImage(Bitmap img)
        {
            string res = "";
            using (var engine = new TesseractEngine(@"tessdata", "vie", EngineMode.Default))
            {
                using (var page = engine.Process(img, PageSegMode.AutoOnly))
                    res = page.GetText();
            }
            return res;
        }

        public static Point? searchTextFromImg(string imgPath, string textToFind, bool isDebug = false)
        {
            Bitmap img = (Bitmap)Image.FromFile(imgPath);
            Point? point = searchTextFromImg(img, textToFind, isDebug);
            img.Dispose();
            return point;
        }

        public static Point? searchTextFromImg(Bitmap img, string textToFind, bool isDebug = false)
        {
            PageIteratorLevel myLevel = PageIteratorLevel.TextLine;
            using (var engine = new TesseractEngine(@"tessdata", "vie", EngineMode.Default))
            using (var page = engine.Process(img, PageSegMode.AutoOnly))
            using (var iter = page.GetIterator())
            {
                iter.Begin();
                do
                {
                    if (iter.TryGetBoundingBox(myLevel, out var rect))
                    {
                        var curText = iter.GetText(myLevel);
                        if (isDebug)
                            ToolHelper.raiseOnWriteLog(DataHelper.RemoveSign4VietnameseString(curText).Trim());
                        if (DataHelper.RemoveSign4VietnameseString(curText).Trim() == DataHelper.RemoveSign4VietnameseString(textToFind).Trim())
                        {
                            Point point = new Point(rect.X1 + (rect.X2 - rect.X1) / 2, rect.Y1 + (rect.Y2 - rect.Y1) / 2);
                            ToolHelper.raiseOnWriteLog(point.X.ToString() + ", " + point.Y.ToString());
                            return point;
                        }
                    }
                } while (iter.Next(myLevel));
                return null;
            }
        }
    }

    public class MailHelper
    {
        public static List<MimeMessage> readMailIMAP(string mailServer, int port, string email, string password, string proxyInfo = "")
        {
            using (var client = new ImapClient())
            {
                List<MimeMessage> listMail = new List<MimeMessage>();
                if (proxyInfo != "")
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
    }
}
