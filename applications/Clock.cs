using Cosmos.HAL;
using System;

namespace OSaft.applications
{
    public static class Clock
    {
        public static void ShowTime()
        {
            int h = RTC.Hour;
            int m = RTC.Minute;
            int s = RTC.Second;
            var now = $"{h:D2}:{m:D2}:{s:D2}";
            Console.WriteLine($"Current Time: {now}");
            //Cosmos zieht die Uhrzeit durch RTC vom Hostsystem, Sommer/-Winterzeit muss dabei nicht beachtet werden.
            // wenn nicht virtualisert wird, dann wird die Hardwareuhr genutzt. HW Uhr 1:00Uhr UTC,
            // Hostsystem 2:00Uhr MEZ 3:00 MESZ, Cosmos Strg+c, strg+v
        }
        public static void ShortTime()
        {
            int h = RTC.Hour;
            int m = RTC.Minute;
            var now = $"{h:D2}:{m:D2}  ";
            Console.Write(now);
        }
        public static bool ActiveClock = true;
    }
}



