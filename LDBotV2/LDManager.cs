using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LDBotV2
{
	public class LDManager
	{
		private static string ldConsole = ConfigurationManager.AppSettings["LDPath"] + "ldconsole.exe";
		private static string adb = ConfigurationManager.AppSettings["LDPath"] + "adb.exe";
		public static List<LDEmulator> listEmulator = new List<LDEmulator>();

		public static void executeLdConsole(string cmdCommand)
		{
			ToolHelper.runCMD(ldConsole, cmdCommand);
		}

		public static string executeLdConsoleForResult(string cmdCommand, int timeout = 10000, int retry = 2)
		{
			return ToolHelper.runCMDForResult(ldConsole, cmdCommand, timeout, retry);
		}

		public static void executeADB(string cmdCommand)
		{
			ToolHelper.runCMD(adb, cmdCommand);
		}

		public static string executeADBForResult(string cmdCommand, int timeout = 10000, int retry = 2)
		{
			return ToolHelper.runCMDForResult(adb, cmdCommand, timeout, retry);
		}
		private async static void getLDInfo(LDEmulator ld)
		{
			bool isStarted = false;
			int elapsedTime = 0;
			while (!isStarted)
			{
				await Task.Delay(1000);
				elapsedTime++;
				if (elapsedTime > 60)
				{
					LDHelper.raiseOnUpdateLDStatus(ld.Index, "Khởi động thất bại");
					break;
				}
				LDHelper.raiseOnUpdateLDStatus(ld.Index, string.Format("Đang khởi động...({0})", elapsedTime));
				string[] result = executeLdConsoleForResult("list2").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string line in result)
				{
					string[] ldInfo = line.Split(',');
					if (int.Parse(ldInfo[4]) == 1 && ldInfo[0] == ld.Index.ToString())
					{
						LDHelper.raiseOnUpdateLDStatus(ld.Index, "Đang chạy");
						LDEmulator obj = listEmulator.FirstOrDefault(_ld => _ld.Index == ld.Index);
						if (obj != null)
						{
							obj.isRunning = true;
							obj.TopHandle = new IntPtr(int.Parse(ldInfo[2]));
							obj.BindHandle = new IntPtr(int.Parse(ldInfo[3]));
							obj.pID = int.Parse(ldInfo[5]);
							obj.VboxPID = int.Parse(ldInfo[6]);
						}
						isStarted = true;
						break;
					}
				}
				if (isStarted)
				{
					bool isADBConnected = false;
					int connectCount = 0;
					while (!isADBConnected)
					{
						LDHelper.raiseOnUpdateLDStatus(ld.Index, string.Format("Đang kết nối...{0}", connectCount));
						List<string> adbDevices = new List<string>(executeADBForResult("devices").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
						string adbDeviceInfo = adbDevices.FirstOrDefault(str => str.Contains((ld.Index * 2 + 5554).ToString()) || str.Contains((ld.Index * 2 + 5555).ToString()));
						if (adbDeviceInfo != null)
						{
							adbDeviceInfo = adbDeviceInfo.Replace("device", "").Trim();
							LDHelper.raiseOnUpdateLDStatus(ld.Index, "Đang kết nối với " + adbDeviceInfo);
							executeADB("connect " + adbDeviceInfo);
							await Task.Delay(1000);
							LDHelper.raiseOnUpdateLDStatus(ld.Index, "Kết nối thành công");
							ld.DeviceID = adbDeviceInfo;
							ld.isRunning = true;
							isADBConnected = true;
						}
						else
						{
							connectCount++;
							if (connectCount > 30)
							{
								LDHelper.raiseOnUpdateLDStatus(ld.Index, "Không thể kết nối");
								break;
							}
							await Task.Delay(1000);
						}
					}
				}
			}
		}
		public static void getAllLD()
		{
			try
			{
				string[] result = executeLdConsoleForResult("list2").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string line in result)
				{
					string[] ldInfo = line.Split(',');
					if (!listEmulator.Any(ld => ld.Index == int.Parse(ldInfo[0])))
					{
						listEmulator.Add(new LDEmulator(
							int.Parse(ldInfo[0]),
							ldInfo[1],
							new IntPtr(Convert.ToInt32(ldInfo[2], 16)),
							new IntPtr(Convert.ToInt32(ldInfo[3], 16)),
							int.Parse(ldInfo[4]) == 1,
							int.Parse(ldInfo[5]),
							int.Parse(ldInfo[6])
						));
					}
				}
			}
			catch (Exception e)
			{
				ToolHelper.raiseOnWriteError(e);
			}
		}

		public static void createLD(string name)
		{
			Task.Run(() =>
			{
				try
				{
					if (name.Length > 0)
					{
						ToolHelper.raiseOnWriteLog(string.Format("Đang tạo mới giả lập \"{0}\". Vui lòng chờ...", name));
						executeLdConsole(string.Format("add --name \"{0}\"", name));
						LDHelper.raiseOnLoadListLD();
						ToolHelper.raiseOnWriteLog(string.Format("Giả lập \"{0}\" được tạo thành công", name));
					}
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}

		public static void cloneLD(string name, int fromIndex, string fromName)
		{
			Task.Run(() =>
			{
				try
				{
					if (name.Length > 0)
					{
						ToolHelper.raiseOnWriteLog(string.Format("Đang sao chép từ \"{0}\" đến \"{1}\". Vui lòng chờ...", fromName, name));
						executeLdConsole(string.Format("copy --name \"{0}\" --from {1}", name, fromIndex));
						LDHelper.raiseOnLoadListLD();
						ToolHelper.raiseOnWriteLog(string.Format("Sao chép \"{0}\" đến \"{1}\" thành công.", fromName, name));
					}
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}

		public static void changeLDInfo(int index)
		{
			Task.Run(() =>
			{
				try
				{

					Random random = new Random();
					JToken arrDeviceInfo = JToken.Parse(File.ReadAllText("DATA/deviceinfo.json"));
					JToken deviceInfo = arrDeviceInfo[random.Next(0, arrDeviceInfo.Count() - 1)];
					JToken manufacturer = deviceInfo["manufacturer"];
					JToken models = deviceInfo["models"];
					JToken model = models[random.Next(0, models.Count() - 1)];
					JToken arrAreaCode = JToken.Parse(File.ReadAllText("DATA/areacode.json"));
					JToken country = arrAreaCode[0];
					JToken areaCodes = country["areacode"];
					JToken areaCode = areaCodes[random.Next(0, areaCodes.Count() - 1)];
					string imei = "86516602" + DataHelper.CreateRandomNumber(7, random);
					string pNumber = "1" + areaCode.ToString() + DataHelper.CreateRandomNumber(7, random);
					string imsi = "46000" + DataHelper.CreateRandomNumber(10, random);
					string simserial = "898600" + DataHelper.CreateRandomNumber(14, random);
					string androidid = DataHelper.Md5Encode(DataHelper.CreateRandomStringNumber(32, random), "x2").Substring(random.Next(0, 16), 16);

					string command = string.Concat(new string[]
					{
						"modify --index ", index.ToString(),
						" --imei ", imei,
						" --model \"", model.ToString(),
						"\" --manufacturer ", manufacturer.ToString(),
						" --pnumber ", pNumber,
						" --imsi ", imsi,
						" --simserial ", simserial,
						" --androidid ", androidid,
						" --mac auto"
					});
					executeLdConsole(command);
					ToolHelper.raiseOnWriteLog(command);
					LDHelper.raiseOnUpdateLDStatus(index, model.ToString());
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}

		public static void removeLD(LDEmulator ld)
		{
			Task.Run(() =>
			{
				try
				{
					executeLdConsole(string.Format("remove --index {0}", ld.Index));
					Directory.Delete(ld.ScriptFolder, true);
					listEmulator.RemoveAt(listEmulator.FindIndex((LDEmulator l) => l.Index == ld.Index));
					ToolHelper.raiseOnWriteLog(string.Format("Đã xóa giả lập {0}", ld.Name));
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}

		public static void startLD(LDEmulator ld)
		{
			Task.Run(() =>
			{
				try
				{
					executeLdConsole("launch --index " + ld.Index);
					getLDInfo(ld);
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}
		public static void restartLD(LDEmulator ld)
		{
			Task.Run(() =>
			{
				try
				{
					if (ld.isRunning)
					{
						LDHelper.raiseOnUpdateLDStatus(ld.Index, "Đang khởi động lại...");
						executeLdConsole("reboot --index " + ld.Index);
						ld.isRunning = false;
						Task.Delay(TimeSpan.FromSeconds(3)).Wait();
						getLDInfo(ld);
					}
					else
						LDHelper.raiseOnUpdateLDStatus(ld.Index, "Giả lập chưa khởi động");
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}
		public static void quitLD(LDEmulator ld)
		{
			Task.Run(() =>
			{
				try
				{
					executeLdConsole("quit --index " + ld.Index);
					ld.isRunning = false;
					//Stop Script if script is running
					LDHelper.raiseOnUpdateLDStatus(ld.Index, "Đã thoát giả lập");
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}

		public static void loadScript(LDEmulator ld)
		{
			try
			{
				if (ld != null)
				{
					ld.GenerateCode();
				}
			}
			catch (Exception e)
			{
				ToolHelper.raiseOnWriteError(e);
			}
		}

		public static void startScript(LDEmulator ld)
        {
			Task.Run(() =>
			{
				try
				{
					if (ld != null)
					{
						ld.botAction.PreStart();
					}
				}
				catch (Exception e)
				{
					ToolHelper.raiseOnWriteError(e);
				}
			});
		}
		public static void stopScript(LDEmulator ld)
        {
			try
			{
				if (ld != null)
				{
					ld.botAction.Stop();
				}
			}
			catch (Exception e)
			{
				ToolHelper.raiseOnWriteError(e);
			}
		}
		public static void changeProxy(LDEmulator ld, string proxyConfig = "")
		{
			try
			{
				if(ld.isRunning)
				{
					if (proxyConfig.Length > 0)
					{
						executeADB(string.Format("-s {0} shell settings put global http_proxy {1}", ld.DeviceID, proxyConfig));
						ld.isUseProxy = true;
						LDHelper.raiseOnUpdateLDStatus(ld.Index, "Proxy: " + proxyConfig);
					}
					else
					{
						executeADB(string.Format("-s {0} shell settings put global http_proxy :0", ld.DeviceID));
						ld.isUseProxy = false;
						LDHelper.raiseOnUpdateLDStatus(ld.Index, "Xóa proxy");
					}
					ld.Proxy = proxyConfig;
				}					
				else
                {
					LDHelper.raiseOnUpdateLDStatus(ld.Index, "Chạy giả lập trước khi thêm proxy");
                }					
			}
			catch (Exception e)
			{
				ToolHelper.raiseOnWriteError(e);
			}
		}
		public static void scheduleScript(LDEmulator ld, DateTime runTime)
		{
			try
			{
				System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
				timer.Interval = 1000;
				timer.Tick += (object sender, EventArgs e) => Timer_Tick(sender, e, runTime, ld);
				timer.Enabled = true;
				timer.Start();
			}
			catch (Exception e)
			{
				ToolHelper.raiseOnWriteError(e);
			}
		}

		private static void Timer_Tick(object sender, EventArgs e, DateTime runTime, LDEmulator ld)
		{
			LDHelper.raiseOnUpdateLDStatus(ld.Index, string.Format("Script bắt đầu sau {0} giây", (int)(runTime - DateTime.Now).TotalSeconds));
			if ((int)(runTime - DateTime.Now).TotalSeconds == 0)
			{
				System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
				t.Stop();
				t.Enabled = false;
				startScript(ld);
			}
		}
	}
}
