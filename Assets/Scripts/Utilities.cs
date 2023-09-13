
using System;
using System.Globalization;

public class Utilities
{
    public static string GetNowDate()
    {
        var now = DateTime.Now;
        var dateStr = now.ToString("MM/dd/yyyy hh:mm:ss.F", CultureInfo.InvariantCulture);
        return dateStr;
    }
}
