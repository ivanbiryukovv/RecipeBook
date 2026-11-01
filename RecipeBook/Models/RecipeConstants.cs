namespace RecipeBook.Models;

public static class RecipeConstants
{
    public const int MinCookingTimeMinutes = 1;
    public const int MaxCookingTimeMinutes = 1440;

    public const int MinServingsCount = 1;
    public const int MaxServingsCount = 100;

    public const int MinDifficultyLevel = 1;
    public const int MaxDifficultyLevel = 5;

    public const int MinTitleLength = 2;
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 2000;

    public const string DatabaseFileName = "RecipeBookData.db";
    public const string RecipeCollectionName = "recipes";

    public const string CategoryAll = "Все категории";
    public const string CategorySoups = "Супы";
    public const string CategorySalads = "Салаты";
    public const string CategoryMainDishes = "Основные блюда";
    public const string CategoryBaking = "Выпечка";
    public const string CategoryDesserts = "Десерты";
    public const string CategoryBeverages = "Напитки";

    public static readonly IReadOnlyList<string> AllCategories =
    [
        CategorySoups,
        CategorySalads,
        CategoryMainDishes,
        CategoryBaking,
        CategoryDesserts,
        CategoryBeverages
    ];
}
