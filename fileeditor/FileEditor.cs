using System;
using OSaft.filesystem;

namespace OSaft.fileeditor
{
    internal class FileEditor
    {
        public static void EditFile(string path)
        {
            string file = FileSystem.ReadFile(path);
            int row = 0;
            int column = 1;

            while (true)
            {
                var key = Console.ReadKey(true);
                bool ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
                bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                bool alt = (key.Modifiers & ConsoleModifiers.Alt) != 0;

                Console.Clear();
                Console.Write("Input: ");
                switch (key.Key)
                {
                    //File Handling
                    case ConsoleKey.S when ctrl:
                        Console.WriteLine("Save current file");
                        FileSystem.WriteFile(path, file);
                        return;
                    case ConsoleKey.X when ctrl:
                        Console.WriteLine("Close buffer, exit from nano");
                        break;
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
                        if(row < 1) {
                            row = 0;
                        } else {
                            row--;
                        }
                            Console.WriteLine("One character backward");
                        break;
                    case ConsoleKey.RightArrow:
                        row++;
                        Console.WriteLine("One character forward");
                        break;
                    case ConsoleKey.A when ctrl:
                        Console.WriteLine("To start of line");
                        break;
                    case ConsoleKey.E when ctrl:
                        Console.WriteLine("To end of line");
                        break;
                    case ConsoleKey.P when ctrl:
                        if (column < 2) {
                            column = 1;
                        } else {
                            column--;
                        }
                            Console.WriteLine("One line up");
                        break;
                    case ConsoleKey.N when ctrl:
                        Console.WriteLine("One line down");
                        column++;
                        break;
                    case ConsoleKey.UpArrow when ctrl:
                        Console.WriteLine("To previous block");
                        break;
                    case ConsoleKey.DownArrow when ctrl:
                        Console.WriteLine("To next block");
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
                    default:
                        Console.WriteLine(key.KeyChar);
                        break;
                }
                Console.WriteLine(file);
                Console.SetCursorPosition(row,column);
            }
        }
    }
}
