using AuthServer.Core.DTOs;
using FluentValidation;

namespace AuthServer.API.Validations
{
    public class CreateUserDtoValidator :AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator() 
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is wrong");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is reqired");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Username is required");
        }


    }
}
