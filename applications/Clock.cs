using System;
using System.IO;
using Cosmos.HAL;

namespace OSaft.applications
{
    public static class Clock
    {
        public static void ShowTime()
        {
            var now = RTC.Hour + ":" + RTC.Minute + ":" + RTC.Second;
            Console.WriteLine($"Current Time: {now}");
            //Cosmos zieht die Uhrzeit durch RTC vom Hostsystem, Sommer/-Winterzeit muss dabei nicht beachtet werden.
            // wenn nicht virtualisert wird, dann wird die Hardwareuhr genutzt. HW Uhr 1:00Uhr UTC,
            // Hostsystem 2:00Uhr MEZ 3:00 MESZ, Cosmos Strg+c, strg+v
        }
    }
}