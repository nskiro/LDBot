using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace LDBotV2
{

    public class LDEmulator : IEquatable<LDEmulator>
    {
        private int _index;
        private string _name;
        private IntPtr _topHandle;
        private IntPtr _bindHandle;
        private bool _isRunning;
        private bool _isUseProxy;
        private string _proxy;
        private int _pID;
        private int _vBoxPID;
        public BotAction botAction;
        private readonly string _scriptFolder;
        private string _deviceID;
        

        #region Constructor

        public LDEmulator() { }

        public LDEmulator(int index, string name, IntPtr topHandle, IntPtr bindHandle, bool isRunning, int pID, int vBoxPID)
        {
            _index = index;
            _name = name;
            _topHandle = topHandle;
            _bindHandle = bindHandle;
            _isRunning = isRunning;
            _pID = pID;
            _vBoxPID = vBoxPID;
            botAction = new BotAction(this);
            _scriptFolder = ConfigurationManager.AppSettings["LDPath"] + "Scripts\\" + _name;
            _isUseProxy = false;
            _proxy = "";
            if (!Directory.Exists(_scriptFolder))
            {
                Directory.CreateDirectory(_scriptFolder);
            }
        }
        #endregion

        #region Get_Set
        public string Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }
        public bool isUseProxy
        {
            get { return _isUseProxy; }
            set { _isUseProxy = value; }

        }
        public string ScriptFolder
        {
            get { return _scriptFolder; }
        }
        public string DeviceID
        {
            get { return _deviceID; }
            set { _deviceID = value; }
        }
        public int VboxPID
        {
            get { return _vBoxPID; }
            set { _vBoxPID = value; }
        }

        public int pID
        {
            get { return _pID; }
            set { _pID = value; }
        }

        public bool isRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public IntPtr BindHandle
        {
            get { return _bindHandle; }
            set { _bindHandle = value; }
        }

        public IntPtr TopHandle
        {
            get { return _topHandle; }
            set { _topHandle = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        #endregion
        public bool Equals(LDEmulator other)
        {
            return other.Index == Index;
        }

        public void GenerateCode()
        {
            try
            {
                Dictionary<string, CompilerErrorCollection> e = new Dictionary<string, CompilerErrorCollection>();
                string[] files = Directory.GetFiles(_scriptFolder, "*.cs");
                List<ScriptFileNameEntity> scriptFileList = new List<ScriptFileNameEntity>();
                if (files.Length != 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    int num = 8;
                    for (int index = 0; index < files.Length; ++index)
                    {
                        FileInfo fileInfo = new FileInfo(files[index]);
                        string str = System.IO.File.ReadAllText(files[index]);
                        int length = str.Split('\n').Length;
                        scriptFileList.Add(new ScriptFileNameEntity()
                        {
                            BeginLine = num,
                            EndLine = num + length,
                            FileName = fileInfo.Name
                        });
                        num += length;
                        stringBuilder.AppendLine(str);
                    }
                    GenerateCode(e, stringBuilder.ToString().Replace("System.Reflection.", "").Replace("System.CodeDom.Compiler", ""), scriptFileList);
                }
                else
                    LDHelper.raiseOnUpdateLDStatus(_index, "Không tìm thấy script");
            }
            catch(Exception e)
            {
                ToolHelper.raiseOnWriteError(e);
            }
        }

        private void GenerateCode(Dictionary<string, CompilerErrorCollection> e, string ItemName, List<ScriptFileNameEntity> isMainPlayer)
        {
            CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider();
            CompilerParameters options = new CompilerParameters() { GenerateInMemory = true, GenerateExecutable = false };
            options.ReferencedAssemblies.Add("system.dll");
            options.ReferencedAssemblies.Add("system.core.dll");
            options.ReferencedAssemblies.Add("system.data.dll");
            options.ReferencedAssemblies.Add("System.Threading.Tasks.dll");
            options.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            options.ReferencedAssemblies.Add("System.Drawing.dll");
            options.ReferencedAssemblies.Add("Emgu.CV.UI.dll");
            options.ReferencedAssemblies.Add("Emgu.CV.World.dll");
            options.ReferencedAssemblies.Add("ZedGraph.dll");
            options.ReferencedAssemblies.Add("KAutoHelper.dll");
            options.ReferencedAssemblies.Add("xNet.dll");
            options.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
            options.ReferencedAssemblies.Add("MailKit.dll");
            options.ReferencedAssemblies.Add("MimeKit.dll");
            options.ReferencedAssemblies.Add("LDBotV2.exe");
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Linq;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using System.Windows.Forms;");
            stringBuilder.AppendLine("using System.Text;");
            stringBuilder.AppendLine("using System.Text.RegularExpressions;");
            stringBuilder.AppendLine("using System.Drawing;");
            stringBuilder.AppendLine("using System.Drawing.Imaging;");
            stringBuilder.AppendLine("using System.Threading;");
            stringBuilder.AppendLine("using System.Threading.Tasks;");
            stringBuilder.AppendLine("using System.IO;");
            stringBuilder.AppendLine("using System.Runtime.InteropServices;");
            stringBuilder.AppendLine("using Newtonsoft.Json;");
            stringBuilder.AppendLine("using KAutoHelper;");
            stringBuilder.AppendLine("using xNet;");
            stringBuilder.AppendLine("using MailKit;");
            stringBuilder.AppendLine("using MimeKit;");
            stringBuilder.AppendLine("namespace LDBotV2 {");
            stringBuilder.AppendLine("class AutoScriptExternalClass:BotAction {");
            stringBuilder.AppendLine("public AutoScriptExternalClass(LDEmulator _ld) : base(_ld) { }");
            stringBuilder.AppendLine(ItemName);
            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine("}");
            stringBuilder.ToString();
            CompilerResults results = csharpCodeProvider.CompileAssemblyFromSource(options, stringBuilder.ToString());
            if (results.Errors.Count == 0 && results.CompiledAssembly != (Assembly)null)
            {
                Type type = results.CompiledAssembly.GetType("LDBotV2.AutoScriptExternalClass");
                try
                {
                    if (type != (System.Type)null)
                    {
                        botAction = (BotAction)Activator.CreateInstance(type, this);
                        botAction.Init();
                    }
                }
                catch (Exception ex)
                {
                    ToolHelper.raiseOnWriteError(ex);
                }
            }
            else
            {
                ToolHelper.raiseOnWriteError(new Exception(string.Format("Script has {0} error(s)", results.Errors.Count)));
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    ScriptFileNameEntity scriptFileNameEntity = isMainPlayer.Find((Predicate<ScriptFileNameEntity>)(obj0 =>
                    {
                        if (results.Errors[i].Line >= obj0.BeginLine)
                            return results.Errors[i].Line <= obj0.EndLine;
                        return false;
                    }));
                    if (scriptFileNameEntity != null)
                    {
                        results.Errors[i].FileName = scriptFileNameEntity.FileName;
                        results.Errors[i].Line -= scriptFileNameEntity.BeginLine + 14;
                    }
                }
                e.Add("Global", results.Errors);
                new FormError(e).Show();
            }
        }
    }
}
