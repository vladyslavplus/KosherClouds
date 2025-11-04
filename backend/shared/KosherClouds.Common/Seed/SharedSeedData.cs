namespace KosherClouds.Common.Seed
{
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
    }
}
