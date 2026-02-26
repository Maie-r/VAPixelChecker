using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;
using VoiceAttack;

public class VoiceAttackPlugin
{
    public static string VA_DisplayName()
    {
        return "VAPixelChecker";
    }

    public static string VA_DisplayInfo()
    {
        return "Reads screen pixel color at given coordinates.";
    }

    public static Guid VA_Id()
    {
        return new Guid("90de18fd-b56f-4a6d-9800-aa98dc52c4c5");
    }

    public static void VA_Invoke1(dynamic vaProxy)
    {
        vaProxy.SetBoolean("PixelMatchResult", false);
        vaProxy.SetBoolean("PixelMatchCanceled", true);
        try
        {
            // Get arguments from VoiceAttack
            string contextBase = vaProxy.Context;
            if (String.IsNullOrEmpty(contextBase))
            {
                vaProxy.WriteToLog($"No parameters found, add ''x=value;y=value'' replacing value with your coordinates in the plugin context, or 'ping' to check the current hovered position", "red");
                return;
            }
            string context = contextBase.Replace(" ", "").ToLower();
            if (context == "ping")
            {
                PixelChecker.PingCoordinates(vaProxy);
                vaProxy.SetBoolean("PixelMatchCanceled", false);
                return;
            }

            PixelChecker.CheckPixelMatch(vaProxy);
        }
        catch (Exception ex)
        {
            vaProxy.WriteToLog("Unhanded PixelChecker error: " + ex.Message, "red");
        }
    }

    //static Boolean _stopVariableToMonitor = false;
    
    public static void VA_StopCommand()
    {
        //_stopVariableToMonitor = true;
    }

    public static void VA_Init1(dynamic vaProxy)
    {
        //vaProxy.Context = "x=0;y=0";
    }

    public static void VA_Exit1(dynamic vaProxy)
    {
        
    }
}

public static class PixelChecker
{
    public static string ExpectedString = "x=value; y=value; Match(R, G, B); tolerance=value";
    /// <summary>
    /// Catches the current position of the mouse and the color of the hovered pixel
    /// </summary>
    /// <param name="vaProxy"></param>
    public static void PingCoordinates(dynamic vaProxy)
    {
        var pos = System.Windows.Forms.Cursor.Position;
        int x = pos.X;
        int y = pos.Y;

        using (Bitmap bmp = new Bitmap(1, 1))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
            }

            Color pixel = bmp.GetPixel(0, 0);
            vaProxy.WriteToLog($"Mouse Pinged at coordinates X=({x}) Y=({y}), color: RGB({pixel.R}, {pixel.G}, {pixel.B})", "blue", "info");
        }
    }

    public static void CheckPixelMatch(dynamic vaProxy)
    {
        // ACTUAL check
        // Context: x=value;y=value;Match(R, G, B);tolerance=value
        string context = vaProxy.Context.Replace(" ", "").ToLower();
        Dictionary<string, string> values = new();

        string[] parameters = context.Split(";", StringSplitOptions.RemoveEmptyEntries);
        if (parameters.Length <= 1 || parameters.Length > 4)
        {
            vaProxy.WriteToLog($"Couldn't parse parameters, ensure you don't forget the ';' or add too many. Expected format: ''{PixelChecker.ExpectedString}''", "red");
            return;
        }
        foreach (var part in parameters)
        {
            if (part.StartsWith("match(") && part.EndsWith(")"))
            {
                values.Add("match", part);
                continue;
            }

            var split = part.Split('=', 2);

            if (split.Length == 2)
                values.Add(split[0], split[1]);
        }
        /*foreach (var kv in values)
        {
            vaProxy.WriteToLog($"{kv.Key}");
        }//*/

        if (!values.ContainsKey("x") || !values.ContainsKey("y") || !values.ContainsKey("match")) // absolutly required
        {
            vaProxy.WriteToLog($"Couldn't parse parameters, ensure you use the format: ''{PixelChecker.ExpectedString}''", "red");
            return;
        }

        var rgb = ParseRGB(values["match"], vaProxy);
        if (rgb == null || rgb!.Length != 3)
        {
            vaProxy.WriteToLog($"Couldn't parse RGB match parameters, ensure you use the format: ''{PixelChecker.ExpectedString}''", "red");
            return;
        }

        if (values.ContainsKey("tolerance"))
        {
            if (int.Parse(values["tolerance"]) < 0)
            {
                vaProxy.WriteToLog("Invalid Tolerance value, must be 0 or above" , "red");
                return;
            }
            vaProxy.SetBoolean("PixelMatchResult", PixelChecker.SinglePixelCheck(
                int.Parse(values["x"]),
                int.Parse(values["y"]),
                rgb, 
                int.Parse(values["tolerance"]),
                vaProxy
            ));
        }
        else
        {
            vaProxy.SetBoolean("PixelMatchResult", PixelChecker.SinglePixelCheck(
                int.Parse(values["x"]),
                int.Parse(values["y"]),
                rgb,
                vaProxy
            ));
        }
        
    }

    /// <summary>
    /// Checks a single pixel at the specified coordinates, and sets the RGB values to the pixelR, pixelG and pixelB VA variables
    /// </summary>
    /// <param name="x">Mouse X coordinate</param>
    /// <param name="y">Mouse Y coordinate</param>
    /// <param name="vaProxy">VoiceAttack proxy reference</param>
    public static bool SinglePixelCheck(int x, int y, int[] matchee, int tolerance, dynamic vaProxy)
    {
        if (x < 0 || y < 0)
        {
            vaProxy.WriteToLog("Invalid Coordinates, make sure X and Y are above 0", "red");
            return false;
        }

        using (Bitmap bmp = new Bitmap(1, 1))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
            }

            Color pixel = bmp.GetPixel(0, 0);

            int[] scannedrgb = { pixel.R, pixel.G, pixel.B };
            vaProxy.SetInt("pixelR", pixel.R);
            vaProxy.SetInt("pixelG", pixel.G);
            vaProxy.SetInt("pixelB", pixel.B);

            vaProxy.WriteToLog($"PixelChecker: Read at ({x},{y}), RGB({pixel.R}, {pixel.G}, {pixel.B})", "purple", "diamond");
            for (int i = 0; i < scannedrgb.Length; i++)
            {
                if (Math.Abs(scannedrgb[i] - matchee[i]) > tolerance)
                {
                    vaProxy.WriteToLog($"PixelChecker: Checked pixel does not match the passed color.", "red", "diamond");
                    vaProxy.SetBoolean("PixelMatchCanceled", false);
                    return false;
                }
            }
            vaProxy.WriteToLog($"PixelChecker: Checked pixel matches the passed color!", "green", "diamond");
            vaProxy.SetBoolean("PixelMatchCanceled", false);
            return true;
        }
    }

    public static bool SinglePixelCheck(int x, int y, int[] matchee, dynamic vaProxy)
    {
        return SinglePixelCheck(x, y, matchee, 0, vaProxy);
    }

    static int[] ParseRGB(string match, dynamic vaProxy)
    {
        var inside = match.Substring(6, match.Length - 7);
        // removes "Match(" and ")"

        string[] rgbParts = inside.Split(',');

        if (rgbParts.Length != 3)
        {
            vaProxy.WriteToLog("Match must contain exactly 3 values.", "red");
            return new int[0];
        }

        int[] res = new int[3];
        res[0] = int.Parse(rgbParts[0]);
        res[1] = int.Parse(rgbParts[1]);
        res[2] = int.Parse(rgbParts[2]);

        return res;
    }
}