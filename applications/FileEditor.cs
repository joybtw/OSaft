using OSaft.filesystem;
using System;
using System.Collections.Generic;

namespace OSaft.applications
{
    internal class FileEditor
    {
        struct Pos
        {
            public int cursorX;
            public int cursorY;
            public int viewportX;
            public int viewportY;
        }
        struct StackState 
        { 
            public List<string> lines;
            public Pos pos;
        }

        // Constant
        private static readonly int[] console = { 0, Console.WindowWidth - 1, 1, Console.WindowHeight-3 }; // minX, maxX, minY, maxY -> Area of the Console that is for printing
        private static readonly int consoleWidth = console[1] - console[0] + 1;
        private static readonly int consoleHeight = console[3] - console[2] + 1;
        
        // Variable
        private static Pos pos = new();
        private static List<string> lines;
        private static List<StackState> undoStack = new();
        private static List<StackState> redoStack = new();

        private static void CursorToConsole(Pos pos)
        {
            Console.SetCursorPosition(pos.cursorX - pos.viewportX + console[0], pos.cursorY - pos.viewportX + console[2]);
        }
        
        private static void ClearViewport()
        {
            for (int Y = console[2]; Y <= console[3]; Y++)
            {
                Console.SetCursorPosition(console[0], Y);
                Console.Write(new string(' ', consoleWidth));
            }
        }   

        private static void PrintControls() 
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
            Console.Write(" ^S: Save | ^K: Cut line | alt+U: Undo | ^A: Start of line");
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(" ^X: Exit | ^U: Paste    | alt+E: Redo | ^E: End of line");
        }

        private static void PrintViewport() {
            ClearViewport();

            // Move viewport to the marker
            pos.viewportX = Math.Min(Math.Max(pos.cursorX - consoleWidth + 1, 0), pos.cursorX);
            pos.viewportY = Math.Min(Math.Max(pos.cursorY - consoleHeight + 1, 0), pos.cursorY);

            // Print only characters in the viewport
            for (int Y = pos.viewportY; Y < Math.Min(lines.Count, pos.viewportY + consoleHeight); Y++) {
                string line = lines[Y];
                int start = Math.Min(pos.viewportX, line.Length);
                int length = Math.Min(consoleWidth, line.Length - start);

                Console.SetCursorPosition(console[0], console[2]+Y);

                if (length > 0) {
                    Console.Write(line.Substring(start, length));
                } else {
                    Console.Write("");
                }
            }

            CursorToConsole(pos);
        }

