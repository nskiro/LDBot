using KAutoHelper;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace LDBotV2.Core
{
    public class BotAction
    {
        protected LDEmulator _ld;
        public bool isScriptRunning;
        private Random rd;
        private CancellationTokenSource cts;
        private CancellationToken cancelToken;
        public BotAction(LDEmulator ld)
        {
            if (ld != null)
                _ld = ld;
            rd = new Random();
            isScriptRunning = false;
        }
        #region Virtual Function
        public virtual void Init() {}

        public virtual void Start() { }

        public virtual void CaptureGuide(string fileName) { }
        #endregion

        #region Bot Function
        public void Stop()
        {
            isScriptRunning = false;
            cts.Cancel();
        }
        public void PreStart()
        {
            isScriptRunning = true;
            if(cts != null)
                cts.Dispose();
            cts = new CancellationTokenSource();
            cancelToken = cts.Token;
            try
            {
                Start();
            }
            catch(OperationCanceledException)
            {
                status("Đã dừng script");
            }
            catch(Exception e)
            {
                error(e);
            }
            finally
            {
                isScriptRunning = false;
                status("Script đã hoàn thành");
            }
        }
        protected void status(string stt)
        {
            if (stt.Length > 0)
                LDHelper.raiseOnUpdateLDStatus(_ld.Index, stt);
        }
        protected void log(string text)
        {
            if (text.Length > 0)
                ToolHelper.raiseOnWriteLog(text);
        }
        protected void error(Exception e)
        {
            ToolHelper.raiseOnWriteError(e);
        }
        protected void isStopRequested()
        {
            cancelToken.ThrowIfCancellationRequested();
        }
        protected void delay(double milisecond)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.Delay(milisecond);
        }
        protected string getView(bool isShowLog = false)
        {
            cancelToken.ThrowIfCancellationRequested();
            string result = LDManager.executeADBForResult(string.Format("-s {0} shell dumpsys window windows | grep -E 'mCurrentFocus|mFocusedApp'\"", _ld.DeviceID));
            if (isShowLog)
                ToolHelper.raiseOnWriteLog(result);
            return result;
        }
        protected bool isScreenDisplay(string viewCheck)
        {
            cancelToken.ThrowIfCancellationRequested();
            return getView().Contains(viewCheck);
        }
        protected bool isStringInImage(string findStr, int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            cancelToken.ThrowIfCancellationRequested();
            Bitmap screen = null;
            screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
            bool check = false;
            bool flag = startCropX != 0 || startCropY != 0 || right != 0 || bottom != 0;
            if (flag)
            {
                screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, (right - startCropX), (bottom - startCropY)));
            }
            check = DataHelper.RemoveSign4VietnameseString(ImageHelper.getTextFromImage(screen)).Contains(DataHelper.RemoveSign4VietnameseString(findStr));
            screen.Dispose();
            return check;
        }
        protected bool waitForScreen(string searchScreen, int timeOut = 5)
        {
            while (timeOut > 0)
            {
                cancelToken.ThrowIfCancellationRequested();
                status("Đợi " + searchScreen);
                if (isScreenDisplay(searchScreen))
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
                cancelToken.ThrowIfCancellationRequested();
                status("Tìm: " + searchString);
                if (isStringInImage(searchString, startCropX, startCropY, right, bottom))
                {
                    delay(1000);
                    return true;
                }
                timeOut--;
                delay(1000);
            }
            return false;
        }
        protected void click(int x, int y, int count = 1)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.Tap(_ld.DeviceID, x, y, count);
            status(string.Format("Click {0}:{1}", x, y));
        }
        protected void clickP(double x, double y, int count = 1)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.TapByPercent(_ld.DeviceID, x, y, count);
            status(string.Format("Click {0:0.00}%:{1:0.00}%", x, y));
        }
        protected void swipe(int startX, int startY, int stopX, int stopY, int swipeTime = 300)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.Swipe(_ld.DeviceID, startX, startY, stopX, stopY, swipeTime);
            status(string.Format("Swipe {0}:{1} to {2}:{3}", startX, startY, stopX, stopY));
        }
        protected void swipeP(double startX, double startY, double stopX, double stopY, int swipeTime = 300)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.SwipeByPercent(_ld.DeviceID, startX, startY, stopX, stopY, swipeTime);
            status(string.Format("Swipe {0:0.0}%:{1:0.0}% to {2:0.0}%:{3:0.0}%", startX, startY, stopX, stopY));
        }
        protected void inputKey(ADBKeyEvent key)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.Key(_ld.DeviceID, key);
            status("Nhấn " + key.ToString());
        }
        protected void inputText(string txt)
        {
            cancelToken.ThrowIfCancellationRequested();
            ADBHelper.InputText(_ld.DeviceID, txt);
            status("Nhập: " + txt);
        }
        protected void clickAndHold(int x, int y, int duration = 500)
        {
            cancelToken.ThrowIfCancellationRequested();
            status(string.Format("Nhấn giữ {0}:{1} {2} ms", x, y, duration));
            ADBHelper.LongPress(_ld.DeviceID, x, y, duration);
        }
        protected List<string> getInstalledPackages(bool isShowDebug = false)
        {
            cancelToken.ThrowIfCancellationRequested();
            status("Lấy danh sách app");
            List<string> installedPackage = new List<string>();
            string[] results = LDManager.executeADBForResult(string.Format("-s {0} shell cmd package list package", _ld.DeviceID)).Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rs in results)
            {
                cancelToken.ThrowIfCancellationRequested();
                installedPackage.Add(rs.Replace("package:", "").Trim());
                if (isShowDebug)
                    log(rs.Replace("package:", "").Trim());
            }
            return installedPackage;
        }
        protected bool runApp(string packageName, string mainActivity, int timeOut = 10)
        {
            cancelToken.ThrowIfCancellationRequested();
            status("Mở " + packageName);
            LDManager.executeLdConsole(string.Format("runapp --index {0} --packagename {1}", _ld.Index, packageName));
            //Kiểm tra app có mở lên được ko
            if (!waitForScreen(mainActivity, timeOut))
            {
                status("Mở app thất bại");
                return false;
            }
            return true;
        }
        protected void clearAppData(string packageName)
        {
            cancelToken.ThrowIfCancellationRequested();
            status("Xóa d.liệu " + packageName);
            LDManager.executeADB(string.Format("-s {0} shell pm clear {1}", _ld.DeviceID, packageName));
            delay(1000);
        }
        protected void killApp(string packageName)
        {
            cancelToken.ThrowIfCancellationRequested();
            status("Đóng " + packageName);
            LDManager.executeLdConsole(string.Format("killapp --index {0} --packagename {1}", _ld.Index, packageName));
            delay(2000);
            while(!waitForScreen("com.android.launcher3.Launcher"))
            {
                inputKey(ADBKeyEvent.KEYCODE_HOME);
            }
        }
        protected void changeProxy(string proxyConfig = "")
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.changeProxy(_ld, proxyConfig);
        }
        protected List<MimeMessage> getAllMails(string mailServer, int port, string mail, string password)
        {
            cancelToken.ThrowIfCancellationRequested();
            if (_ld.isUseProxy)
                return MailHelper.readMailIMAP(mailServer, port, mail, password, _ld.Proxy);
            return MailHelper.readMailIMAP(mailServer, port, mail, password);
        }
        protected void openIntent(string intentName)
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.executeADB(string.Format("-s {0} shell am start -a {1}", _ld.DeviceID, intentName));
            delay(2000);
        }
        protected void openUrl(string url)
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.executeADB(string.Format("-s {0} shell am start -a android.intent.action.VIEW -d {1} --es 'com.android.browser.application_id' 'com.android.browser'\"", _ld.DeviceID, url));
            delay(2000);
        }
        protected bool searchTextAndClick(string findText, string imgPath = "", bool isDebug = false, int clickCount = 1)
        {
            cancelToken.ThrowIfCancellationRequested();
            Point? coords;
            if (imgPath != "")
                coords = ImageHelper.searchTextFromImg(imgPath, findText, isDebug);
            else
            {
                Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
                coords = ImageHelper.searchTextFromImg(screen, findText, isDebug);
                screen.Dispose();
            }
            if (coords != null)
            {
                ADBHelper.Tap(_ld.DeviceID, coords.Value.X, coords.Value.Y, clickCount);
                return true;
            }
            else
            {
                log(_ld.Name + ": " + findText + " not found");
                return false;
            }
        }
        protected string getTextInCurScr(int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0)
        {
            cancelToken.ThrowIfCancellationRequested();
            Bitmap screen = (Bitmap)CaptureHelper.CaptureWindow(_ld.BindHandle);
            bool flag = startCropX != 0 || startCropY != 0 || right != 0 || bottom != 0;
            if (flag)
            {
                screen = CaptureHelper.CropImage(screen, new Rectangle(startCropX, startCropY, (right - startCropX), (bottom - startCropY)));
            }
            string res = ImageHelper.getTextFromImage(screen);
            screen.Dispose();
            return res;
        }
        protected void shutDownLD()
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.quitLD(_ld);
        }
        protected void startLD()
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.startLD(_ld);
        }
        protected void restartLD()
        {
            cancelToken.ThrowIfCancellationRequested();
            LDManager.restartLD(_ld);
        }
        protected void deleteGoogleAccount(string email, bool isRebootRequired = true)
        {
            cancelToken.ThrowIfCancellationRequested();
            if (_ld.isRunning)
            {
                inputKey(ADBKeyEvent.KEYCODE_HOME);
                string subQuery = string.Format("\"DELETE FROM accounts WHERE name = '{0}'\"", email);
                string query = string.Format("-s {0} shell sqlite3 /data/system_de/0/accounts_de.db \"{1}\"", _ld.DeviceID, subQuery);
                string query2 = string.Format("-s {0} shell sqlite3 /data/system_ce/0/accounts_ce.db \"{1}\"", _ld.DeviceID, subQuery);
                LDManager.executeADB(query);
                delay(200);
                LDManager.executeADB(query2);
                delay(1000);
                if (isRebootRequired)
                {
                    restartLD();
                    while (isScreenDisplay("com.android.launcher3.Launcher"))
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        delay(1000);
                    }
                    do
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        delay(3000);
                    }
                    while (!isScreenDisplay("com.android.launcher3.Launcher"));
                }
            }
        }
        #endregion
    }
}
