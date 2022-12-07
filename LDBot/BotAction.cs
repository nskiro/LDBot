using KAutoHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using xNet;
using MimeKit;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace LDBot
{

    public class BotAction
    {
        protected LDEmulator _ld;
        public bool isRunning;
        private Random rd;
        public BotAction(LDEmulator ld)
        {
            if (ld != null)
                _ld = ld;
            rd = new Random();
            isRunning = false;
        }

        #region Virtual Function
        public virtual void Init()
        {
        }

        public virtual void Start() { }

        public virtual void CaptureGuide(string fileName) { }

        public void PreStart()
        {
            new Task(delegate
            {
                isRunning = true;
                Start();
            }).Start();
        }

        public virtual void Stop()
        {
            isRunning = false;
            setStatus("Stop script");
        }
        #endregion

        #region Bot Function
        protected void setStatus(string stt)
        {
            if (stt.Length > 0)
                Helper.raiseOnUpdateLDStatus(_ld.Index, stt);
        }

        protected bool findAndClick(string imgPath, int timeOut = 7, double similarPercent = 0.9, bool isClickUntilDisApp = false, int xPlus = 0, int yPlus = 0, int startCropX = 0, int startCropY = 0, int cropWidth = 0, int cropHeight = 0)
        {
            if (!isRunning)
                return false;
            Bitmap img = (Bitmap)Image.FromFile(imgPath);
            Bitmap screen = null;
            bool result = false;
            while (timeOut > 0)
            {
                click_image:
                screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                bool flag = startCropX != 0 || startCropY != 0 || cropWidth != 0 || cropHeight != 0;
                if (flag)
                {
                    screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, cropWidth, cropHeight));
                }
                Point? point = ImageScanOpenCV.FindOutPoint(screen, img, similarPercent);
                bool flag2 = point != null;
                if (flag2)
                {
                    setStatus(string.Format("{0} found-click", Helper.getFileNameByPath(imgPath)));
                    int Xmore = rd.Next(3);
                    int Ymore = rd.Next(3);
                    //AutoControl.SendClickOnPosition(_ld.TopHandle, point.Value.X + Xmore + xPlus + startCropX, point.Value.Y + Ymore + yPlus + startCropY, EMouseKey.LEFT, 1);
                    ADBHelper.Tap(_ld.DeviceID, point.Value.X + Xmore + xPlus + startCropX, point.Value.Y + Ymore + yPlus + startCropY);
                    if (isClickUntilDisApp)
                    {
                        delay(1000);
                        goto click_image;
                    }
                    result = true;
                    break;
                }
                setStatus(string.Format("{0} not found", Helper.getFileNameByPath(imgPath)));
                timeOut--;
                delay(200);
            }
            screen.Dispose();
            img.Dispose();
            return result;
        }

        protected bool findImage(string imgPath, int timeOut = 7, double similarPercent = 0.9, int startCropX = 0, int startCropY = 0, int cropWidth = 0, int cropHeight = 0)
        {
            if (!isRunning)
                return false;
            Bitmap img = (Bitmap)Image.FromFile(imgPath);
            Bitmap screen = null;
            bool result = false;
            while (timeOut > 0)
            {
                screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                bool flag = startCropX != 0 || startCropY != 0 || cropWidth != 0 || cropHeight != 0;
                if (flag)
                {
                    screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, cropWidth, cropHeight));
                }
                result = ImageScanOpenCV.FindOutPoint(screen, img, similarPercent) != null;
                if (result)
                {
                    setStatus(string.Format("{0} found", Helper.getFileNameByPath(imgPath)));
                    break;
                }
                timeOut--;
                delay(200);
            }
            screen.Dispose();
            img.Dispose();
            return result;
        }

        protected void captureImage(string imgName, int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            try
            {
                Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                bool flag = startCropX != 0 || startCropY != 0 || right != 0 || bottom != 0;
                if (flag)
                {
                    screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, (right - startCropX), (bottom - startCropY)));
                }
                screen.Save(_ld.ScriptFolder + "\\" + imgName + ".png");
                screen.Dispose();
                Helper.raiseOnWriteLog("Capture " + imgName + ".png done");
            }
            catch(Exception e)
            {
                Helper.raiseOnErrorMessage(e);
            }
        }

        protected bool checkStringInImage(string findStr, int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            try
            {
                Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                bool check = false;
                bool flag = startCropX != 0 || startCropY != 0 || right != 0 || bottom != 0;
                if (flag)
                {
                    screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, (right - startCropX), (bottom - startCropY)));
                }
                check = Helper.RemoveSign4VietnameseString(Helper.getTextFromImage(screen)).Contains(Helper.RemoveSign4VietnameseString(findStr));
                return check;
            }
            catch (Exception e)
            {
                Helper.raiseOnErrorMessage(e);
                return false;
            }
        }
        protected void click(int x, int y, int count = 1)
        {
            if (!isRunning)
                return;
            ADBHelper.Tap(_ld.DeviceID, x, y, count);
            setStatus(string.Format("Click at {0}:{1}", x, y));
        }
        protected void clickP(double x, double y, int count = 1)
        {
            if (!isRunning)
                return;
            ADBHelper.TapByPercent(_ld.DeviceID, x, y, count);
            setStatus(string.Format("Click at {0:0.00}%:{1:0.00}%", x, y));
        }
        protected void swipe(int startX, int startY, int stopX, int stopY, int swipeTime = 300)
        {
            if (!isRunning)
                return;
            ADBHelper.Swipe(_ld.DeviceID, startX, startY, stopX, stopY, swipeTime);
            setStatus(string.Format("Swipe from {0}:{1} to {2}:{3}", startX, startY, stopX, stopY));
        }
        protected void swipeP(double startX, double startY, double stopX, double stopY, int swipeTime = 300)
        {
            if (!isRunning)
                return;
            ADBHelper.SwipeByPercent(_ld.DeviceID, startX, startY, stopX, stopY, swipeTime);
            setStatus(string.Format("Swipe from {0:0.00}%:{1:0.00}% to {2:0.00}%:{3:0.00}%", startX, startY, stopX, stopY));
        }
        protected void inputKey(ADBKeyEvent key)
        {
            if (!isRunning)
                return;
            ADBHelper.Key(_ld.DeviceID, key);
            setStatus("Press key " + key.ToString());
        }
        protected void inputKey(VKeys key)
        {
            if (!isRunning)
                return;
            AutoControl.SendKeyBoardPress(_ld.BindHandle, key);
            setStatus("Press key " + key.ToString());
        }
        protected void inputText(string txt)
        {
            if (!isRunning)
                return;
            ADBHelper.InputText(_ld.DeviceID, txt);
            setStatus("Input: " + txt);
        }
        protected void clickAndHold(int x, int y, int duration = 500)
        {
            if (!isRunning)
                return;
            ADBHelper.LongPress(_ld.DeviceID, x, y, duration);
        }
        protected void delay(double ms)
        {
            if (!isRunning)
                return;
            ADBHelper.Delay(ms);
        }
        protected List<string> getInstalledPackages(bool isShowDebug = false)
        {
            if (!isRunning)
                return null;
            setStatus("Get installed packages");
            List<string> installedPackage = new List<string>();
            string[] results = Helper.runCMD(LDManager.adb, string.Format("-s {0} shell cmd package list package", _ld.DeviceID)).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rs in results)
            {
                installedPackage.Add(rs.Replace("package:", "").Trim());
                if (isShowDebug)
                    writeLog(rs.Replace("package:", "").Trim());
            }
            return installedPackage;
        }
        protected void writeLog(string log)
        {
            if (log.Length > 0)
                Helper.raiseOnWriteLog(log);
        }
        protected void runApp(string packageName, string mainActivity, int timeOut = 10)
        {
            if (!isRunning)
                return;
            setStatus("Run " + packageName);
            LDManager.executeLdConsole(string.Format("runapp --index {0} --packagename {1}", _ld.Index, packageName));
            //Kiểm tra app có mở lên được ko
            if (!waitForScreen(mainActivity, timeOut))
                throw new ScriptException(packageName + " can not opened");
        }
        protected void killApp(string packageName)
        {
            if (!isRunning)
                return;
            setStatus("Kill " + packageName);
            LDManager.executeLdConsole(string.Format("killapp --index {0} --packagename {1}", _ld.Index, packageName));
        }
        protected void changeProxy(string proxyConfig = "")
        {
            LDManager.changeProxy(_ld, proxyConfig);
        }
        protected string getCurrentIP()
        {
            using (var request = new HttpRequest())
            {
                try
                {
                    request.UserAgent = Http.ChromeUserAgent();
                    if (_ld.isUseProxy)
                        request.Proxy = HttpProxyClient.Parse(_ld.Proxy);
                    string content = request.Get("https://api.ipify.org").ToString();
                    /*var jsonStruct = new
                    {
                        ip = "",
                        country = "",
                        cc = ""
                    };
                    var data = JsonConvert.DeserializeAnonymousType(content, jsonStruct);
                    if (data.ip != "")
                        return data.ip;
                    return "";*/
                    return content;
                }
                catch(Exception e)
                {
                    writeLog(e.Message);
                    return "";
                }
            }
        }
        protected List<MimeMessage> getAllMails(string mailServer, int port, string mail, string password)
        {
            try
            {
                if (_ld.isUseProxy)
                    return Helper.readMailIMAP(mailServer, port, mail, password, _ld.Proxy);
                return Helper.readMailIMAP(mailServer, port, mail, password);
            }
            catch (Exception e)
            {
                Helper.raiseOnErrorMessage(e);
                return null;
            }
        } 
        protected void clearAppData(string packageName)
        {
            if (!isRunning)
                return;
            setStatus("Clear " + packageName);
            Helper.runCMD(LDManager.adb, string.Format("-s {0} shell pm clear {1}", _ld.DeviceID, packageName));
        }
        protected void restartLD()
        {
            if(_ld.isRunning)
                LDManager.restartLD(_ld);
        }
        protected double getScreenScaling()
        {
            return Helper.GetScreenScalingFactor();
        }
        protected void deleteGoogleAccount(string email, bool isRebootRequired = true)
        {
            if (isRunning)
            {
                inputKey(ADBKeyEvent.KEYCODE_HOME);
                string subQuery = string.Format("\"DELETE FROM accounts WHERE name = '{0}'\"", email);
                string query = string.Format("adb --index {0} --command \"shell sqlite3 /data/system_de/0/accounts_de.db \"{1}\"\"", _ld.Index, subQuery);
                string query2 = string.Format("adb --index {0} --command \"shell sqlite3 /data/system_ce/0/accounts_ce.db \"{1}\"\"", _ld.Index, subQuery);
                LDManager.executeLdConsole(query);
                LDManager.executeLdConsole(query2);
                delay(1000);
                if (isRebootRequired)
                {
                    restartLD();
                    while (checkView("com.android.launcher3.Launcher"))
                    {
                        delay(1000);
                    }
                    do
                    {
                        delay(3000);
                    }
                    while (!checkView("com.android.launcher3.Launcher"));
                }
            }
        }
        protected bool checkView(string viewCheck)
        {
            return getView().Contains(viewCheck);
        }
        public string getView()
        {
            return LDManager.executeLdConsoleForResult(string.Format("adb --index {0} --command \"shell dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'\"", _ld.Index));
        }
        protected void openIntent(string intentName)
        {
            LDManager.executeLdConsole(string.Format("adb --index {0} --command \"shell am start -a {1}\"", _ld.Index, intentName));
            delay(2000);
        }
        protected void openUrl(string url)
        {
            LDManager.executeLdConsole(string.Format("adb --index {0} --command \"shell am start -a android.intent.action.VIEW -d {1}\"", _ld.Index, url));
            delay(2000);
        }

        public bool searchImgAndClick(string findText, string imgPath = "", bool isDebug = false, int clickCount = 1)
        {
            Point? coords;
            if (imgPath != "")
                coords = Helper.searchTextFromImgAndClick(imgPath, findText, isDebug);
            else
            {
                Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                coords = Helper.searchTextFromImgAndClick(screen, findText, isDebug);
            }    
            if(coords != null)
            {
                ADBHelper.Tap(_ld.DeviceID, coords.Value.X, coords.Value.Y, clickCount);
                return true;
            }
            else
            {
                writeLog(_ld.Name + ": " + findText + " not found");
                return false;
            }    
        }

        protected string getTextInCurScr(int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
            bool flag = startCropX != 0 || startCropY != 0 || right != 0 || bottom != 0;
            if (flag)
            {
                screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, (right - startCropX), (bottom - startCropY)));
            }
            string res = Helper.getTextFromImage(screen);
            screen.Dispose();
            return res;
        }

        protected bool waitForScreen(string searchScreen, int timeOut = 5)
        {
            while (timeOut > 0)
            {
                setStatus("Wait " + searchScreen);
                if (checkView(searchScreen))
                {
                    delay(1000);
                    return true;
                }
                timeOut--;
                delay(1000);
            }
            return false;
        }
        protected bool waitForStr(string searchString, int timeOut = 5, int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            while (timeOut > 0)
            {
                setStatus("Search: " + searchString);
                if (checkStringInImage(searchString, startCropX, startCropY, right, bottom))
                {
                    delay(1000);
                    return true;
                }
                timeOut--;
                delay(1000);
            }
            return false;
        }
        protected void shutDownLD()
        {
            LDManager.quitLD(_ld.Index);
        }

        protected void runLD()
        {
            LDManager.runLD(_ld);
            do
            {
                delay(3000);
            }
            while (!checkView("com.android.launcher3.Launcher"));
        }
        #endregion
    }
}