namespace KosherClouds.Common.Seed;

public static class SharedSeedData
{
    public const string RoleAdmin = "Admin";
    public const string RoleManager = "Manager";
    public const string RoleUser = "User";

    public static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid ManagerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid UserId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public const string AdminEmail = "admin@kosherclouds.com";
    public const string ManagerEmail = "manager@kosherclouds.com";
    public const string UserEmail = "user@kosherclouds.com";

    public const string AdminPassword = "Admin@1234";
    public const string ManagerPassword = "Manager@1234";
    public const string UserPassword = "User@1234";

    public const string AdminFirstName = "System";
    public const string AdminLastName = "Administrator";

    public const string ManagerFirstName = "John";
    public const string ManagerLastName = "Doe";

    public const string UserFirstName = "Jane";
    public const string UserLastName = "Smith";

    public static readonly Guid ProductKugelId = Guid.Parse("1a3b5c7d-9e1f-4a2b-3c4d-5e6f7a8b9c0d");
    public static readonly Guid ProductFalafelSetId = Guid.Parse("2b4c6d8e-0f2a-5b3c-4d5e-6f7a8b9c0d1e");
    public static readonly Guid ProductHookahTropicalId = Guid.Parse("5c1f0d3b-9f5e-4f0a-97c4-91c7621f5812");
}