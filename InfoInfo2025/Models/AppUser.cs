using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InfoInfo2025.Models
{
    public class AppUser : IdentityUser
    {
        public AppUser()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Information = string.Empty;
            Photo = string.Empty;
        }


        [Display(Name = "Imię:")]
        [MaxLength(20, ErrorMessage = "Imię nie może być dłuższe niż 20 znaków.")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*$", ErrorMessage = "Imię powinno zaczynać się wielką literą i zawierać tylko litery.")]
        public string FirstName { get; set; }


        [Display(Name ="Nazwisko:")] 
        [MaxLength(50, ErrorMessage = "Nazwisko nie może być dłuższe niż 50 znaków.")]
        [RegularExpression(@"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*(?:[-][A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]*)?$", ErrorMessage = "Nazwisko powinno zaczynać się wielką literą i zawierać tylko litery (dopuszczalny jeden myślnik).")]
        public string LastName { get; set; }


        #region dodatkowe pole nieodwzorowane w bazie
        [NotMapped]
        [Display(Name = "Imię i nazwisko:")]
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return "Nie podano";

                return $"{FirstName} {LastName}".Trim();
            }
        }
        #endregion


        [Display(Name = "Informacja o użytkowniku:")]
        [MaxLength(800, ErrorMessage = "Opis nie może przekraczać 800 znaków.")]
        [DataType(DataType.MultilineText)]
        public string Information { get; set; }



        [Display(Name = "Zdjęcie profilowe:")]
        [MaxLength(128)]
        [FileExtensions(Extensions = ".jpg,.png,.gif", ErrorMessage ="Dozwolone rozszerzenia plików to: .jpg, .png, .gif.")]
        public string Photo { get; set; }
    }
}
