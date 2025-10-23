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
            public Pos(int cX, int cY, int vX, int vY)
            {
                cursorX = cX;
                cursorY = cY;
                viewportX = vX;
                viewportY = vY;
            }
        }
        struct StackState 
        { 
            public List<string> lines;
            public Pos pos;
            public StackState(List<string> l, Pos p)
            {
                lines = l;
                pos = p;
            }
        }

        // Constant
        private static readonly int[] console = { 0, Console.WindowWidth - 1, 3, Console.WindowHeight-4 }; // minX, maxX, minY, maxY -> Area of the Console that is for printing
        private static readonly int consoleWidth = console[1] - console[0] + 1;
        private static readonly int consoleHeight = console[3] - console[2] + 1;
        private static bool visibleHelp;

        // Variable
        private static Pos pos = new();
        private static List<string> lines;
        private static List<StackState> undoStack = new();
        private static List<StackState> redoStack = new();
        private static List<Pos> markers = new();
        private static string markBuffer = "";
        private static Pos searchPos = new(0,1,0,0);

        private static void CursorToConsole(Pos pos)
        {
            Console.SetCursorPosition(pos.cursorX - pos.viewportX + console[0], pos.cursorY - pos.viewportY + console[2]);
        }
        
        private static void ClearViewport()
        {
            // Console.CursorVisible = false;
            for (int Y = console[2]; Y <= console[3]; Y++)
            {
                Console.SetCursorPosition(console[0], Y);
                Console.Write(new string(' ', consoleWidth));
            }
            // Console.CursorVisible = true;
        }

        private static void SearchInput()
        {
            Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
            Console.Write("Searching for: ");
            bool contuine = true;
            markBuffer = "";

            while (contuine) { 
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        // Remove last character
                        if (markBuffer.Length > 0)
                        {
                            markBuffer = markBuffer.Substring(0, markBuffer.Length - 1);
                        }
                        Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                        Console.Write($"Searching for: {markBuffer}");
                        break;
                    case ConsoleKey.Enter:
                        // Confirm search
                        if(markBuffer == "")
                        {
                            Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                        } else
                        {
                            markers = SearchWord(markBuffer);
                        }
                        contuine = false;
                        break;
                    case ConsoleKey.Escape:
                        markBuffer = "";
                        Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        contuine = false;
                        break;
                    default:
                        // Input character
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0')
                        {
                            markBuffer += key.KeyChar;
                            markers = SearchWord(markBuffer);
                            Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(searchPos.cursorX, searchPos.cursorY);
                            Console.Write($"Searching for: {markBuffer}");
                        }
                        break;
                }
                PrintViewport();
            }
        }
        private static List<Pos> SearchWord(string word)
        {
            if(word == "")
            {
                return null;
            }

            List<Pos> results = new();
            for (int i = 0; i < lines.Count; i++)
            {
                int index = lines[i].IndexOf(word);
                while (index != -1)
                {
                    if (index >= 0 && index < lines[i].Length)
                    {
                        results.Add(new Pos
                        {
                            cursorX = index,
                            cursorY = i,
                            viewportX = 0,
                            viewportY = 0
                        });
                    }
                    index = lines[i].IndexOf(word, index + word.Length);
                }
            }
            return results;
        }
        private static void ToggleHelp()
        {
            if (visibleHelp)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                Console.Write(new string(' ', Console.WindowWidth-1));
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.Write(new string(' ', Console.WindowWidth-1));
                visibleHelp = false;
            } else {
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                Console.Write(" ^S: Save | ^K: Cut line | alt+U: Undo | ^A: Start of line | ^W: Search Input");
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.Write(" ^X: Exit | ^U: Paste    | alt+E: Redo | ^E: End of line   | alt+F: next find");
                visibleHelp = true;
            }
        }

        private static void PrintViewport() {
            ClearViewport();

            // Move viewport to the marker
            if (pos.cursorY < pos.viewportY)
                pos.viewportY = pos.cursorY;
            else if (pos.cursorY >= pos.viewportY + consoleHeight)
                pos.viewportY = pos.cursorY - consoleHeight + 1;

            if (pos.cursorX < pos.viewportX)
                pos.viewportX = pos.cursorX;
            else if (pos.cursorX >= pos.viewportX + consoleWidth)
                pos.viewportX = pos.cursorX - consoleWidth + 1;

            // Print only characters in the viewport
            for (int Y = pos.viewportY; Y < Math.Min(lines.Count, pos.viewportY + consoleHeight); Y++) {
                string line = lines[Y];
                int start = Math.Min(pos.viewportX, line.Length);
                int length = Math.Min(consoleWidth, line.Length - start);

                Console.SetCursorPosition(console[0], console[2]+ (Y - pos.viewportY));

                if (length > 0) {
                    Console.Write(line.Substring(start, length));
                } else {
                    Console.Write("");
                }

                // Mark words of the search results
                if (markers.Count > 0 && !string.IsNullOrEmpty(markBuffer))
                {
                    foreach (Pos mpos in markers)
                    {
                        if (mpos.cursorY == Y)
                        {
                            int wordStart = mpos.cursorX;
                            int wordEnd = wordStart + markBuffer.Length;

                            // Area of Viewports
                            int viewportStart = start;
                            int viewportEnd = start + length;

                            // Test if word is in the viewport
                            if (wordEnd > viewportStart && wordStart < viewportEnd)
                            {
                                // calculate the part of the word in viewport
                                int visibleStart = Math.Max(wordStart, viewportStart);
                                int visibleEnd = Math.Min(wordEnd, viewportEnd);
                                int visibleLength = visibleEnd - visibleStart;

                                // set cursor to word
                                int consoleX = visibleStart - viewportStart + console[0];
                                int consoleY = console[2] + (Y - pos.viewportY);

                                // mark the word
                                Console.SetCursorPosition(consoleX, consoleY);
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.Write(line.Substring(visibleStart, visibleLength));
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }
            CursorToConsole(pos);
        }
        private static void AddUndoStack()
        {
            redoStack.Clear();
            undoStack.Add(new StackState { lines = new List<string>(lines), pos = pos });
        }

        public static void EditFile(string path)
        {
            // Import the File
            string file = FileSystem.ReadFile(path);
            if (file == null) { return; }
            lines = new(file.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
            if (lines.Count == 0) {lines.Add("");}

            // Initialize variables
            undoStack.Clear();
            redoStack.Clear();
            markers.Clear();
            undoStack.Add(new StackState { lines = new List<string>(lines), pos = pos});
            string buffer = "";
            pos = new Pos(0, 0, 0, 0);
            visibleHelp = false;

            Console.ResetColor();
            Console.Clear();
            string title = $"-- Editing: {path} --";
            Console.SetCursorPosition(Math.Max(0, Console.WindowWidth/2 - (title.Length / 2)) , 0);
            Console.WriteLine(title);
            ToggleHelp();
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
                        markBuffer = "";
                        Console.Clear();
                        return;
                    //Editing
                    case ConsoleKey.K when ctrl:
                        AddUndoStack();
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
                        break;
                    case ConsoleKey.D6 when alt:
                        // Copy current line into cutbuffer
                        buffer = lines[pos.cursorY];
                        break;
                    case ConsoleKey.U when ctrl:
                        AddUndoStack();
                        // Paste contents of cutbuffer
                        lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, buffer);
                        break;
                    case ConsoleKey.U when alt:
                        // Undo last action
                        if(undoStack.Count > 0)
                        {
                            lines = undoStack[^1].lines;
                            pos = undoStack[^1].pos;
                            redoStack.Add(undoStack[^1]);
                            undoStack.RemoveAt(undoStack.Count - 1);
                        }
                        break;
                    case ConsoleKey.E when alt:
                        // Redo last undone action
                        if (redoStack.Count > 0)
                        {
                            lines = redoStack[^1].lines;
                            pos = redoStack[^1].pos;
                            undoStack.Add(redoStack[^1]);
                            redoStack.RemoveAt(redoStack.Count - 1);
                        }
                        break;
                    //Search and Replace
                    case ConsoleKey.W when ctrl:
                        // Start search input
                        SearchInput();
                        break;
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
                        AddUndoStack();
                        // Delete character before cursor
                        if (pos.cursorX > 0)
                        {
                            lines[pos.cursorY] = lines[pos.cursorY].Remove(pos.cursorX - 1, 1);
                            pos.cursorX--;
                        }
                        break;
                    case ConsoleKey.D when ctrl:
                        AddUndoStack();
                        // Delete character under cursor
                        if (pos.cursorX > 0)
                        {
                            lines[pos.cursorY] = lines[pos.cursorY].Remove(pos.cursorX,1);
                            pos.cursorX--;
                        }
                        break;
                    case ConsoleKey.Backspace when ctrl:
                        AddUndoStack();
                        // Delete word to the left
                        int startWord = pos.cursorX;
                        int endWord = pos.cursorX;
                        while (pos.cursorX > 0 && lines[pos.cursorY][pos.cursorX - 1] == ' ')
                        {
                            pos.cursorX--;
                            startWord--;
                        }
                        while (pos.cursorX > 0 && lines[pos.cursorY][pos.cursorX - 1] != ' ')
                        {
                            pos.cursorX--;
                            startWord--;
                        }
                        lines[pos.cursorY] = lines[pos.cursorY].Remove(startWord, endWord - startWord);
                        break;
                    case ConsoleKey.Delete when ctrl:
                        AddUndoStack();
                        // Delete word to the right
                        startWord = pos.cursorX;
                        endWord = pos.cursorX;
                        while (endWord < lines[pos.cursorY].Length && lines[pos.cursorY][endWord + 1] == ' ')
                        {
                            endWord++;
                        }
                        while (endWord < lines[pos.cursorY].Length && lines[pos.cursorY][endWord + 1] != ' ')
                        {
                            endWord++;
                        }
                        lines[pos.cursorY] = lines[pos.cursorY].Remove(startWord, endWord - startWord);
                        break;
                    case ConsoleKey.Delete when alt:
                        AddUndoStack();
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
                        break;
                    case ConsoleKey.Delete:
                        AddUndoStack();
                        // Delete character under cursor
                        if (pos.cursorX < lines[pos.cursorY].Length)
                        {
                            lines[pos.cursorY] = lines[pos.cursorY].Remove(pos.cursorX, 1);
                        }
                        break;
                    //Operations
                    //Moving around
                    case ConsoleKey.LeftArrow when ctrl:
                        // One word backward
                        while (pos.cursorX > 0 && lines[pos.cursorY][pos.cursorX - 1] == ' ')
                        {
                            pos.cursorX--;
                        }
                        while (pos.cursorX > 0 && lines[pos.cursorY][pos.cursorX - 1] != ' ')
                        {
                            pos.cursorX--;
                        }
                        break;
                    case ConsoleKey.RightArrow when ctrl:
                        // One word forward
                        while (pos.cursorX < lines[pos.cursorY].Length && lines[pos.cursorY][pos.cursorX + 1] == ' ')
                        {
                            pos.cursorX++;
                        }
                        while (pos.cursorX < lines[pos.cursorY].Length && lines[pos.cursorY][pos.cursorX+1] != ' ')
                        {
                            pos.cursorX++;
                        }
                        if(pos.cursorX < lines[pos.cursorY].Length)
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
                    case ConsoleKey.X when alt:
                        // Toggle help visibility
                        ToggleHelp();
                        break;
                    case ConsoleKey.Backspace:
                        AddUndoStack();
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
                        break;
                    case ConsoleKey.Enter:
                        AddUndoStack();
                        // Add a line break
                        string currentLine = lines[pos.cursorY];
                        string before = currentLine.Substring(0,pos.cursorX);
                        string after = currentLine.Substring(pos.cursorX);
                        lines[pos.cursorY] = before;
                        pos.cursorY++;
                        lines.Insert(pos.cursorY, after);
                        pos.cursorX = 0;
                        break;
                    default:
                        // Insert character
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0') {
                            lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, key.KeyChar.ToString());
                            pos.cursorX++;
                        }
                        break;
                }
                if(markBuffer != "")
                {
                    markers = SearchWord(markBuffer);
                }
                PrintViewport();
            }
        }
    }
}
