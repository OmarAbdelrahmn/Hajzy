using Microsoft.AspNetCore.Http;

namespace Application.Abstraction.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials =
        new("بيانات تسجيل الدخول غير صحيحة", "البريد الإلكتروني أو كلمة المرور غير صحيحة", StatusCodes.Status401Unauthorized);

    public static readonly Error EmailAlreadyExist =
        new("البريد الإلكتروني مسجل مسبقًا", "يوجد مستخدم مسجل بهذا البريد الإلكتروني بالفعل", StatusCodes.Status409Conflict);

    public static readonly Error DuplicatedConfermation =
        new("البريد الإلكتروني مؤكد", "تم تأكيد البريد الإلكتروني مسبقًا", StatusCodes.Status400BadRequest);

    public static readonly Error EmailNotConfirmed =
        new("البريد الإلكتروني غير مؤكد", "لم يتم تأكيد البريد الإلكتروني بعد", StatusCodes.Status401Unauthorized);

    public static readonly Error UserNotFound =
        new("المستخدم غير موجود", "المستخدم غير موجود", StatusCodes.Status401Unauthorized);

    public static readonly Error Disableuser =
        new("المستخدم معطل", "تم تعطيل هذا المستخدم، يرجى التواصل مع الإدارة", StatusCodes.Status401Unauthorized);

    public static readonly Error userLockedout =
        new("المستخدم مقفل", "تم قفل هذا المستخدم، يرجى التواصل مع الإدارة", StatusCodes.Status401Unauthorized);

    public static readonly Error Unauthorized =
        new("ليس لديك صلاحية", "ليس لديك الصلاحية المطلوبة", StatusCodes.Status401Unauthorized);
}