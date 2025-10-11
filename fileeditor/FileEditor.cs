using OSaft.filesystem;
using System;
using System.Collections.Generic;

namespace OSaft.fileeditor
{
    internal class FileEditor
    {
        // Constant
        private static int[] console = { 0, 10/*Console.WindowWidth - 1*/, 1, 10/*Console.WindowHeight-2*/ }; // minX, maxX, minY, maxY -> Area of the Console that is for printing
        private static int consoleWidth = console[1] - console[0] + 1;
        private static int consoleHeight = console[3] - console[2] + 1;

        private static void CursorToConsole(int cursorX, int cursorY, int viewportX, int viewportY)
        {
            Console.SetCursorPosition(cursorX - viewportX + console[0], cursorY - viewportY + console[2]);
        }
        

        private static void PrintViewport(List<string> lines, ref int viewportX, ref int viewportY, ref int cursorX, ref int cursorY) {
            Console.Clear();
            cursorY = Math.Clamp(cursorY, 0, lines.Count - 1);
            cursorX = Math.Clamp(cursorX, 0, lines[cursorY].Length);

            // Move viewport to the marker
            viewportX = Math.Min(Math.Max(cursorX - consoleWidth + 1, 0), cursorX);
            viewportY = Math.Min(Math.Max(cursorY - consoleHeight + 1, 0), cursorY);

            // Print only characters in the viewport
            for (int Y = viewportY; Y < Math.Min(lines.Count, viewportY + consoleHeight); Y++) {
                string line = lines[Y];
                int start = Math.Min(viewportX, line.Length);
                int length = Math.Min(consoleWidth, line.Length - start);

                Console.SetCursorPosition(console[0], console[2]+Y);

                if (length > 0) {
                    Console.Write(line.Substring(start, length));
                } else {
                    Console.Write("");
                }
            }

            CursorToConsole(cursorX, cursorY, viewportX, viewportY);
        }

