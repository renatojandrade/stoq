namespace Stoq.Catalog.Domain.Constants;

public static class WellKnownIds
{
    public static class Categories
    {
        public static readonly Guid Uncategorized = new("00000000-0000-0000-0001-000000000001");
        public static readonly Guid DairyAndEggs = new("00000000-0000-0000-0001-000000000002");
        public static readonly Guid MeatAndSeafood = new("00000000-0000-0000-0001-000000000003");
        public static readonly Guid FruitsAndVegetables = new("00000000-0000-0000-0001-000000000004");
        public static readonly Guid BakeryAndBread = new("00000000-0000-0000-0001-000000000005");
        public static readonly Guid Beverages = new("00000000-0000-0000-0001-000000000006");
        public static readonly Guid SnacksAndSweets = new("00000000-0000-0000-0001-000000000007");
        public static readonly Guid FrozenFoods = new("00000000-0000-0000-0001-000000000008");
        public static readonly Guid CannedAndPackaged = new("00000000-0000-0000-0001-000000000009");
        public static readonly Guid CleaningAndHousehold = new("00000000-0000-0000-0001-000000000010");
        public static readonly Guid PersonalCare = new("00000000-0000-0000-0001-000000000011");
        public static readonly Guid PetSupplies = new("00000000-0000-0000-0001-000000000012");
        public static readonly Guid CondimentsAndSauces = new("00000000-0000-0000-0001-000000000013");
        public static readonly Guid SpicesAndSeasonings = new("00000000-0000-0000-0001-000000000014");
        public static readonly Guid CerealsAndBreakfast = new("00000000-0000-0000-0001-000000000015");
        public static readonly Guid PastaAndRice = new("00000000-0000-0000-0001-000000000016");
        public static readonly Guid OilAndVinegar = new("00000000-0000-0000-0001-000000000017");
        public static readonly Guid NutsAndDriedFruits = new("00000000-0000-0000-0001-000000000018");
    }

    public static class Units
    {
        public static readonly Guid UnitsId = new("00000000-0000-0000-0002-000000000001");
        public static readonly Guid Grams = new("00000000-0000-0000-0002-000000000002");
        public static readonly Guid Kilograms = new("00000000-0000-0000-0002-000000000003");
        public static readonly Guid Millilitres = new("00000000-0000-0000-0002-000000000004");
        public static readonly Guid Litres = new("00000000-0000-0000-0002-000000000005");
        public static readonly Guid Milligrams = new("00000000-0000-0000-0002-000000000006");
        public static readonly Guid Pieces = new("00000000-0000-0000-0002-000000000007");
        public static readonly Guid Boxes = new("00000000-0000-0000-0002-000000000008");
        public static readonly Guid Packs = new("00000000-0000-0000-0002-000000000009");
        public static readonly Guid Bottles = new("00000000-0000-0000-0002-000000000010");
        public static readonly Guid Cans = new("00000000-0000-0000-0002-000000000011");
        public static readonly Guid Bags = new("00000000-0000-0000-0002-000000000012");
        public static readonly Guid Dozens = new("00000000-0000-0000-0002-000000000013");
    }
}
