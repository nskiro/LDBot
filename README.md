<!-- TABLE OF CONTENTS -->
<details>
  <summary>TABLE OF CONTENTS</summary>
  <ul>
	<li><a href="#script-example">Script Example</a></li>
    <li><a href="#variables">Variables</a></li>
    <li><a href="#virtual-functions">Virtual Functions</a></li>
    <li><a href="#script-functions">Script Functions</a></li>
    <li><a href="#adbhelper-functions">ADBHelper Functions</a></li>
    <li><a href="#used-libraries">Used Libraries</a></li>
	<li><a href="#library-documents">Library Documents</a></li>
  </ul>
  
</details>

<!-- Variables -->
## Variables
```cs
bool isRunning; //kiểm tra trạng thái script đang chạy hay dừng, dùng để ngắt script | khởi tạo false
_ld: {
	Index: int,
	Name: string,
	TopHandle: IntPtr,
	BindHandle: IntPtr,
	isRunning: bool,
	pID: int,
	VboxPID: int
	botAction: BotAction,
	ScriptFolder: string,
	DeviceID: string
};
```
<!-- Virtual Functions -->
## Virtual Functions
```cs
void Init(); //Chạy ngay khi chọn Load Script
void Start(); //Chạy khi chọn Start Script
void Stop(); // set isRunning = false
void CaptureGuide(string fileName); //Xử lý hỗ trợ chụp ảnh theo hướng dẫn
/* Example */
public override void CaptureGuide(string fileName)
{
	switch(fileName)
	{
		case "reg-1.png":
			captureImage("img_skip", 10, 442, 38, 460);
			break;
		case "reg-2.png":
			captureImage("img_signup", 56, 324, 85, 341);
			captureImage("img_login", 217, 324, 243, 341);
			break;
		case "reg-3.png":
			captureImage("img_captcha", 229, 108, 255, 128);
			break;
		case "reg-4.png":
			captureImage("img_checkCapt", 23, 430, 43, 448);
			break;
		case "reg-5.png":
			captureImage("img_inviteCode", 269, 41, 304, 59);
			break;
		case "reg-6.png":
			captureImage("img_signContinue", 130, 362, 190, 377);
			break;
		case "login-1.png":
			captureImage("img_login_done", 9, 37, 49, 77);
			break;
		case "login-2.png":
			captureImage("img_noInternet", 87, 125, 107, 138);
			break;
		case "login-3.png":
			captureImage("img_close_ad", 273, 84, 292, 101);
			break;
		case "login-4.png":
			captureImage("img_done", 103, 397, 164, 413);
			break;
	}
}
```
<!-- Script Functions -->
## Script Functions
```cs
//Cập nhật trạng thái vào list view
void setStatus(string stt); 

//Tìm kiếm hình ảnh
bool findImage(string imgPath, [double similarPercent = 0.9, int startCropX = 0, int startCropY = 0, int cropWidth = 0, int cropHeight = 0]);

//Tìm kiếm và click theo hình ảnh.
bool findAndClick(string imgPath, [double similarPercent = 0.9, bool isClickUntilDisApp = false, int xPlus = 0, int yPlus = 0, int startCropX = 0, int startCropY = 0, int cropWidth = 0, int cropHeight = 0]);

//Chụp và cắt, lưu ảnh
void captureImage(string imgName, int startCropX = 0, int startCropY = 0, int right = 0, int bottom = 0);

//Click theo tọa độ x,y
void click(int x, int y, int count = 1);

//Click tại điểm tính theo % độ phân giải
void clickP(double x, double y, int count = 1);

//Nhấn và giữ tại 1 điểm
void clickAndHold(int x, int y, int duration = 500);

//Vuốt theo 2 điểm tọa độ
void swipe(int startX, int startY, int stopX, int stopY, [int swipeTime = 300]);

//Vuốt theo 2 điểm tính theo % độ phân giải
void swipeP(double startX, double startY, double stopX, double stopY, int swipeTime = 300);

//Giả lập nhấn phím
void inputKey(ADBKeyEvent key);

//Nhập chuỗi ký tự
void inputText(string txt);

//Delay
void delay(double ms);

//Lấy danh sách app được cài đặt dưới dạng package name. Package Name có dạng: com.cyanogenmod.filemanager
List<string> getInstalledPackages();

//run app theo package name
void runApp(string packageName);

//kill app theo package name
void killApp(string packageName);

//Xóa dữ liệu app
void clearAppData(string packageName);

// Hiển thị thông tin vào debug log
void writeLog(string log); 

//Change proxy, truyền vào chuỗi rỗng "" để remove proxy
void changeProxy(string proxyConfig); 

//Hiển thị địa chỉ IP hiện tại
string getCurrentIP(); 

//Đọc email IMAP
List<MimeMessage> getAllMails(string mailServer, int port, string mail, string password);
```
<!-- ADBHelper Functions -->
## ADBHelper Functions
```cs
void Delay(double delayTime);
string ExecuteCMD(string cmdCommand);
string ExecuteCMDBat(string deviceID, string cmdCommand);
Point? FindImage(string deviceID, string ImagePath, int delayPerCheck = 2000, int count = 5);
bool FindImageAndClick(string deviceID, string ImagePath, int delayPerCheck = 2000, int count = 5);
string GetDeviceName(string deviceID);
List<string> GetDevices();
Point GetScreenResolution(string deviceID);
void InputText(string deviceID, string text);
void Key(string deviceID, ADBKeyEvent key);
void LongPress(string deviceID, int x, int y, int duration = 100);
void PlanModeOFF(string deviceID, CancellationToken cancellationToken);
void PlanModeON(string deviceID, CancellationToken cancellationToken);
Bitmap ScreenShoot(string deviceID = null, bool isDeleteImageAfterCapture = true, string fileName = "screenShoot.png");
string SetADBFolderPath(string folderPath);
void Swipe(string deviceID, int x1, int y1, int x2, int y2, int duration = 100);
void SwipeByPercent(string deviceID, double x1, double y1, double x2, double y2, int duration = 100);
void Tap(string deviceID, int x, int y, int count = 1);
void TapByPercent(string deviceID, double x, double y, int count = 1);
void SetTextFromClipboard(string deviceID, string text);
```
<!-- Script Example -->
## Script Example
```cs
/*
* Some variables here
*/
public override void Init()
{
	setStatus("Script initialized");
}

public override void Start()
{
	// Your script code here
}
```
<!-- Used Libraries -->
## Used Libraries
```cs
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using KAutoHelper;
using xNet;
using MimeKit;
```

<!-- Library Documents -->
## Library Documents
* [HTTP Request - xNet 3.3.3](https://teamcodedao.com/forum/index.php?/topic/3-huong-dan-co-ban-ve-thu-vien-xnet-trong-csharp/)
* [JSON - Newtonsoft.Json 13.0.0.0](https://www.newtonsoft.com/json)
* [Email - MailKit 3.4.0.0](https://github.com/jstedfast/MailKit)