using Cosmos.HAL;

namespace OSaft.applications
{
    public static class Clock
    {
        public static string ShowTime()
        {
            int h = RTC.Hour;
            int m = RTC.Minute;
            int s = RTC.Second;
            var now = $"{h:D2}:{m:D2}:{s:D2}";
            return now;
            //Cosmos zieht die Uhrzeit durch RTC vom Hostsystem, Sommer/-Winterzeit muss dabei nicht beachtet werden.
            // wenn nicht virtualisert wird, dann wird die Hardwareuhr genutzt. HW Uhr 1:00Uhr UTC,
            // Hostsystem 2:00Uhr MEZ 3:00 MESZ, Cosmos übernimmt Uhrzeit
        }
        public static string GetShortTime()
        {
            if (IsActive == false)
            {
                return null;
            }
            int h = RTC.Hour;
            int m = RTC.Minute;
            var now = $"{h:D2}:{m:D2}";
            now += " - ";
            return now;
        }
        public static bool IsActive = true;
        public static void ToggleClock()
        {
            IsActive = !IsActive;
        }
    }
}



