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

    public enum FilterStatus
    {
        Assigned,
        AssignedActive,
        Available,
        Starred,

        // Dead?
        All,
    }

    public struct Query
    {
        public Sort sort = Sort.ByPoints;
        public Direction direction = Direction.Descending;

        public FilterStatus filterStatus = FilterStatus.All;
        public string nameSearch = "";

        public Query() { }

        public void SetSort(int index)
        {
            if (index < 0 || index >= Functions.Count)
            {
                WayfarersWingsPlugin.Instance.SWLogger.LogError("Invalid sort dropdown index: " + index);
                return;
            }

            this.sort = Functions[index].Item1;
        }

        public string GetSortChoice()
        {
            var selectedSort = sort;
            return Functions.Find(fn => fn.Item1 == selectedSort).Item2();
        }

        public void SetFilterStatus(int index)
        {
            if (index < 0 || index >= FilterStatusTuples.Count)
            {
                WayfarersWingsPlugin.Instance.SWLogger.LogError("Invalid filter status index: " + index);
                return;
            }

            this.filterStatus = FilterStatusTuples[index].Item1;
        }

        public string GetFilterStatusChoice()
        {
            var selectedStatus = filterStatus;
            return FilterStatusTuples.Find(fn => fn.Item1 == selectedStatus).Item2();
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
                    .KerbalInfo?.Attributes.GetFullName()
                    .Contains(selectedNameSearch, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (filterStatus != FilterStatus.All)
            {
                var status = filterStatus;
                profiles.RemoveAll(profile =>
                {
                    var profileStatus = profile.GetStatus();
                    return status switch
                    {
                        FilterStatus.Assigned => profileStatus != KerbalStatus.Assigned,
                        FilterStatus.AssignedActive => profileStatus != KerbalStatus.AssignedActive,
                        FilterStatus.Available => profileStatus != KerbalStatus.Available,
                        FilterStatus.Starred => !profile.isStarred,
                        _ => false,
                    };
                });
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
    private static readonly List<(Sort, Func<string>, Func<KerbalProfile, KerbalProfile, int>)> Functions =
    [
        (Sort.ByName, () => LocalizedStrings.SortByName,
            (a, b) => string.Compare(a.KerbalInfo?.Attributes.GetFullName(), b.KerbalInfo?.Attributes.GetFullName(),
                StringComparison.OrdinalIgnoreCase)),
        (Sort.ByPoints, () => LocalizedStrings.SortByPoints, (a, b) => a.totalPoints.CompareTo(b.totalPoints)),
    ];

    public static List<string> SortOptions => Functions.Select(fn => fn.Item2()).ToList();

    /// <summary>
    /// All the filters status options.
    /// </summary>
    private static readonly List<(FilterStatus, Func<string>)> FilterStatusTuples =
    [
        (FilterStatus.All, () => LocalizedStrings.All),
        (FilterStatus.Assigned, () => LocalizedStrings.OnMission),
        (FilterStatus.AssignedActive, () => LocalizedStrings.ActiveVessel),
        (FilterStatus.Available, () => LocalizedStrings.Available),
        (FilterStatus.Starred, () => LocalizedStrings.Starred),
    ];

    public static List<string> FilterStatusOptions => FilterStatusTuples.Select(fn => fn.Item2()).ToList();
}