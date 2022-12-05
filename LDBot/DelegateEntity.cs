using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDBot
{
    //Delegate to update status in Form Main
    public delegate void dlgUpdateMainStatus(string stt);
    public delegate void dlgErrorMessage(Exception stt);
    public delegate void dlgUpdateLDStatus(int ldIndex, string stt);
    public delegate void dlgWriteLog(string log);
    public delegate void dlgLoadListLD();
    public delegate string dlgGetLDStatus(int index);
}
