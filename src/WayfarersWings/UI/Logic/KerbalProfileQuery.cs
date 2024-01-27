using WayfarersWings.Models.Session;
using WayfarersWings.UI.Localization;

namespace WayfarersWings.UI.Logic;

public static class KerbalProfileQuery
{
    public enum Sort
    {
        ByName,
        ByPoints,
    }

    public enum Direction
    {
        Ascending,
        Descending,
    }

    public struct Query
    {
        public Sort sort = Sort.ByPoints;
        public Direction direction = Direction.Descending;
        public string nameSearch = "";

        public Query() { }

        public void SetSort(int index)
        {
            this.sort = Functions[index].Item1;
        }

        public void SetSort(string localizedName)
        {
            this.sort = Functions.Find(fn => fn.Item2 == localizedName).Item1;
        }

        public string GetSortChoice()
        {
            var selectedSort = sort;
            return Functions.Find(fn => fn.Item1 == selectedSort).Item2;
        }

        /// <summary>
        /// Applies the query to the given list of kerbal profiles.
        /// </summary>
        public void Apply(List<KerbalProfile> profiles)
        {
            var selectedSort = sort;
            var selectedDirection = direction;
            var selectedNameSearch = nameSearch;

            if (!string.IsNullOrEmpty(selectedNameSearch))
            {
                profiles.RemoveAll(profile => !profile
                    .KerbalInfo.Attributes.GetFullName()
                    .Contains(selectedNameSearch, StringComparison.OrdinalIgnoreCase));
            }

            var sorter = Functions.Find(fn => fn.Item1 == selectedSort);

            profiles.Sort((a, b) =>
            {
                var result = sorter.Item3(a, b);
                return selectedDirection == Direction.Ascending ? result : -result;
            });
        }
    }

    /// <summary>
    /// All the functions available for sorting kerbal profiles.
    /// </summary>
    public static List<(Sort, string, Func<KerbalProfile, KerbalProfile, int>)> Functions =
    [
        (Sort.ByName, LocalizedStrings.SortByName,
            (a, b) => string.Compare(a.KerbalInfo.Attributes.GetFullName(), b.KerbalInfo.Attributes.GetFullName(),
                StringComparison.OrdinalIgnoreCase)),
        (Sort.ByPoints, LocalizedStrings.SortByPoints, (a, b) => a.totalPoints.CompareTo(b.totalPoints)),
    ];

    public static List<string> SortOptions => Functions.Select(fn => fn.Item2).ToList();
}