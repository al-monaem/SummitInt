namespace Utility
{
    public class Filter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public static class Utility
    {
        public static List<string> UnitTypes { get; set; } = new List<string>()
        {
            "Pcs", "Kgs", "lbs", "Usd",
        };

    }

    public static class FilterKeys
    {
        public const string FilterByItemName = "filterByItemName";
        public const string FilterByItemCategory = "filterByItemCategory";
        public const string FilterByItemUnit = "filterByItemUnit";
        public const string FitlerByItemQuatity = "filterByItemQuatity";
    }
}
