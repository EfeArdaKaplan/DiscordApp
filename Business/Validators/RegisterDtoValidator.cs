using Business.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz")
                .Length(3, 20).WithMessage("Kullanıcı adı 3 ile 20 karakter arasında olmalıdır");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi boş olamaz")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz!")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir!")
                .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir!")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam (sayı) içermelidir!");
        }
    }
}
