using OSaft.filesystem;
using System;
using System.Collections.Generic;

namespace OSaft.fileeditor
{
    internal class FileEditor
    {
        private static void PrintViewport(string[] lines, int viewportX, int viewportY,int cursorX,int cursorY, int MAX_HEIGHT, int MAX_WIDTH)
        {
            Console.SetCursorPosition(0, 1);
            // Print only characeters in the viewport
            viewportX = Math.Max(0, viewportX);
            viewportY = Math.Max(0, Math.Min(viewportY, Math.Max(0, lines.Length - MAX_HEIGHT)));
            for (int row = viewportY; row <= Math.Min(viewportY + MAX_HEIGHT, lines.Length - 1); row++)
            {
                string line = lines[row];
                if (line.Length > viewportX)
                {
                    int length = Math.Min(MAX_WIDTH + 1, line.Length - viewportX);
                    string substring = line.Substring(viewportX, length);
                    Console.WriteLine(substring);
                }
                else
                {
                    Console.WriteLine();
                }
            }
            Console.SetCursorPosition(cursorX - viewportX, cursorY - viewportY + 1);
        }
        public static void EditFile(string path)
        {
            // Constants
            int MAX_WIDTH = Console.WindowWidth - 1;
            int MAX_HEIGHT = Console.WindowHeight - 2;

            // Import the File
            string file = FileSystem.ReadFile(path);
            if (file == null) {return;}
            string[] lines = file.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            // Position
            int cursorX = 0;
            int cursorY = 0;
            int viewportX = 0;
            int viewportY = 0;

            Console.Clear();
            Console.WriteLine("Input: ");
            PrintViewport(lines, viewportX, viewportY, cursorX, cursorY, MAX_HEIGHT, MAX_WIDTH);

            while (true)
            {
                var key = Console.ReadKey(true);
                bool ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
                bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                bool alt = (key.Modifiers & ConsoleModifiers.Alt) != 0;
                string buffer;

                Console.Clear();
                Console.Write("Input: ");
                switch (key.Key)
                {
                    //File Handling
                    case ConsoleKey.S when ctrl:
                        Console.WriteLine("Save current file");
                        file = string.Join("\r\n", lines);
                        FileSystem.WriteFile(path, file);
                        return;
                    case ConsoleKey.X when ctrl:
                        Console.WriteLine("Close buffer, exit from nano");
                        return;
                    //Editing
                    case ConsoleKey.K when ctrl:
                        Console.WriteLine("Cut current line into cutbuffer");
                        break;
                    case ConsoleKey.D6 when alt:
                        Console.WriteLine("Copy current line into cutbuffer");
                        break;
                    case ConsoleKey.U when ctrl:
                        Console.WriteLine("Paste contents of cutbuffer");
                        break;
                    case ConsoleKey.U when alt:
                        Console.WriteLine("Undo last action");
                        break;
                    case ConsoleKey.E when alt:
                        Console.WriteLine("Redo last undone action");
                        break;
                    //Search and Replace
                    case ConsoleKey.B when ctrl:
                        Console.WriteLine("Start backward search");
                        break;
                    case ConsoleKey.F when ctrl:
                        Console.WriteLine("Start forward search");
                        break;
                    case ConsoleKey.B when alt:
                        Console.WriteLine("Find next occurrence backward");
                        break;
                    case ConsoleKey.F when alt:
                        Console.WriteLine("Find next occurrence forward");
                        break;
                    case ConsoleKey.R when alt:
                        Console.WriteLine("Start a replacing session");
                        break;
                    //Deletion
                    case ConsoleKey.H when ctrl:
                        Console.WriteLine("Delete character before cursor");
                        break;
                    case ConsoleKey.D when ctrl:
                        Console.WriteLine("Delete character under cursor");
                        break;
                    case ConsoleKey.Backspace when alt:
                        Console.WriteLine("Delete word to the left");
                        break;
                    case ConsoleKey.Delete when ctrl:
                        Console.WriteLine("Delete word to the right");
                        break;
                    case ConsoleKey.Delete when alt:
                        Console.WriteLine("Delete current line");
                        break;
                    //Operations
                    //Moving around
                    case ConsoleKey.LeftArrow when ctrl:
                        Console.WriteLine("One word backward");
                        if (cursorX < 1)
                        {
                            cursorY--;
                            cursorX = lines[cursorY].Length;
                        }
                        char c = lines[cursorY][cursorX];
                        while (cursorX < lines[cursorY].Length && char.IsWhiteSpace(c))
                        {
                            cursorX--;
                        }
                        break;
                    case ConsoleKey.RightArrow when ctrl:
                        Console.WriteLine("One word forward");
                        break;
                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("One character backward");
                        if (cursorX < 1)
                        {
                            if (cursorY > 0)
                            {
                                cursorY--;
                                cursorX = lines[cursorY].Length;
                            }
                        } else {
                            cursorX--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        Console.WriteLine("One character forward");
                        if (cursorX < lines[cursorY].Length)
                        {
                            cursorX++;
                        } else
                        {
                            if (cursorY < (lines.Length - 1))
                            {
                                cursorY++;
                                cursorX = 0;
                            }
                        }
                        break;
                    case ConsoleKey.A when ctrl:
                        Console.WriteLine("To start of line");
                        cursorX = 0;
                        break;
                    case ConsoleKey.E when ctrl:
                        Console.WriteLine("To end of line");
                        cursorX = lines[cursorY].Length;
                        break;
                    case ConsoleKey.P when ctrl:
                        Console.WriteLine("One line up");
                        if (cursorY > 0) {
                            cursorY--;
                        } else {
                            cursorY = 0;
                        }
                        break;
                    case ConsoleKey.N when ctrl:
                        Console.WriteLine("One line down");
                        if (cursorY < lines.Length - 1) {
                            cursorY++;
                            if (lines[cursorY].Length < cursorX) {
                                cursorX = lines[cursorY].Length;
                            }
                        }
                        break;
                    case ConsoleKey.UpArrow when ctrl:
                        Console.WriteLine("To previous block");
                        break;
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("One line up");
                        if (cursorY > 0)
                        {
                            cursorY--;
                        }
                        else
                        {
                            cursorY = 0;
                        }
                        break;
                    case ConsoleKey.DownArrow when ctrl:
                        Console.WriteLine("To next block");
                        break;
                    case ConsoleKey.DownArrow:
                        Console.WriteLine("One line down");
                        if (cursorY < lines.Length - 1)
                        {
                            cursorY++;
                            if (lines[cursorY].Length < cursorX)
                            {
                                cursorX = lines[cursorY].Length;
                            }
                        }
                        break;
                    case ConsoleKey.Home when alt:
                        Console.WriteLine("To first row in viewport");
                        break;
                    case ConsoleKey.End when alt:
                        Console.WriteLine("To last row in viewport");
                        break;
                    case ConsoleKey.Y when ctrl:
                        Console.WriteLine("One page up");
                        break;
                    case ConsoleKey.V when ctrl:
                        Console.WriteLine("One page down");
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
                                lines[cursorY - 1] = lines[cursorY - 1] + lines[cursorY];
                                List<string> temp = new List<string>(lines);
                                temp.RemoveAt(cursorY);
                                lines = temp.ToArray();

                                cursorY--;
                                cursorX = lines[cursorY].Length;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("Add a line break");
                        string currentLine = lines[cursorY];
                        string before = currentLine.Substring(0, cursorX);
                        string after = currentLine.Substring(cursorX);
                        lines[cursorY] = before;
                        var newLines = new List<string>(lines);
                        newLines.Insert(cursorY + 1, after);
                        lines = newLines.ToArray();
                        cursorY++;
                        cursorX = 0;
                        break;
                    default:
                        lines[cursorY] = lines[cursorY].Insert(cursorX, key.KeyChar.ToString());
                        cursorX++;
                        Console.WriteLine(key.ToString());
                        break;
                }

                //Move viewport to the marker
                if (cursorX < viewportX) {
                viewportX = cursorX;
                }else if (cursorX >= viewportX + MAX_WIDTH) { 
                    viewportX = cursorX - MAX_WIDTH + 1;
                }

                if (cursorY < viewportY) {
                    viewportY = cursorY;
                }else if (cursorY >= viewportY + MAX_HEIGHT) { 
                    viewportY = cursorY - MAX_HEIGHT + 1;
                }

                PrintViewport(lines, viewportX, viewportY, cursorX, cursorY, MAX_HEIGHT, MAX_WIDTH);

                file = string.Join("\r\n", lines);
            }
        }
    }
}
