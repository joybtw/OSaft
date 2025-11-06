using System;
using System.Collections.Generic;
using System.Text;

namespace OSaft.utils
{
    public static class Shell
    {
        // --- History ---
        private static readonly List<string> _history = new();
        private static int _histIndex = -1;

        // --- Rendering state ---
        private static int _lastRenderLen = 0; // prompt + text length from previous render

        /// <summary>
        /// Line editor with history and cursor movement (no backspace control chars used).
        /// Keys: Left/Right, Home/End, Backspace, Delete, Up/Down, Enter.
        /// </summary>
        public static string ReadLineWithHistory(string prompt = "> ")
        {
            Console.Write(prompt);

            var buf = new StringBuilder();
            int cursor = 0; // index in buf (0..buf.Length)

            _histIndex = -1;
            _lastRenderLen = prompt.Length;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        {
                            Console.WriteLine();
                            var line = buf.ToString();

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
                            if (cursor < buf.Length)
                            {
                                buf.Remove(cursor, 1);
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.LeftArrow:
                        {
                            if (cursor > 0)
                            {
                                cursor--;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.RightArrow:
                        {
                            if (cursor < buf.Length)
                            {
                                cursor++;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.Home:
                        {
                            if (cursor != 0)
                            {
                                cursor = 0;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.End:
                        {
                            if (cursor != buf.Length)
                            {
                                cursor = buf.Length;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }

                    case ConsoleKey.UpArrow:
                        {
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
                            // Find Suggestion based on current input
                        }

                    default:
                        {
                            char ch = key.KeyChar;
                            if (!char.IsControl(ch))
                            {
                                buf.Insert(cursor, ch);
                                cursor++;
                                RenderLine(prompt, buf, cursor);
                            }
                            break;
                        }
                }
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
    }



}
