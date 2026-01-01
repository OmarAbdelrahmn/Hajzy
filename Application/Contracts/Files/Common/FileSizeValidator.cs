//namespace Medical_E_Commerce.Contracts.Files.Common;

//public class FileSizeValidator : AbstractValidator<IFormFile>
//{
//    public FileSizeValidator()
//    {
//        RuleFor(c => c)
//            .Must((request, context) => request.Length <= FileSettings.MaxFileSizeInBytes)
//            .WithMessage($"Max file size is {FileSettings.MaxFileSizeInMB} MB")
//            .When(c => c is not null);
//    }
//}
