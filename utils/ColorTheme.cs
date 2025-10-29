using System;

namespace OSaft.utils
{
    public static class ColorTheme
    {
        public static ConsoleColor BackgroundColor { get; set; }
        public static ConsoleColor ForegroundColor { get; set; }
        public static void ApplyTheme()
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
        }
        public static void ChangeColor()
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
