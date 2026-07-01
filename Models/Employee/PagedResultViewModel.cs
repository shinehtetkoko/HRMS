namespace HRMS.Models.Employee
{
    public class PagedResultViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public string SelectedMode { get; set; } = "Pending";
        public int? SelectedMonth { get; set; }
        public int? SelectedYear { get; set; }
    }
}