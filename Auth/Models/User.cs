using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations; // 👈 Add this

namespace Auth.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Номи пурра лозим аст")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Номи пурра бояд аз 3 то 100 аломат бошад")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Почтаи электронӣ лозим аст")]
        [EmailAddress(ErrorMessage = "Лутфан нишонаи имейли амалкунандаро ворид намоед")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Рамз лозим аст")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Рамз бояд на камтар аз 6 аломат бошад")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; } = "User";
    }
}