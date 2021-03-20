using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EconomyUtils
{
    private static readonly int charA = Convert.ToInt32('A');
    private static readonly Dictionary<int, string> units = new Dictionary<int, string>
    {
        {0, ""},
        {1, "K"},
        {2, "M"},
        {3, "B"},
        {4, "T"}
    };

    public static string FormatNumber(double value, bool onlyAfterMillion)
    {
        if (value < 1d)
            return "0";

        double flooredValue = Math.Floor(value);

        if (flooredValue < 1000 || (flooredValue < 1000000 && onlyAfterMillion))
            return flooredValue.ToString("N0");

        var n = (int)Math.Log(flooredValue, 1000);
        var m = flooredValue / Math.Pow(1000, n);

        string unit;
        if (n < 0)
        {
            return "Infinity";
        }
        else if (n < units.Count)
        {
            unit = units[n];
        }
        else
        {
            var unitInt = n - units.Count;
            var secondUnit = unitInt % 26;
            var firstUnit = unitInt / 26;
            unit = " " + Convert.ToChar(firstUnit + charA).ToString() + Convert.ToChar(secondUnit + charA).ToString();
        }

        return (Math.Floor(m * 100) / 100).ToString("0.00") + unit;
    }

    public static string FormatSeconds(double seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        if (seconds < 10)
            return t.Seconds + "." + (t.Milliseconds / 100) + "s";

        t = TimeSpan.FromSeconds(Math.Ceiling(seconds));

        string result = "";
        if (t.Hours > 0)
        {
            result += t.Hours + "h ";
            result += t.Minutes + "m ";
        }
        else
        {
            if (t.Minutes > 0)
                result += t.Minutes + "m ";
            result += t.Seconds + "s";
        }

        return result;
    }

    public static double GetUpgradeCost(double baseCost, float coefficient, double currentLevel, int amountWanted)
    {
        int n = amountWanted;
        double b = baseCost;
        double r = coefficient;
        double k = currentLevel;

        double cost = b * (Math.Pow(r, k) * (Math.Pow(r, n) - 1) / (r - 1));
        return Math.Floor(cost);
    }

    public static double GetMaximumPurchasable(double baseCost, float coefficient, double currentLevel, double currency)
    {
        double b = baseCost;
        double r = coefficient;
        double k = currentLevel;
        double c = currency;

        return Math.Floor(Math.Log(c * (r - 1) / (b * (Math.Pow(r, k)) + 1), r));
    }
}
