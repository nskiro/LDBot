using System;

namespace LDBotV2
{
    //Delegate to update status in Form Main
    public delegate void dlgUpdateMainStatus(string stt);
    public delegate void dlgErrorMessage(Exception stt);
    public delegate void dlgUpdateLDStatus(int ldIndex, string stt);
    public delegate void dlgWriteLog(string log);
    public delegate void dlgLoadListLD();
    public delegate string dlgGetLDStatus(int index);
}
