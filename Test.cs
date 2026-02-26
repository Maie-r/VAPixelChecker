using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

public class VoiceAttackPlugin
{
    public static string VA_DisplayName()
        => "TestPlugin";

    public static string VA_DisplayInfo()
        => "If you see this, plugin works.";

    public static Guid VA_Id()
        => new Guid("{B5A3D0A1-2F4E-4E5A-9C11-8C2E9C123406}");

    public static void VA_Invoke1(dynamic vaProxy)
    {
        vaProxy.WriteToLog("TestPlugin invoked!");
    }
}