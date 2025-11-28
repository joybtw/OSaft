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
                lines = new List<string>(l);
                pos = p;
            }
        }

        // Constant
        private static readonly int[] console = { 0, Console.WindowWidth - 1, 3, Console.WindowHeight-4 }; // minX, maxX, minY, maxY -> Area of the Console that is for printing
        private static readonly int consoleWidth = console[1] - console[0] + 1;
        private static readonly int consoleHeight = console[3] - console[2] + 1;
        private static bool visibleHelp;
        private static Pos searchInputPos = new(0, 1, 0, 0);

        // Variable
        private static Pos pos = new();
        private static List<string> lines;
        private static List<StackState> undoStack = new();
        private static List<StackState> redoStack = new();
        private static List<Pos> markers = new();
        private static string markBuffer = "";
        private static int searchType = 0; // 0: normal, 1: backward, 2: forward
        private static Pos searchPos = new();

        private static void AddUndoStack()
        {
            undoStack.Add(new StackState(new List<string>(lines), pos));
            redoStack.Clear();
        }
        private static void CursorToConsole(Pos pos)
        {
            Console.SetCursorPosition(pos.cursorX - pos.viewportX + console[0], pos.cursorY - pos.viewportY + console[2]);
        }
        private static void PrintViewport()
        {
            //ClearViewport();
            if (markBuffer == "")
            {
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
            }
            else
            {
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write("Searching for: " + markBuffer);
            }

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
            for (int Y = pos.viewportY; Y < Math.Min(lines.Count, pos.viewportY + consoleHeight); Y++)
            {
                string line = lines[Y];
                int start = Math.Min(pos.viewportX, line.Length);
                int length = Math.Min(consoleWidth, line.Length - start);

                Console.SetCursorPosition(console[0], console[2] + (Y - pos.viewportY));

                if (length > 0)
                {
                    line = line.Substring(start, length);
                    line = line.PadRight(consoleWidth, ' '); // Keine Ahnung warum, aber das macht die Ausgabe ineffizient
                    Console.Write(line);
                }
                else
                {
                    Console.Write(new string(' ', consoleWidth));
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
        private static string SearchBar(string pretext, string input)
        {
            bool contuine = true;
            while (contuine)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        // Confirm search
                        contuine = false;
                        break;
                    case ConsoleKey.Escape:
                        // Cancel input
                        input = "";
                        contuine = false;
                        break;
                    case ConsoleKey.Backspace:
                        // Remove last character
                        if (input.Length > 0)
                        {
                            input = input.Substring(0, input.Length - 1);
                        }
                        break;
                    default:
                        // Input character
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0')
                        {
                            input += key.KeyChar;
                        }
                        break;
                }
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write(pretext);
                Console.Write(input);
            }
            return input;
        }
        private static void SearchInput()
        {
            Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
            Console.Write("Searching for: ");

            markBuffer = SearchBar("Searching for: ", "");

            if (markBuffer == "")
            {
                Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
            } else {
                markers = SearchWord(markBuffer);
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
                        switch (searchType)
                        {
                            case 1: // backward
                                if (i < searchPos.cursorY || (i == searchPos.cursorY && index < searchPos.cursorX))
                                {
                                    results.Add(new Pos
                                    {
                                        cursorX = index,
                                        cursorY = i,
                                        viewportX = 0,
                                        viewportY = 0
                                    });
                                }
                                break;
                            case 2: // forward
                                if (i > searchPos.cursorY || (i == searchPos.cursorY && index > searchPos.cursorX))
                                {
                                    results.Add(new Pos
                                    {
                                        cursorX = index,
                                        cursorY = i,
                                        viewportX = 0,
                                        viewportY = 0
                                    });
                                }
                                break;
                            default: // normal
                                results.Add(new Pos
                                {
                                    cursorX = index,
                                    cursorY = i,
                                    viewportX = 0,
                                    viewportY = 0
                                });
                                break;
                        }
                        index = lines[i].IndexOf(word, index + word.Length);
                    }
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
                Console.Write(" ^S: Save | ^K: Cut line | ^R: Undo | ^A: Start of line | ^W: Search Input");
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.Write(" ^X: Exit | ^U: Paste    | ^T: Redo | ^E: End of line   | alt+F: next find");
                visibleHelp = true;
            }
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
                    case ConsoleKey.R when ctrl:
                        // Undo last action
                        if(undoStack.Count > 0)
                        {
                            var old = undoStack[^1];
                            redoStack.Add(new StackState(new List<string>(lines), pos));

                            lines = new List<string>(old.lines);
                            pos = old.pos;

                            undoStack.RemoveAt(undoStack.Count - 1);
                        }
                        break;
                    case ConsoleKey.T when ctrl:
                        // Redo last undone action
                        if (redoStack.Count > 0)
                        {
                            var old = redoStack[^1];
                            undoStack.Add(new StackState(new List<string>(lines), pos));

                            lines = new List<string>(old.lines);
                            pos = old.pos;

                            redoStack.RemoveAt(redoStack.Count - 1);
                        }
                        break;
//Search and Replace
                    case ConsoleKey.W when ctrl:
                        // Start search input
                        searchType = 0;
                        searchPos = pos;
                        SearchInput();
                        break;
                    case ConsoleKey.B when ctrl:
                        // Start backward search
                        searchType = 1;
                        searchPos = pos;
                        SearchInput();
                        break;
                    case ConsoleKey.F when ctrl:
                        // Start forward search;
                        searchType = 2;
                        searchPos = pos;
                        SearchInput();
                        break;
                    case ConsoleKey.B when alt:
                        // Find previous occurrence
                        if (markers.Count > 0)
                        {
                            // Find index of the marker before the current position
                            int previousIndex = -1;

                            for (int i = 0; i < markers.Count; i++)
                            {
                                var m = markers[i];
                                // if marker is before the current position
                                if (m.cursorY < pos.cursorY || (m.cursorY == pos.cursorY && m.cursorX < pos.cursorX))
                                {
                                    previousIndex = i;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            // If none found, wrap to last
                            if (previousIndex == -1)
                            {
                                previousIndex = markers.Count - 1;
                            }

                            // Move cursor to that marker
                            pos.cursorX = markers[previousIndex].cursorX;
                            pos.cursorY = markers[previousIndex].cursorY;
                        }
                        break;
                    case ConsoleKey.F when alt:
                        // Find next occurrence
                        if (markers.Count > 0)
                        {
                            // Find the first marker that appears *after* the current position
                            int nextIndex = -1;
                            for (int i = 0; i < markers.Count; i++)
                            {
                                var m = markers[i];
                                if (m.cursorY > pos.cursorY || (m.cursorY == pos.cursorY && m.cursorX > pos.cursorX))
                                {
                                    nextIndex = i;
                                    break;
                                }
                            }

                            // If none found, wrap around to the first match
                            if (nextIndex == -1)
                            {
                                nextIndex = 0;
                            }

                            // Move cursor to that marker
                            pos.cursorX = markers[nextIndex].cursorX;
                            pos.cursorY = markers[nextIndex].cursorY;
                        }
                        break;
                    case ConsoleKey.R when alt:
                        // Start a replacing session
                        AddUndoStack();

                        if (markBuffer == "")
                        {
                            searchType = 0;
                            searchPos = pos;
                            SearchInput();
                        }

                        Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                        Console.Write(new string(' ', Console.WindowWidth - 1));
                        Console.SetCursorPosition(searchInputPos.cursorX, searchInputPos.cursorY);
                        Console.Write("Replacing with: ");

                        string replaceBuffer = SearchBar("Replacing with: ", "");
                        if (replaceBuffer != "")
                        {
                            for (int i = 0; i < lines.Count; i++)
                            {
                                lines[i] = lines[i].Replace(markBuffer, replaceBuffer);
                            }
                            markers = SearchWord(markBuffer);
                        }
                        markBuffer = "";
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
// Operations
// Moving around
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
                    case ConsoleKey.Spacebar:
                        AddUndoStack();
                        // Insert space
                        lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, " ");
                        pos.cursorX++;
                        break;
                    default:
                        // Insert character
                        if (!char.IsControl(key.KeyChar) && key.KeyChar != '\0') {
                            lines[pos.cursorY] = lines[pos.cursorY].Insert(pos.cursorX, key.KeyChar.ToString());
                            pos.cursorX++;
                            redoStack.Clear();
                        }
                        break;
                }
                if (markBuffer != "")
                {
                    markers = SearchWord(markBuffer);
                }
                PrintViewport();
            }
        }
    }
}