        public static void EditFile(string path)
        {
            // Import the File
            string file = FileSystem.ReadFile(path);
            lines = new(file.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            if (lines.Count == 0) {lines.Add("");}

            undoStack.Add(new StackState { lines = new List<string>(lines), pos = pos});
            string buffer = "";

            Console.Clear();
            string title = $"-- Editing: {path} --";
            Console.SetCursorPosition(Math.Max(0, Console.WindowWidth/2 - (title.Length / 2)) , 0);
            Console.WriteLine(title);
            PrintControls();
            PrintViewport();
            CursorToConsole(pos);

            while (true)
            {
                var key = Console.ReadKey(true);
                bool ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
                // bool shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
                bool alt = (key.Modifiers & ConsoleModifiers.Alt) != 0;

                switch (key.Key)
                {
                    //File Handling
                    case ConsoleKey.S when ctrl:
                        //Input: Save current file
                        file = "";
                        for (int i = 0; i < lines.Count; i++) {
                            file += lines[i];
                            if (i < lines.Count - 1) {
                                file += "\n";
                            }
                        }
                        FileSystem.WriteFile(path, file);
                        break;
                    case ConsoleKey.X when ctrl:
                        // Close buffer, exit from nano
                        Console.Clear();
                        return;
                    //Editing
                    case ConsoleKey.K when ctrl:
                        // Cut current line into cutbuffer
                        buffer = lines[pos.cursorY];
                        lines.RemoveAt(pos.cursorY);
                        if(pos.cursorY > 1)
                        {
                            pos.cursorY--;
                        } else
                        {
                            pos.cursorY = 0;
                            pos.cursorX = 0;
                            if(lines.Count <= 1)
                            {
                                lines.Add("");
                            }
                        }
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    case ConsoleKey.D6 when alt:
                        // Copy current line into cutbuffer
                        buffer = lines[pos.cursorY];
                        break;
                    case ConsoleKey.U when ctrl:
                        // Paste contents of cutbuffer
                        lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, buffer);
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    case ConsoleKey.U when alt:
                        // Undo last action
                        if(undoStack.Count > 0)
                        {
                            lines = undoStack[^1].lines;
                            pos.cursorX = undoStack[^1].pos.cursorX;
                            pos.cursorY = undoStack[^1].pos.cursorY;
                            pos.viewportX = undoStack[^1].pos.viewportX;
                            pos.viewportY = undoStack[^1].pos.viewportY;
                            redoStack.Add(undoStack[^1]);
                            undoStack.RemoveAt(undoStack.Count - 1);
                        }
                        break;
                    case ConsoleKey.E when alt:
                        // Redo last undone action
                        break;
                    //Search and Replace
                    case ConsoleKey.B when ctrl:
                        // Start backward search
                        break;
                    case ConsoleKey.F when ctrl:
                        // Start forward search");
                        break;
                    case ConsoleKey.B when alt:
                        // Find next occurrence backward
                        break;
                    case ConsoleKey.F when alt:
                        // Find next occurrence forward
                        break;
                    case ConsoleKey.R when alt:
                        // Start a replacing session
                        break;
                    //Deletion
                    case ConsoleKey.H when ctrl:
                        // Delete character before cursor
                        if (pos.cursorX > 0)
                        {
                            lines[pos.cursorY].Remove(pos.cursorX - 1, 1);
                            pos.cursorX--;
                        }
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    case ConsoleKey.D when ctrl:
                        // Delete character under cursor
                        if (pos.cursorX > 0)
                        {
                            lines[pos.cursorY].Remove(pos.cursorX,1);
                            pos.cursorX--;
                        }
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    case ConsoleKey.Backspace when alt:
                        // Delete word to the left
                        break;
                    case ConsoleKey.Delete when ctrl:
                        // Delete word to the right
                        break;
                    case ConsoleKey.Delete when alt:
                        // Delete current line
                        lines.RemoveAt(pos.cursorY);
                        if (pos.cursorY > 1)
                        {
                            pos.cursorY--;
                        }
                        else
                        {
                            pos.cursorY = 0;
                            pos.cursorX = 0;
                            if (lines.Count <= 1)
                            {
                                lines.Add("");
                            }
                        }
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    //Operations
                    //Moving around
                    case ConsoleKey.LeftArrow when ctrl:
                        // One word backward
                        while(pos.cursorX > 0 && lines[pos.cursorY][pos.cursorX - 1] != ' ')
                        {
                            pos.cursorX--;
                        }
                        break;
                    case ConsoleKey.RightArrow when ctrl:
                        // One word forward
                        while(pos.cursorX < lines[pos.cursorY].Length && lines[pos.cursorY][pos.cursorX+1] != ' ')
                        {
                            pos.cursorX++;
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        // One character backward
                        if(pos.cursorX > 0)
                        {
                            pos.cursorX--;
                        } else
                        {
                            if(pos.cursorY > 0)
                            {
                                pos.cursorY--;
                                pos.cursorX = lines[pos.cursorY].Length;
                            }
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        // One character forward
                        if (pos.cursorX < lines[pos.cursorY].Length) {
                            pos.cursorX++;
                        } else if (pos.cursorY < lines.Count - 1) {
                            pos.cursorY++;
                            pos.cursorX = 0;
                        }
                        break;
                    case ConsoleKey.A when ctrl:
                        // To start of line
                        pos.cursorX = 0;
                        break;
                    case ConsoleKey.E when ctrl:
                        // To end of line
                        pos.cursorX = lines[pos.cursorY].Length;
                        break;
                    case ConsoleKey.P when ctrl:
                        // One line up
                        pos.cursorY = Math.Clamp(pos.cursorY - 1, 0, lines.Count - 1);
                        pos.cursorX = Math.Min(pos.cursorX, lines[pos.cursorY].Length);
                        break;
                    case ConsoleKey.N when ctrl:
                        // One line down
                        pos.cursorY = Math.Clamp(pos.cursorY + 1, 0, lines.Count - 1);
                        pos.cursorX = Math.Min(pos.cursorX, lines[pos.cursorY].Length);
                        break;
                    case ConsoleKey.UpArrow when ctrl:
                        // To previous block
                        break;
                    case ConsoleKey.UpArrow:
                        // One line up
                        pos.cursorY = Math.Clamp(pos.cursorY - 1, 0, lines.Count - 1);
                        pos.cursorX = Math.Min(pos.cursorX, lines[pos.cursorY].Length);
                        break;
                    case ConsoleKey.DownArrow when ctrl:
                        // To next block
                        break;
                    case ConsoleKey.DownArrow:
                        // One line down
                        pos.cursorY = Math.Clamp(pos.cursorY+1, 0, lines.Count - 1);
                        pos.cursorX = Math.Min(pos.cursorX, lines[pos.cursorY].Length);
                        break;
                    case ConsoleKey.Home when alt:
                        // To first row in viewport
                        pos.cursorX = pos.viewportX;
                        pos.cursorY = pos.viewportY;
                        break;
                    case ConsoleKey.End when alt:
                        // To last row in viewport
                        pos.cursorX = pos.viewportX + consoleWidth - 1;
                        pos.cursorY = pos.viewportY + consoleHeight - 1;
                        break;
                    case ConsoleKey.Y when ctrl:
                        // One page up
                        pos.cursorY = Math.Max(pos.cursorY - consoleHeight, 0);
                        pos.viewportY = Math.Max(pos.viewportY - consoleHeight, 0);

                        break;
                    case ConsoleKey.V when ctrl:
                        // One page down
                        pos.cursorY = Math.Min(pos.cursorY + consoleHeight, lines.Count);
                        pos.viewportY = Math.Min(pos.viewportY + consoleHeight, lines.Count - consoleHeight);
                        break;
                    // Special Movement
                    // Information
                    // Various
                    case ConsoleKey.Backspace:
                        // Delete character
                        if (pos.cursorX > 0) {
                            lines[pos.cursorY] = lines[pos.cursorY].Remove(pos.cursorX - 1, 1);
                            pos.cursorX--;
                        } else {
                            if (pos.cursorY > 0) {
                                pos.cursorY--;
                                pos.cursorX = lines[pos.cursorY].Length;

                                lines[pos.cursorY] += lines[pos.cursorY + 1];
                                lines.RemoveAt(pos.cursorY + 1);
                            }
                        }
                        pos.cursorY = Math.Clamp(pos.cursorY, 0, lines.Count - 1);
                        pos.cursorX = Math.Clamp(pos.cursorX, 0, lines[pos.cursorY].Length);
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    case ConsoleKey.Enter:
                        // Add a line break
                        string currentLine = lines[pos.cursorY];
                        string before = currentLine.Substring(0,pos.cursorX);
                        string after = currentLine.Substring(pos.cursorX);
                        lines[pos.cursorY] = before;
                        pos.cursorY++;
                        lines.Insert(pos.cursorY, after);
                        pos.cursorX = 0;
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                    default:
                        // Insert character
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0') {
                            lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, key.KeyChar.ToString());
                            pos.cursorX++;
                        }
                        undoStack.Add(new StackState { lines = new List<string>(lines),  pos = pos});
                        break;
                }

                PrintViewport();
            }
        }
    }
}
