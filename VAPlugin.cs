using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

/*
public class VAPlugin
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
        try
        {
            // Get arguments from VoiceAttack
            int x = Convert.ToInt32(vaProxy.GetText("X"));
            int y = Convert.ToInt32(vaProxy.GetText("Y"));

            using (Bitmap bmp = new Bitmap(1, 1))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
                }

                Color pixel = bmp.GetPixel(0, 0);

                vaProxy.SetInt("pixelR", pixel.R);
                vaProxy.SetInt("pixelG", pixel.G);
                vaProxy.SetInt("pixelB", pixel.B);

                vaProxy.WriteToLog($"PixelReader: Read at ({x},{y}), RGB({pixel.R}, {pixel.G}, {pixel.B})");
            }
        }
        catch (Exception ex)
        {
            vaProxy.WriteToLog("PixelReader error: " + ex.Message);
        }
    }
}//*/