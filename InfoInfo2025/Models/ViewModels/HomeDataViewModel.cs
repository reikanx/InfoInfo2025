namespace InfoInfo2025.Models.ViewModels
{
    public class HomeDataViewModel
    {
        public IEnumerable<Category>? DisplayCategories { get; set; }
        
        public IEnumerable<AppUser>? Authors { get; set; }
    }
}
