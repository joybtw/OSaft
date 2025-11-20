using OSaft.commands;
using OSaft.filesystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OSaft.utils
{
    public static class Shell
    {
        // --- History ---
        private static readonly List<string> _history = new();
        private static readonly List<ConsoleKey> _keyHistory = new();
        private static int _histIndex = -1;

        // --- Rendering state ---
        private static int _lastRenderLen = 0; // prompt + text length from previous render

        // --- Rendering state ---
        private static List<string> _cycleMatches = new();
        private static int _cycleIndex = -1;
        private static int _cycleTokenStart = -1;
        private static string _cyclePrefix = "";

        /// <summary>
        /// Line editor with history and cursor movement (no backspace control chars used).
        /// Keys: Left/Right, Home/End, Backspace, Delete, Up/Down, Enter.
        /// </summary>
        public static string ReadLineWithHistory(string prompt = "> ")
        {
            Console.Write(prompt);

            StringBuilder buf = new();
            int cursor = 0; // index in buf (0..buf.Length)

            _histIndex = -1;
            _lastRenderLen = prompt.Length;
            ResetCycle();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        {
                            ResetCycle();
                            Console.WriteLine();
                            string line = buf.ToString();

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                if (_history.Count == 0 || _history[^1] != line)
                                    _history.Add(line);
                            }

                            _lastRenderLen = 0;
                            return line;
                        }

                    case ConsoleKey.Backspace:
                        {
                            ResetCycle();
                            if (cursor > 0)
                            {
                                buf.Remove(cursor - 1, 1);
                                cursor--;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.Delete:
                        {
                            ResetCycle();
                            if (cursor < buf.Length)
                            {
                                buf.Remove(cursor, 1);
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.LeftArrow:
                        {
                            ResetCycle();
                            if (cursor > 0)
                            {
                                cursor--;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.RightArrow:
                        {
                            ResetCycle();
                            if (cursor < buf.Length)
                            {
                                cursor++;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.Home:
                        {
                            ResetCycle();
                            if (cursor != 0)
                            {
                                cursor = 0;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.End:
                        {
                            ResetCycle();
                            if (cursor != buf.Length)
                            {
                                cursor = buf.Length;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.UpArrow:
                        {
                            ResetCycle();
                            if (_history.Count == 0) break;
                            if (_histIndex == -1) _histIndex = _history.Count;
                            if (_histIndex > 0) _histIndex--;
                            buf.Clear();
                            buf.Append(_history[_histIndex]);
                            cursor = buf.Length;
                            RenderLine(prompt, buf, cursor);
                            break;
                        }

                    case ConsoleKey.DownArrow:
                        {
                            ResetCycle();
                            if (_history.Count == 0) break;

                            if (_histIndex != -1 && _histIndex < _history.Count - 1)
                            {
                                _histIndex++;
                                buf.Clear();
                                buf.Append(_history[_histIndex]);
                            }
                            else
                            {
                                _histIndex = -1;
                                buf.Clear();
                            }
                            cursor = buf.Length;
                            RenderLine(prompt, buf, cursor);
                            break;
                        }

                    case ConsoleKey.Tab:
                        {
                            // Where does the current token start, and what is it now?
                            int tokenStart = LastTokenStart(buf);
                            string currentToken = buf.ToString(tokenStart, buf.Length - tokenStart);

                            // If we are already cycling, stick to the original prefix;
                            // otherwise, start a new cycle session based on the current token.
                            string prefix = (_cycleIndex == -1) ? currentToken : _cyclePrefix;

                            // Determine which suggestions list to use (first token vs later)
                            string[] tokens = buf.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            var all = GetSugggestions(tokens);
                            var matches = FilterByPrefix(all, prefix);

                            if (matches.Count == 0)
                            {
                                ResetCycle();
                                continue;
                            }

                            // Start a new cycle session
                            if (_cycleIndex == -1)
                            {
                                // Save cycle anchor
                                _cycleMatches = matches;
                                _cycleTokenStart = tokenStart;
                                _cyclePrefix = prefix;

                                // Try to extend to LCP first; if no extension, pick first match
                                string lcp = LongestCommonPrefix(matches);
                                if (lcp.Length > prefix.Length)
                                {
                                    // Extend the token to the LCP
                                    string addition = lcp[prefix.Length..];
                                    buf.Insert(tokenStart + prefix.Length, addition);
                                    // We haven't picked a concrete item yet; set index to 0 so next Tab cycles to first match.
                                    _cycleIndex = 0;
                                }
                                else
                                {
                                    _cycleIndex = 0;
                                    buf.Remove(_cycleTokenStart, buf.Length - _cycleTokenStart);
                                    buf.Append(_cycleMatches[_cycleIndex]);
                                }
                            }
                            else // Continue cycling existing matches
                            {
                                _cycleIndex = (_cycleIndex + 1) % _cycleMatches.Count;
                                buf.Remove(_cycleTokenStart, buf.Length - _cycleTokenStart);
                                buf.Append(_cycleMatches[_cycleIndex]);
                            }

                            cursor = buf.Length;
                            RenderLine(prompt, buf, cursor);
                            break;
                        }
                    default:
                        {
                            char ch = key.KeyChar;
                            if (!char.IsControl(ch))
                            {
                                buf.Insert(cursor, ch);
                                cursor++;
                                RenderLine(prompt, buf, cursor);
                                ResetCycle();
                            }
                            break;
                        }
                }
                _keyHistory.Add(key.Key);
            }
        }
        private static void RenderLine(string prompt, StringBuilder buf, int cursor)
        {
            string text = buf.ToString();
            int currentLen = prompt.Length + text.Length;

            Console.Write("\r");
            Console.Write(prompt);
            Console.Write(text);

            int extra = _lastRenderLen - currentLen;
            if (extra > 0)
            {
                Console.Write(new string(' ', extra));
                Console.Write("\r");
                Console.Write(prompt);
                Console.Write(text);
            }
            _lastRenderLen = currentLen;

            if (cursor != text.Length)
            {
                Console.Write("\r");
                Console.Write(prompt);

                if (cursor > 0)
                {
                    Console.Write(text[..cursor]);
                }
            }
        }

        private static List<string> GetSugggestions(string[] tokens)
        {
            List<string> suggestions = new();
            if (tokens.Length <= 1) { suggestions.AddRange(CommandHelper.Commands.Keys.ToList()); }
            else
            {
                suggestions.AddRange(Directory.GetDirectories(FileSystem.CurrentPath + "\\"));
                suggestions.AddRange(Directory.GetFiles(FileSystem.CurrentPath + "\\"));
            }

            return suggestions;
        }

        private static List<string> FilterByPrefix(List<string> items, string prefix)
        {
            List<string> res = new();
            for (int i = 0; i < items.Count; i++)
            {
                string it = items[i];
                if (it.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    res.Add(it);
            }
            return res;
        }


        private static int LastTokenStart(StringBuilder sb)
        {
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (sb[i] == ' ') { return i + 1; }
            }
            return 0;
        }

        private static void PrintSuggestions(List<string> items, int width = 80)
        {
            int max = 0;
            foreach (string s in items)
            {
                if (s.Length > max) { max = s.Length; }
            }

            int colWidth = Math.Max(max + 2, 8);
            int cols = Math.Max(1, width / colWidth);

            int i = 0;
            while (i < items.Count)
            {
                int count = Math.Min(cols, items.Count - i);
                for (int c = 0; c < count; c++)
                {
                    string s = items[i + c];
                    Console.Write(s);
                    for (int k = s.Length; k < colWidth; k++) Console.Write(' ');
                }
                Console.WriteLine();
                i += count;
            }
        }

        private static string LongestCommonPrefix(List<string> items)
        {
            if (items.Count == 0) return "";
            string prefix = items[0];
            for (int i = 1; i < items.Count; i++)
            {
                prefix = LongestCommonPrefix(prefix, items[i]);
                if (prefix.Length == 0) break;
            }
            return prefix;
        }

        private static string LongestCommonPrefix(string a, string b)
        {
            int len = Math.Min(a.Length, b.Length);
            int i = 0;
            for (; i < len; i++)
                if (char.ToLowerInvariant(a[i]) != char.ToLowerInvariant(b[i]))
                    break;
            return a[..i];
        }

        private static void ResetCycle()
        {
            _cycleMatches.Clear();
            _cycleIndex = -1;
            _cycleTokenStart = -1;
            _cyclePrefix = "";
        }
    }
}
