using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoInfo2025.Models
{
    public class Opinion
    {
        public Opinion()
        {
            Comment = string.Empty;
            AddedDate = DateTime.Now;
        }

        public int OpinionId { get; set; }


        [Required(ErrorMessage = "Proszę podać treść komentarza.")]
        [Display(Name = "Treść komentarza:")]
        [DataType(DataType.MultilineText)]
        [MinLength(3, ErrorMessage = "Komentarz musi mieć co najmniej 3 znaki.")]
        [MaxLength(1000, ErrorMessage = "Komentarz nie może przekraczać 1000 znaków.")]
        public string Comment { get; set; }


        [Required]
        [Display(Name = "Data dodania:")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm", ApplyFormatInEditMode = true)]
        public DateTime AddedDate { get; set; }


        [Display(Name = "Ocena tekstu:")]
        [Range(0,5,ErrorMessage = "Proszę wybrać ocenę od 0 do 5.")]
        public Rating? Rating { get; set; }


        [Required]
        [Display(Name = "Komentowany tekst:")]
        public int TextId { get; set; }


        [ForeignKey("TextId")]
        public Text? Text {get; set;}


        [Required]
        [Display(Name = "Autor komentarza:")]
        public string? UserId {get; set;}


        [ForeignKey("UserId")]
        public AppUser? Author { get; set; }

    }

    public enum Rating
    {
        [Display(Name = "Brak")]
        Unrated = 0,
        [Display(Name = "Nieprzydatny")]
        Useless = 1,
        [Display(Name = "Słaby")]
        Poor = 2,
        [Display(Name = "Przeciętny")]
        Average = 3,
        [Display(Name = "Dobry")]
        Good = 4,
        [Display(Name = "Świetny")]
        Excellent = 5
    }
}
