using FluentValidation;
using PersonelLeaveManagement.Application.DTOs;


namespace PersonelLeaveManagement.Application.Validators;

public  class PersonelValidator: AbstractValidator<PersonelDto>
{
    public PersonelValidator()
    {
        RuleFor(x => x.Ad).NotEmpty().MaximumLength(100);
        RuleFor(x=> x.SoyAd).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TCKimlikNo)
            .NotEmpty().Length(11)
            .Matches("^[0-9]+$").WithMessage("TCKN sadece rakamlardan oluşmalıdır.");
    }
}

