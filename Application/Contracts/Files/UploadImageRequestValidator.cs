//using Medical_E_Commerce.Contracts.Files.Common;

//namespace Medical_E_Commerce.Contracts.Files;

//public class UploadImageRequestValidator : AbstractValidator<UpdoadImagessRequest>
//{
//    public UploadImageRequestValidator()
//    {
//        RuleFor(c => c.Image)
//            .SetValidator(new FileSizeValidator())
//            .SetValidator(new FileNameValidator());

//        RuleFor(c => c.Image)
//            .Must((request, context) =>
//            {
//                var extension = Path.GetExtension(request.Image.FileName.ToLower());

//                return FileSettings.AllowedImagesExtensions.Contains(extension);
//            })
//            .WithMessage("file extenion is not allowed ")
//            .When(c => c is not null);
//    }
//}
