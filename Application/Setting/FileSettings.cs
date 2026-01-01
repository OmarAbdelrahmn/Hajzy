namespace Application.Setting;

public static class FileSettings
{
    public const int MaxFileSizeInMB = 3;
    public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
    public static readonly string[] BlockedSigntures = ["4D-5A", "2F-2A", "D0-CF"];
    public static readonly string[] AllowedImagesExtensions = [".jpg", ".jpeg", ".png"];
}
