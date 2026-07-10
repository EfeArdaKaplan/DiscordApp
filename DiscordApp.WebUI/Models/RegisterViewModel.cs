using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace DiscordApp.WebUI.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Kullanıcı adı boş olamaz")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 20 karakter arasında olmalıdır")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi boş olamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş olamaz!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Orijinal JS validator'daki şifre kurallarının (uzunluk, büyük/küçük harf, rakam) birebir karşılığı.
        // Data Annotations tek bir RegularExpression yerine burada ayrı ayrı, kullanıcıya aynı mesajlarla
        // birden fazla hata döndürebilmek için IValidatableObject kullanıldı.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Password))
            {
                if (Password.Length < 6)
                    yield return new ValidationResult("Şifre en az 6 karakter olmalıdır.", new[] { nameof(Password) });

                if (!Password.Any(char.IsUpper))
                    yield return new ValidationResult("Şifre en az bir büyük harf içermelidir!", new[] { nameof(Password) });

                if (!Password.Any(char.IsLower))
                    yield return new ValidationResult("Şifre en az bir küçük harf içermelidir!", new[] { nameof(Password) });

                if (!Password.Any(char.IsDigit))
                    yield return new ValidationResult("Şifre en az bir rakam (sayı) içermelidir!", new[] { nameof(Password) });
            }
        }
    }
}
