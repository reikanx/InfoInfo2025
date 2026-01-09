using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoInfo2025.Models
{
    public class Text
    {
        public Text()
        {
            Title = string.Empty;
            Summary = string.Empty;
            Content = string.Empty;
            Keywords = string.Empty;
            Graphic = string.Empty;
            AddedDate = DateTime.Now;
            Active = true;
        }

        public int TextId { get; set; }


        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        [Display(Name = "Tytuł tekstu:")]
        [StringLength(75, ErrorMessage = "Tytuł tekstu nie może przekraczać 75 znaków.")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Streszczenie jest wymagane.")]
        [Display(Name = "Streszczenie tekstu:")]
        [StringLength(255, ErrorMessage = "Streszczenie nie może przekraczać 255 znaków.")]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }


        [Display(Name = "Słowa kluczowe:")]
        [MaxLength(255, ErrorMessage = "Słowa kluczowe nie mogą być dłuższe niż 255 znaków.")]
        public string Keywords { get; set; }


        [Required(ErrorMessage = "Treść jest wymagana.")]
        [Display(Name = "Treść tekstu:")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }


        [Display(Name = "Grafika do tekstu:")]
        [MaxLength(128)]
        public string? Graphic { get; set; }


        [Required]
        [Display(Name = "Czy wyświetlać?")]
        public bool Active { get; set; }


        [Display(Name = "Data dodania:")]
        public DateTime AddedDate { get; set; }


        [Required]
        [Display(Name = "Kategoria tekstu:")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [Display(Name = "Kategoria tesktu:")]
        public Category? Category { get; set; }


        [Display(Name = "Autor tekstu:")]
        public string? UserId { get; set; }


        [ForeignKey("UserId")]
        [Display(Name = "Autor tekstu:")]
        public AppUser ?Author { get; set; }


        public ICollection<Opinion> Opinions { get; set; } = new List<Opinion>();
    }

}