        public static void EditFile(string path)
        {
            // Import the File
            string file = FileSystem.ReadFile(path);
            List<string> lines = new List<string>(file.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            if (lines.Count == 0) {
                lines.Add("");
            }

            // Position
            int cursorX = 0;
            int cursorY = 0;
            int viewportX = 0;
            int viewportY = 0;

            string buffer = "";

            Console.Clear();
            PrintViewport(lines, ref viewportX, ref viewportY, ref cursorX, ref cursorY);

            while (true)
            {
                var key = Console.ReadKey(true);
                bool ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
                bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                bool alt = (key.Modifiers & ConsoleModifiers.Alt) != 0;

                switch (key.Key)
                {
                    //File Handling
                    case ConsoleKey.S when ctrl:
                        Console.WriteLine("Input: Save current file");
                        //file = string.Join(Environment.NewLine, lines);
                        FileSystem.WriteFile(path, file);
                        break;
                    case ConsoleKey.X when ctrl:
                        Console.WriteLine("Input: Close buffer, exit from nano");
                        Console.Clear();
                        return;
                    //Editing
                    case ConsoleKey.K when ctrl:
                        Console.WriteLine("Input: Cut current line into cutbuffer");
                        buffer = lines[cursorY];
                        break;
                    case ConsoleKey.D6 when alt:
                        Console.WriteLine("Input: Copy current line into cutbuffer");
                        buffer = lines[cursorY];
                        break;
                    case ConsoleKey.U when ctrl:
                        Console.WriteLine("Input: Paste contents of cutbuffer");
                        break;
                    case ConsoleKey.U when alt:
                        Console.WriteLine("Input: Undo last action");
                        break;
                    case ConsoleKey.E when alt:
                        Console.WriteLine("Input: Redo last undone action");
                        break;
                    //Search and Replace
                    case ConsoleKey.B when ctrl:
                        Console.WriteLine("Input: Start backward search");
                        break;
                    case ConsoleKey.F when ctrl:
                        Console.WriteLine("Input: Start forward search");
                        break;
                    case ConsoleKey.B when alt:
                        Console.WriteLine("Input: Find next occurrence backward");
                        break;
                    case ConsoleKey.F when alt:
                        Console.WriteLine("Input: Find next occurrence forward");
                        break;
                    case ConsoleKey.R when alt:
                        Console.WriteLine("Input: Start a replacing session");
                        break;
                    //Deletion
                    case ConsoleKey.H when ctrl:
                        Console.WriteLine("Input: Delete character before cursor");
                        break;
                    case ConsoleKey.D when ctrl:
                        Console.WriteLine("Input: Delete character under cursor");
                        break;
                    case ConsoleKey.Backspace when alt:
                        Console.WriteLine("Input: Delete word to the left");
                        break;
                    case ConsoleKey.Delete when ctrl:
                        Console.WriteLine("Input: Delete word to the right");
                        break;
                    case ConsoleKey.Delete when alt:
                        Console.WriteLine("Input: Delete current line");
                        break;
                    //Operations
                    //Moving around
                    case ConsoleKey.LeftArrow when ctrl:
                        Console.WriteLine("Input: One word backward");
                        while(cursorX > 0 && lines[cursorY][cursorX - 1] != ' ')
                        {
                            cursorX--;
                        }
                        break;
                    case ConsoleKey.RightArrow when ctrl:
                        Console.WriteLine("Input: One word forward");
                        while(cursorX < lines[cursorY].Length && lines[cursorY][cursorX+1] != ' ')
                        {
                            cursorX++;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("Input: One character backward");
                        if(cursorX > 0)
                        {
                            cursorX--;
                        } else
                        {
                            if(cursorY > 0)
                            {
                                cursorY--;
                                cursorX = lines[cursorY].Length;
                            }
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        Console.WriteLine("Input: One character forward");
                        if (cursorX < lines[cursorY].Length) {
                            cursorX++;
                        } else if (cursorY < (lines.Count - 1)) {
                            cursorY++;
                            cursorX = 0;
                        }
                        break;
                    case ConsoleKey.A when ctrl:
                        Console.WriteLine("Input: To start of line");
                        cursorX = 0;
                        break;
                    case ConsoleKey.E when ctrl:
                        Console.WriteLine("Input: To end of line");
                        cursorX = lines[cursorY].Length;
                        break;
                    case ConsoleKey.P when ctrl:
                        Console.WriteLine("Input: One line up");
                        cursorY = Math.Clamp(cursorY - 1, 0, lines.Count - 1);
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;
                    case ConsoleKey.N when ctrl:
                        Console.WriteLine("Input: One line down");
                        cursorY = Math.Clamp(cursorY + 1, 0, lines.Count - 1);
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;
                    case ConsoleKey.UpArrow when ctrl:
                        Console.WriteLine("Input: To previous block");
                        break;
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("Input: One line up");
                        cursorY = Math.Clamp(cursorY - 1, 0, lines.Count - 1);
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;
                    case ConsoleKey.DownArrow when ctrl:
                        Console.WriteLine("Input: To next block");
                        break;
                    case ConsoleKey.DownArrow:
                        Console.WriteLine("Input: One line down");
                        cursorY = Math.Clamp(cursorY+1, 0, lines.Count - 1);
                        cursorX = Math.Min(cursorX, lines[cursorY].Length);
                        break;
                    case ConsoleKey.Home when alt:
                        Console.WriteLine("Input: To first row in viewport");
                        cursorX = viewportX;
                        cursorY = viewportY;
                        break;
                    case ConsoleKey.End when alt:
                        Console.WriteLine("Input: To last row in viewport");
                        cursorX = viewportX + consoleWidth - 1;
                        cursorY = viewportY + consoleHeight - 1;
                        break;
                    case ConsoleKey.Y when ctrl:
                        Console.WriteLine("Input: One page up");
                        cursorY = Math.Max(cursorY - consoleHeight, 0);
                        viewportY = Math.Max(viewportY - consoleHeight, 0);

                        break;
                    case ConsoleKey.V when ctrl:
                        Console.WriteLine("Input: One page down");
                        cursorY = Math.Min(cursorY + consoleHeight, lines.Count);
                        viewportY = Math.Min(viewportY + consoleHeight, lines.Count - consoleHeight);
                        break;
                    //Special Movement
                    //Information
                    //Various
                    case ConsoleKey.Backspace:
                        Console.WriteLine("Delete character");
                        if (cursorX > 0) {
                            lines[cursorY] = lines[cursorY].Remove(cursorX - 1, 1);
                            cursorX--;
                        } else {
                            if (cursorY > 0) {
                                cursorY--;
                                cursorX = lines[cursorY].Length;

                                lines[cursorY] += lines[cursorY + 1];
                                lines.RemoveAt(cursorY + 1);
                            }
                        }
                        cursorY = Math.Clamp(cursorY, 0, lines.Count - 1);
                        cursorX = Math.Clamp(cursorX, 0, lines[cursorY].Length);
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("Add a line break");
                        string currentLine = lines[cursorY];
                        string before = currentLine.Substring(0, cursorX);
                        string after = currentLine.Substring(cursorX);
                        lines[cursorY] = before;
                        lines.Insert(cursorY + 1, after);
                        cursorY++;
                        cursorX = 0;
                        break;
                    default:
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0') { 
                            lines[cursorY] = lines[cursorY].Insert(cursorX, key.KeyChar.ToString());
                            cursorX++;
                            Console.WriteLine(key.ToString());
                        }
                        break;
                }

                PrintViewport(lines, ref viewportX, ref viewportY, ref cursorX, ref cursorY);
            }
        }
    }
}
