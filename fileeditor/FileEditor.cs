using OSaft.filesystem;
using System;
using System.Collections.Generic;

namespace OSaft.fileeditor
{
    internal class FileEditor
    {
        public static void EditFile(string path)
        {
            string file = FileSystem.ReadFile(path);
            if (file == null) {return;}
            string[] lines = file.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int row = 0;
            int column = 0;
            Console.Clear();
            Console.WriteLine("Input: ");
            Console.WriteLine(file);
            Console.SetCursorPosition(row, column + 1);

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
                        break;
                    case ConsoleKey.RightArrow when ctrl:
                        Console.WriteLine("One word forward");
                        break;
                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("One character backward");
                        if(row < 1)
                        {
                            if(column > 0)
                            {
                                column--;
                                row = lines[column].Length;
                            }
                        } else {
                            row--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        Console.WriteLine("One character forward");
                        if(row < lines[column].Length)
                        {
                            row++;
                        } else
                        {
                            if(column < (lines.Length - 1))
                            {
                                column++;
                                row = 0;
                            }
                        }
                            break;
                    case ConsoleKey.A when ctrl:
                        Console.WriteLine("To start of line");
                        row = 0;
                        break;
                    case ConsoleKey.E when ctrl:
                        Console.WriteLine("To end of line");
                        row = lines[column].Length;
                        break;
                    case ConsoleKey.P when ctrl:
                        Console.WriteLine("One line up");
                        if (column > 0) {
                            column--;
                        } else {
                            column = 0;
                        }
                        break;
                    case ConsoleKey.N when ctrl:
                        Console.WriteLine("One line down");
                        if(column < lines.Length -1 ) {
                            column++;
                            if (lines[column].Length < row) {
                                row = lines[column].Length;
                            }
                        }
                        break;
                    case ConsoleKey.UpArrow when ctrl:
                        Console.WriteLine("To previous block");
                        break;
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("One line up");
                        if (column > 0)
                        {
                            column--;
                        }
                        else
                        {
                            column = 0;
                        }
                        break;
                    case ConsoleKey.DownArrow when ctrl:
                        Console.WriteLine("To next block");
                        break;
                    case ConsoleKey.DownArrow:
                        Console.WriteLine("One line down");
                        if (column < lines.Length - 1)
                        {
                            column++;
                            if (lines[column].Length < row)
                            {
                                row = lines[column].Length;
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
                        if (row > 0) {
                            lines[column] = lines[column].Remove(row - 1, 1);
                            row--;
                        } else {
                            if(column > 0) {
                                lines[column-1] = lines[column-1] + lines[column];
                                List<string> temp = new List<string>(lines);
                                temp.RemoveAt(column);
                                lines = temp.ToArray();
                                
                                column--;
                                row = lines[column].Length;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.WriteLine("Add a line break");
                        string currentLine = lines[column];
                        string before = currentLine.Substring(0, row);
                        string after = currentLine.Substring(row);
                        lines[column] = before;
                        var newLines = new List<string>(lines);
                        newLines.Insert(column + 1, after);
                        lines = newLines.ToArray();
                        column++;
                        row = 0;
                        break;
                    default:
                        lines[column] = lines[column].Insert(row, key.KeyChar.ToString());
                        row++;
                        Console.WriteLine(key.ToString());
                        break;
                }
                file = string.Join("\r\n", lines);
                Console.WriteLine(file);
                Console.SetCursorPosition(row, column + 1);
            }
        }
    }
}
