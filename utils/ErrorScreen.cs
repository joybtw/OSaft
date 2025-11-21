using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Drawing;
using Sys = Cosmos.System;

namespace OSaft.utils
{
    public static class ErrorScreen
    {
        public static void init(string progname, string[] logs)
        {
            Canvas canvas = FullScreenCanvas.GetFullScreenCanvas();
            canvas.Clear(Color.Red);
            string errormessage = @"
                                     _______   ________  ________  ________  ________     
                                    |\  ___ \ |\   __  \|\   __  \|\   __  \|\   __  \    
                                    \ \   __/|\ \  \|\  \ \  \|\  \ \  \|\  \ \  \|\  \   
                                     \ \  \_|/_\ \   _  _\ \   _  _\ \  \\\  \ \   _  _\  
                                      \ \  \_|\ \ \  \\  \\ \  \\  \\ \  \\\  \ \  \\  \| 
                                       \ \_______\ \__\\ _\\ \__\\ _\\ \_______\ \__\\ _\ 
                                        \|_______|\|__|\|__|\|__|\|__|\|_______|\|__|\|__|";

            string[] lines = errormessage.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Startkoordinaten
            int startX = 0;
            int startY = 0;
            int lineHeight = PCScreenFont.Default.Height; // Höhe einer Zeile in Pixeln

            // Schleife über jede Zeile
            for (int i = 0; i < lines.Length; i++)
            {
                canvas.DrawString(
                    lines[i],
                    PCScreenFont.Default,
                    new Pen(Color.Orange),
                    new Sys.Graphics.Point(startX, startY + i * lineHeight)
                );
            }

            canvas.DrawString(("Very Critical Error occured when executing " + progname + ". Please view the logs to evaluate the issue:"), PCScreenFont.Default, new Pen(Color.Black), new Sys.Graphics.Point(startX + 20, startY + (lines.Length + 1) * lineHeight));

            foreach (string l in logs)
            {
                canvas.DrawString((l), PCScreenFont.Default, new Pen(Color.Black), new Sys.Graphics.Point(startX + 20, startY + (lines.Length + 1) * lineHeight));
                startY += lineHeight + 10;
            }

            canvas.Display();

        }
    }
}
