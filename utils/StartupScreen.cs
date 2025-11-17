using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Sys = Cosmos.System;

namespace OSaft.utils
{
    public static class StartupScreen
    {
        const string OS_TITLE = "OSaft";
        const string OS_VERSION = "v0.1";
        private static bool IsActive = false;
        private static readonly string path = "0:\\system\\startscreen.cfg";

        public static void Init()
        {
            if (!File.Exists(path))
            {
                FileSystem.CreateFile(path);
                FileSystem.WriteFile(path, "StartScreen = true\n", true);
                IsActive = true;
            }
            else
            {
                string content = FileSystem.ReadFile(@"0:\system\startscreen.cfg", true) ?? "StartScreen = false\n";
                IsActive = content == "StartScreen = true\n";
            }

            if (IsActive)
            {
                Canvas canvas = FullScreenCanvas.GetFullScreenCanvas();
                canvas.Clear();
                string orange = @"
                                               @@@@=    #@@##****@@                                 
                                              @%####@ *@##****#******@@                               
                                              @###*#@@##*****************@@                           
                                               @%#*@#@@*******@@@************@@                       
                                            @@@#@%*#@#***@******@****@@*********@                     
                                       @@#******@###%@#***@**************@@******@                    
                                    @@+**********%###@@########**#******%*********@                   
                                  @*******+++++*@@###@**@#########****************@                  
                                @******+++++++++*@@@@@%*****#@@@############*******#                 
                              @******+++++++++@++++++++++@+++*****%@@##########****@                 
                             @*****++++++++++++++++++++++++++++******+**#@@@@@@@@@%@                 
                           =@*****++++++++++++++++++++++++++++++++++++******@*                       
                          +@****+++++++++++++++++++++++++++++++++++++++++++++#*                      
                          @****+++++++++++++++++++++++++++++++++++++++++++++++@                      
                         @*****++++++++++++++++++++++++++++++++++++++++++++++++@                     
                        @+****++++++++++++++++++++++++++++++++++++++++++++++++++@                    
                        @*****++++++++++++++++++++++++++++++++++++++++++++++++++@                    
                        @****+++++++++++++++++++++++++++++++++++++++++++++++++++@                    
                       +*****++++++++++++++++++++++++++++++++++++++++++++++++++++*                   
                       %+****++++++++++++++++++++++++++++++++++++++++++++++++++++@                   
                       +*****++++++++++++++++++++++++++++++++++++++++++++++++++++*                   
                        @****+++++++++++++++++++++++++++++++++++++++++++++++++++@  *@@@@@@@@@@       
                        @****+++++++++++++++++++++++++++++++++++*@@@@@@@@#=+@   *++=====+++   @=     
                        @+****+++++++++++++++++++++++++++@#@=   ++++++++==++@+*++@+=+@#++++%  =@     
                         @***+++++++++++++++++++++++++*@%+++@=  @+++++==++++=+++@++@++===++@=  @@    
                          @+***+++++++++++++++++++++**@+*++++@=  @+++++@+++==@+==@+=+@++++++@  =@%   
                          %%*****++++++++++++++++++**@**++++++@=  #++@+++++=+*++=+++==+@+++++  =@@   
                           @@*****+++++++++++++++*+*@***+++++++@=   ++++++==++++==++++=++++++   @@   
                            =@*******++++++++++**++*@***++++++++@=   @++++++@+++==+@++++++++%   @@   
                              @***+*****+++++*******@***++++++++++@=   @++++++++++++@+++++++%  =@@   
                               =@*******************@****++++++++++@@=   @+++++++++++++++++@   @+@   
                                 +@*********+********@***++++++++++++@@=    @++++++++++++@    @+*@   
                                    @@****************@++*+++++++++++++@@=        +%%+      =@+*@    
                                       @@*************+@****++++++++++++++@@==            =@*+*@     
                                           @@@@+*****+***@*****+++++++++++++++@@@@*=+%@@@+++*%@      
                                                          #@*******++++++++++++++++++++++**@@        
                                                             @@**************++++*******@@           
                                                                @@@+*******+******+@@@               
                                                                       *@@@@@%=                      
                                             _____   ____                 ___  __      
                                            /\  __`\/\  _`\             /'___\/\ \__   
                                            \ \ \/\ \ \,\L\_\     __   /\ \__/\ \ ,_\  
                                             \ \ \ \ \/_\__ \   /'__`\ \ \ ,__\\ \ \/  
                                              \ \ \_\ \/\ \L\ \/\ \L\.\_\ \ \_/ \ \ \_ 
                                               \ \_____\ `\____\ \__/.\_\\ \_\   \ \__\
                                                \/_____/\/_____/\/__/\/_/ \/_/    \/__/
                ";

                // Aufteilen in Zeilen
                string[] lines = orange.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

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

                canvas.Display();
                Thread.Sleep(10000);
                canvas.Disable();

                //---------------------------------------------------

                ShowSplash();
                WriteCenteredLine("Press any key to start...", Console.WindowHeight / 2 + 5, ConsoleColor.White);
                Console.ReadKey(true);
            }
        }

        public static void ShowSplash()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            string title = OS_TITLE;
            string version = OS_VERSION;
            string subtitle = "Welcome to OSaft!";

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            //Console.Write("\x1b[38;2;0;0;0m");
            //Console.Write("\x1b[48;2;255;165;0m");
            Console.Clear();

            int boxW = Math.Min(50, width - 4);
            int boxH = 7;
            int boxX = (width - boxW) / 2;
            int boxY = (height - boxH) / 2;

            for (int y = 0; y < boxH; y++)
            {
                for (int x = 0; x < boxW; x++)
                {
                    Console.SetCursorPosition(boxX + x, boxY + y);
                    if (y == 0 || y == boxH - 1 || x == 0 || x == boxW - 1)
                        Console.Write('*');
                    else
                        Console.Write(' ');
                }
            }

            WriteCenteredInBox(title, boxX, boxY, boxW, 2, ConsoleColor.Red);
            WriteCenteredInBox(version, boxX, boxY, boxW, 3, ConsoleColor.DarkGray);
            WriteCenteredInBox(subtitle, boxX, boxY, boxW, 5, ConsoleColor.Gray);
        }

        public static void WriteCenteredInBox(string text, int boxX, int boxY, int boxW, int lineIndex, ConsoleColor color)
        {
            int y = boxY + lineIndex;
            int x = boxX + (boxW - text.Length) / 2;
            Console.ForegroundColor = color;
            Console.SetCursorPosition(Math.Max(boxX + 1, x), y);
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteCenteredLine(string text, int y, ConsoleColor color)
        {
            int x = (Console.WindowWidth - text.Length) / 2;
            Console.ForegroundColor = color;
            Console.SetCursorPosition(Math.Max(0, x), y);
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Toggle()
        {
            {

                if (!PermissionManager.CanWrite(path, UserDatabase.GetUserID(UserHelper.CurrentUser)))
                {
                    Console.WriteLine("Error: No permission to use this command.");
                    return;
                }
                IsActive = !IsActive;
                string content = IsActive ? "StartScreen = true\n" : "StartScreen = false\n";
                FileSystem.WriteFile(path, content, true);
            }
        }

    }
}
