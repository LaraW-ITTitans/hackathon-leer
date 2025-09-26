namespace ITTitans.Hackathon2025.Utils;

public static class StaticData
{
    public const string PredefinedAdminUserName = "admin";
    public const string PredefinedAdminRoleName = "Administrator";

    public static readonly Guid PredefinedAdminUserId = Guid.Parse(
        "F8749bd0-25e6-4269-87b9-78dde52855e4"
    );
    public static readonly Guid PredefinedAdminRoleId = Guid.Parse(
        "fd654a92-a664-482c-b0a1-31354f45f466"
    );

    public const string PredefinedReporterRoleName = "Protokollant";
    public static readonly Guid PredefinedReporterRoleId = Guid.Parse(
        "fce2f2b8-e1f3-4baa-8e1c-b025bab3d174"
    );
}