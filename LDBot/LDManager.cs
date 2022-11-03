﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace LDBot
{
	public class LDManager
	{
		public static string ldConsole = ConfigurationManager.AppSettings["LDPath"] + "\\ldconsole.exe";
		public static string adb = ConfigurationManager.AppSettings["LDPath"] + "\\adb.exe";

		public static List<LDEmulator> listEmulator = new List<LDEmulator>();

		private static void getLDInfo(LDEmulator ld)
        {
			bool isStarted = false;
			int elapsedTime = 0;
			while (!isStarted)
			{
				Thread.Sleep(1000);
				elapsedTime++;
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
						Helper.raiseOnUpdateLDStatus(ld.Index,string.Format("ADB Connecting...{0}",connectCount));
						List<string> adbDevices = new List<string>(Helper.runCMD(adb, "devices").Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
						string adbDeviceInfo = adbDevices.FirstOrDefault(str => str.Contains((ld.Index * 2 + 5554).ToString()) || str.Contains((ld.Index * 2 + 5555).ToString()));
						if(adbDeviceInfo != null)
                        {

							adbDeviceInfo = adbDeviceInfo.Replace("device", "").Trim();
							Helper.raiseOnUpdateLDStatus(ld.Index, "Connecting to " + adbDeviceInfo);
							Helper.runCMD(adb, "connect " + adbDeviceInfo);
							Thread.Sleep(1000);
							Helper.raiseOnUpdateLDStatus(ld.Index, "Adb connected");
							ld.DeviceID = adbDeviceInfo;
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
			if (name.Length > 0)
			{
				LDManager.executeLdConsole(string.Format("add --name \"{0}\"", name));
				Helper.raiseOnUpdateMainStatus(string.Format("Create new LD Player \"{0}\" successfully", name));
			}
		}

		public static void cloneLD(string name, int fromIndex, string fromName)
		{
			if (name.Length > 0)
			{
				LDManager.executeLdConsole(string.Format("copy --name \"{0}\" --from {1}", name, fromIndex));
				Helper.raiseOnUpdateMainStatus(string.Format("Clone from \"{0}\" to \"{1}\" successfully", fromName, name));
			}
		}

		public static void changeLDInfo(int index)
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

				LDManager.executeLdConsole(string.Concat(new object[]
				{
					"modify --index ", index,
					" --imei ", imei,
					" --model \"", model.ToString(),
					"\" --manufacturer ", manufacturer.ToString(),
					" --pnumber ", pNumber,
					" --imsi ", imsi,
					" --simserial ", simserial,
					" --androidid ", androidid,
					" --resolution 320,480,120",
					" --cpu 1 --memory 1024",
					" --mac auto"
				}));

				Helper.raiseOnUpdateMainStatus(string.Format("New info: {0}, {1}, IMEI: {2}, Phone: {3}", manufacturer, model, imei, pNumber));
				Helper.raiseOnUpdateLDStatus(index, "Change LD info OK");
			}
			catch (Exception e)
			{
				Helper.raiseOnErrorMessage(e);
			}
		}

		public static void removeLD(int index)
        {
			try
            {
				LDManager.executeLdConsole(string.Format("remove --index {0}", index));
				LDManager.listEmulator.RemoveAt(LDManager.listEmulator.FindIndex((LDEmulator l) => l.Index == index));
				Helper.raiseOnUpdateMainStatus("Player deleted successful");
			}
			catch(Exception e)
            {
				Helper.raiseOnErrorMessage(e);
			}
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
					LDManager.executeLdConsole("sortWnd");
				});
				thread.IsBackground = true;
				thread.Name = "LD" + ld.Index.ToString();
				thread.Start();
			}
			catch(Exception e)
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
					Helper.raiseOnUpdateLDStatus(ld.Index, "Rebooting...");
					LDManager.executeLdConsole("reboot --index " + ld.Index);
					Thread.Sleep(3000);
					getLDInfo(ld);
					LDManager.executeLdConsole("sortWnd");
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
			foreach(LDEmulator ld  in listEmulator)
            {
				Helper.raiseOnUpdateLDStatus(ld.Index, "Stop");
				ld.isRunning = false;
				ld.TopHandle = new IntPtr(0);
				ld.BindHandle = new IntPtr(0);
				ld.pID = -1;
				ld.VboxPID = -1;
			}
		}

		public static void loadScript(LDEmulator ld)
        {
			if(ld != null)
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
					ld.botAction.Start();
				}
			}
			catch(Exception e)
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
					ld.botAction.Stop();
				}
			}
			catch (Exception e)
			{

				Helper.raiseOnUpdateLDStatus(ld.Index, "Err: " + e.Message);
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
	}
}