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

    public const string AdminPhoneNumber = "+38031313213";
    public const string ManagerPhoneNumber = "+38031223213";
    public const string UserPhoneNumber = "+38031314413";

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
    public static readonly Guid ProductShakshukaId = Guid.Parse("4d6e8f0a-2b3c-7d5e-6f7a-8b9c0d1e2f3a");
    public static readonly Guid ProductChallengeId = Guid.Parse("5e7f9a1b-3c4d-8e6f-7a8b-9c0d1e2f3a4b");
    public static readonly Guid ProductMatzoSoupId = Guid.Parse("6f8a0b2c-4d5e-9f7a-8b9c-0d1e2f3a4b5c");
    public static readonly Guid ProductBrisketId = Guid.Parse("7a9b1c3d-5e6f-0a8b-9c0d-1e2f3a4b5c6d");
    public static readonly Guid ProductSchnitzelId = Guid.Parse("8b0c2d4e-6f7a-1b9c-0d1e-2f3a4b5c6d7e");
    public static readonly Guid ProductBabkaId = Guid.Parse("9c1d3e5f-7a8b-2c0d-1e2f-3a4b5c6d7e8f");
    public static readonly Guid ProductHummusId = Guid.Parse("3c5d7e9f-1a2b-6c4d-5e6f-7a8b9c0d1e2f");

    public static readonly Guid ProductHookahBerryId = Guid.Parse("0d2e4f6a-8b9c-3d1e-2f3a-4b5c6d7e8f9a");
    public static readonly Guid ProductHookahMintId = Guid.Parse("1e3f5a7b-9c0d-4e2f-3a4b-5c6d7e8f9a0b");
    public static readonly Guid ProductHookahTropicalId = Guid.Parse("5c1f0d3b-9f5e-4f0a-97c4-91c7621f5812");

    public static readonly Guid ProductLemonadeId = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d");
    public static readonly Guid ProductPomegranatJuiceId = Guid.Parse("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e");
    public static readonly Guid ProductMintTeaId = Guid.Parse("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f");

    public const string ProductKugelName = "Kugel";
    public const string ProductKugelNameUk = "Кугель";

    public const string ProductFalafelSetName = "Falafel Set";
    public const string ProductFalafelSetNameUk = "Набір Фалафель";

    public const string ProductHummusName = "Classic Hummus";
    public const string ProductHummusNameUk = "Класичний Хумус";

    public const string ProductShakshukaName = "Shakshuka";
    public const string ProductShakshukaNameUk = "Шакшука";

    public const string ProductChallengeName = "Challah Bread";
    public const string ProductChallengeNameUk = "Хала";

    public const string ProductMatzoSoupName = "Matzo Ball Soup";
    public const string ProductMatzoSoupNameUk = "Суп з Маца";

    public const string ProductBrisketName = "Beef Brisket";
    public const string ProductBrisketNameUk = "Яловича Грудинка";

    public const string ProductSchnitzelName = "Chicken Schnitzel";
    public const string ProductSchnitzelNameUk = "Курячий Шніцель";

    public const string ProductBabkaName = "Chocolate Babka";
    public const string ProductBabkaNameUk = "Шоколадна Бабка";

    public const string ProductHookahBerryName = "Hookah Wild Berry";
    public const string ProductHookahBerryNameUk = "Кальян Дикі Ягоди";

    public const string ProductHookahMintName = "Hookah Fresh Mint";
    public const string ProductHookahMintNameUk = "Кальян Свіжа М'ята";

    public const string ProductHookahTropicalName = "Hookah Tropical";
    public const string ProductHookahTropicalNameUk = "Кальян Тропічний";

    public const string ProductLemonadeName = "Fresh Lemonade";
    public const string ProductLemonadeNameUk = "Свіжий Лимонад";

    public const string ProductPomegranatJuiceName = "Pomegranate Juice";
    public const string ProductPomegranatJuiceNameUk = "Гранатовий Сік";

    public const string ProductMintTeaName = "Moroccan Mint Tea";
    public const string ProductMintTeaNameUk = "Марокканський М'ятний Чай";

    public const decimal ProductKugelPrice = 120.00m;
    public const decimal ProductFalafelSetPrice = 250.00m;
    public const decimal ProductHummusPrice = 85.00m;
    public const decimal ProductShakshukaPrice = 140.00m;
    public const decimal ProductChallengePrice = 95.00m;
    public const decimal ProductMatzoSoupPrice = 110.00m;
    public const decimal ProductBrisketPrice = 380.00m;
    public const decimal ProductSchnitzelPrice = 220.00m;
    public const decimal ProductBabkaPrice = 150.00m;

    public const decimal ProductHookahBerryPrice = 320.00m;
    public const decimal ProductHookahMintPrice = 300.00m;
    public const decimal ProductHookahTropicalPrice = 350.00m;

    public const decimal ProductLemonadePrice = 45.00m;
    public const decimal ProductPomegranatJuicePrice = 55.00m;
    public const decimal ProductMintTeaPrice = 40.00m;

    public const decimal ProductFalafelSetDiscountPrice = 199.00m;
    public const decimal ProductShakshukaDiscountPrice = 105.00m;
    public const decimal ProductBrisketDiscountPrice = 299.00m;
    public const decimal ProductHookahTropicalDiscountPrice = 280.00m;
    public const decimal ProductBabkaDiscountPrice = 120.00m;
    public const decimal ProductSchnitzelDiscountPrice = 175.00m;

    public static readonly Guid Order1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Order2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Order3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid Review1Id = Guid.Parse("e1111111-1111-1111-1111-111111111111");
    public static readonly Guid Review2Id = Guid.Parse("e2222222-2222-2222-2222-222222222222");
    public static readonly Guid Review3Id = Guid.Parse("e3333333-3333-3333-3333-333333333333");
}