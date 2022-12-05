using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LDBot
{
	public class LDManager
	{
		public static string ldConsole = ConfigurationManager.AppSettings["LDPath"] + "\\ldconsole.exe";
		public static string adb = ConfigurationManager.AppSettings["LDPath"] + "\\adb.exe";

		public static List<LDEmulator> listEmulator = new List<LDEmulator>();

		private static List<Thread> ldScriptThreads = new List<Thread>();

		private static void getLDInfo(LDEmulator ld)
		{
			bool isStarted = false;
			int elapsedTime = 0;
			while (!isStarted)
			{
				Thread.Sleep(1000);
				elapsedTime++;
				if(elapsedTime > 60)
                {
					Helper.raiseOnUpdateLDStatus(ld.Index, "LD start failed");
					break;
				}					
				Helper.raiseOnUpdateLDStatus(ld.Index, string.Format("Starting...({0})", elapsedTime));
				string[] result = Helper.runCMD(ldConsole, "list2").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string line in result)
				{
					string[] ldInfo = line.Split(',');
					if (int.Parse(ldInfo[4]) == 1 && ldInfo[0] == ld.Index.ToString())
					{
						Helper.raiseOnUpdateLDStatus(ld.Index, "Running");
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
						Helper.raiseOnUpdateLDStatus(ld.Index, string.Format("ADB Connecting...{0}", connectCount));
						List<string> adbDevices = new List<string>(Helper.runCMD(adb, "devices").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
						string adbDeviceInfo = adbDevices.FirstOrDefault(str => str.Contains((ld.Index * 2 + 5554).ToString()) || str.Contains((ld.Index * 2 + 5555).ToString()));
						if (adbDeviceInfo != null)
						{
							adbDeviceInfo = adbDeviceInfo.Replace("device", "").Trim();
							Helper.raiseOnUpdateLDStatus(ld.Index, "Connecting to " + adbDeviceInfo);
							Helper.runCMD(adb, "connect " + adbDeviceInfo);
							Thread.Sleep(1000);
							Helper.raiseOnUpdateLDStatus(ld.Index, "Adb connected");
							ld.DeviceID = adbDeviceInfo;
							ld.isRunning = true;
							isADBConnected = true;
						}
						else
						{
							connectCount++;
							if (connectCount > 30)
							{
								Helper.raiseOnUpdateLDStatus(ld.Index, "Cannot adb connect");
								break;
							}
							Thread.Sleep(1000);
						}
					}
				}
			}
		}

		public static void getAllLD()
		{
			try
			{
				string[] result = Helper.runCMD(ldConsole, "list2").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
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
				Helper.raiseOnErrorMessage(e);
			}
		}

		public static void createLD(string name)
		{
			new Task(delegate
			{
				if (name.Length > 0)
				{
					Helper.raiseOnWriteLog(string.Format("Creating new LD Player \"{0}\". Please wait...", name));
					LDManager.executeLdConsole(string.Format("add --name \"{0}\"", name));
					Helper.raiseOnLoadListLD();
					Helper.raiseOnWriteLog(string.Format("Create new LD Player \"{0}\" successfully", name));
				}
			}).Start();
		}

		public static void cloneLD(string name, int fromIndex, string fromName)
		{
			new Task(delegate
			{
				if (name.Length > 0)
				{
					Helper.raiseOnWriteLog(string.Format("Cloning from \"{0}\" to \"{1}\". Please wait...", fromName, name ));
					LDManager.executeLdConsole(string.Format("copy --name \"{0}\" --from {1}", name, fromIndex));
					Helper.raiseOnLoadListLD();
					Helper.raiseOnWriteLog(string.Format("Clone from \"{0}\" to \"{1}\" successfully", fromName, name));
				}
			}).Start();
		}

		public static void changeLDInfo(int index)
		{
			new Task(delegate
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
					string imei = "86516602" + Helper.CreateRandomNumber(7, random);
					string pNumber = "1" + areaCode.ToString() + Helper.CreateRandomNumber(7, random);
					string imsi = "46000" + Helper.CreateRandomNumber(10, random);
					string simserial = "898600" + Helper.CreateRandomNumber(14, random);
					string androidid = Helper.Md5Encode(Helper.CreateRandomStringNumber(32, random), "x2").Substring(random.Next(0, 16), 16);

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
					LDManager.executeLdConsole(command);

					Helper.raiseOnWriteLog(command);
					Helper.raiseOnUpdateLDStatus(index, "Change LD info OK");
				}
				catch (Exception e)
				{
					Helper.raiseOnErrorMessage(e);
				}
			}).Start();
		}

		public static void removeLD(LDEmulator ld)
		{
			new Task(delegate
			{
				try
				{
					LDManager.executeLdConsole(string.Format("remove --index {0}", ld.Index));
					Directory.Delete(ld.ScriptFolder, true);
					LDManager.listEmulator.RemoveAt(LDManager.listEmulator.FindIndex((LDEmulator l) => l.Index == ld.Index));
					Helper.raiseOnWriteLog(string.Format("{0} deleted successful", ld.Name));
				}
				catch (Exception e)
				{
					Helper.raiseOnErrorMessage(e);
				}
			}).Start();
		}

		public static void runLD(LDEmulator ld)
		{
			try
			{
				Thread thread = new Thread((ThreadStart)delegate
				{
					Helper.raiseOnUpdateLDStatus(ld.Index, "Starting...");
					LDManager.executeLdConsole("launch --index " + ld.Index);
					getLDInfo(ld);
					//LDManager.executeLdConsole("sortWnd");
				});
				thread.IsBackground = true;
				thread.Name = "LD" + ld.Index.ToString();
				thread.Start();
				
			}
			catch (Exception e)
			{
				Helper.raiseOnErrorMessage(e);
			}
		}

		public static void restartLD(LDEmulator ld)
		{
			try
			{
				Thread thread = new Thread((ThreadStart)delegate
				{
					if (ld.isRunning)
					{
						Helper.raiseOnUpdateLDStatus(ld.Index, "Rebooting...");
						LDManager.executeLdConsole("reboot --index " + ld.Index);
						ld.isRunning = false;
						Thread.Sleep(3000);
						getLDInfo(ld);
					}
					else
						Helper.raiseOnUpdateLDStatus(ld.Index, "LD has not started");
				});
				thread.IsBackground = true;
				thread.Name = "LD" + ld.Index.ToString();
				thread.Start();
			}
			catch (Exception e)
			{
				Helper.raiseOnErrorMessage(e);
			}

		}

		public static void quitLD(int index)
		{
			LDManager.executeLdConsole("quit --index " + index);
			Helper.raiseOnUpdateLDStatus(index, "Stop");
		}

		public static void quitAll()
		{
			LDManager.executeLdConsole("quitall");
			foreach (LDEmulator ld in listEmulator)
			{
				Helper.raiseOnUpdateLDStatus(ld.Index, "Stop");
				ld.isRunning = false;
				ld.TopHandle = new IntPtr(0);
				ld.BindHandle = new IntPtr(0);
				ld.pID = -1;
				ld.VboxPID = -1;
			}
		}

		public static bool idLdRunning(int index)
        {
			string rs = executeLdConsoleForResult($"isrunning --index {index}").Trim();
			return rs == "running";
		}
		public static void loadScript(LDEmulator ld)
		{
			if (ld != null)
			{
				ld.GenerateCode();
			}
		}

		public static void startScript(LDEmulator ld)
		{
			try
			{
				if (ld != null)
				{
					Thread thread = new Thread((ThreadStart)delegate
					{
						ld.botAction.Start();
					});
					thread.IsBackground = true;
					thread.Name = "Script" + ld.Index.ToString();
					thread.Start();
					var checkThread = ldScriptThreads.FirstOrDefault(x => x.Name == "Script" + ld.Index.ToString());
					if (checkThread == null)
						ldScriptThreads.Add(thread);
				}
			}
			catch (Exception e)
			{

				Helper.raiseOnUpdateLDStatus(ld.Index, "Err: " + e.Message);
			}
		}

		public static void stopScript(LDEmulator ld)
		{
			try
			{
				if (ld != null)
				{
					var checkThread = ldScriptThreads.FirstOrDefault(x => x.Name == "Script" + ld.Index.ToString());
					if (checkThread != null)
                    {
						//ld.botAction.Stop();
						checkThread.Abort();
						ldScriptThreads.Remove(checkThread);
						Helper.raiseOnUpdateLDStatus(ld.Index, "Script aborted");
					}						
				}
			}
			catch (Exception e)
			{
				Helper.raiseOnUpdateLDStatus(ld.Index, "Err: " + e.Message);
			}
		}

		public static void installAPK(int index, string apkPath)
		{
			try
			{
				executeLdConsole("installapp --index " + index + " --filename " + apkPath);
				Helper.raiseOnUpdateLDStatus(index, "Install APK successful");
			}
			catch(Exception e)
            {
				Helper.raiseOnErrorMessage(e);
            }
		}

		public static void changeProxy(LDEmulator ld, string proxyConfig = "")
        {
			try
			{
				if (proxyConfig.Length > 0)
				{
					Helper.runCMD(adb, string.Format("-s {0} shell settings put global http_proxy {1}", ld.DeviceID, proxyConfig));
					ld.isUseProxy = true;
					Helper.raiseOnUpdateLDStatus(ld.Index, "Use HTTP proxy " + proxyConfig);
				}
				else
				{
					Helper.runCMD(adb, string.Format("-s {0} shell settings put global http_proxy :0", ld.DeviceID));
					ld.isUseProxy = false;
					Helper.raiseOnUpdateLDStatus(ld.Index, "Remove HTTP proxy");
				}
				ld.Proxy = proxyConfig;
			}
			catch(Exception e)
            {
				Helper.raiseOnErrorMessage(e);
            }			
        }
		
		public static void executeLdConsole(string cmd)
		{
			try
			{
				Process process = new Process();
				process.StartInfo.FileName = ldConsole;
				process.StartInfo.Arguments = cmd;
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.EnableRaisingEvents = true;
				process.Start();
				process.WaitForExit();
				process.Close();
			}
			catch(Exception e)
            {
				Helper.raiseOnErrorMessage(e);
			}			
		}

		public static string executeLdConsoleForResult(string cmdCommand, int timeout = 10000, int retry = 2)
		{
			string result;
			try
			{
				var process = new Process();
				process.StartInfo = new ProcessStartInfo
				{
					FileName = ldConsole,
					Arguments = cmdCommand,
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
			catch
			{
				result = null;
			}

			return result;
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
				Helper.raiseOnErrorMessage(e);
            }			
		}

        private static void Timer_Tick(object sender, EventArgs e, DateTime runTime, LDEmulator ld)
        {
			Helper.raiseOnUpdateLDStatus(ld.Index, string.Format("Wait {0} seconds to run script", (int)(runTime - DateTime.Now).TotalSeconds));
			if((int)(runTime - DateTime.Now).TotalSeconds == 0)
            {
				System.Windows.Forms.Timer t = sender as System.Windows.Forms.Timer;
				t.Stop();
				t.Enabled = false;
				startScript(ld);
            }			
        }
    }
}
