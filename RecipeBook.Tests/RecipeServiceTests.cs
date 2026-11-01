using System.IO;
using RecipeBook.Data;
using RecipeBook.Models;
using RecipeBook.Services;
using Xunit;

namespace RecipeBook.Tests;

public class RecipeServiceTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly LiteDbRecipeRepository _repository;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _testDbPath = $"test_{Guid.NewGuid():N}.db";
        _repository = new LiteDbRecipeRepository(_testDbPath);
        _service = new RecipeService(_repository);
    }

    [Fact]
    public void ValidateRecipe_WithValidData_ReturnsSuccess()
    {
        var recipe = new Recipe
        {
            Title = "Блины классические",
            Category = RecipeConstants.CategoryBaking,
            CookingTimeMinutes = 35,
            Servings = 4,
            Difficulty = 2
        };

        var result = _service.ValidateRecipe(recipe);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ValidateRecipe_EmptyOrWhitespaceTitle_ReturnsError(string? invalidTitle)
    {
        var recipe = new Recipe
        {
            Title = invalidTitle!,
            Category = RecipeConstants.CategorySoups,
            CookingTimeMinutes = 40,
            Servings = 2,
            Difficulty = 1
        };

        var result = _service.ValidateRecipe(recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, err => err.Contains("Название блюда"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void ValidateRecipe_NonPositiveCookingTime_ReturnsError(int invalidTime)
    {
        var recipe = new Recipe
        {
            Title = "Паста Карбонара со сливками",
            Category = RecipeConstants.CategoryMainDishes,
            CookingTimeMinutes = invalidTime,
            Servings = 4,
            Difficulty = 2
        };

        var result = _service.ValidateRecipe(recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, err => err.Contains("больше 0 минут"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateRecipe_NonPositiveServings_ReturnsError(int invalidServings)
    {
        var recipe = new Recipe
        {
            Title = "Овощной салат",
            Category = RecipeConstants.CategorySalads,
            CookingTimeMinutes = 15,
            Servings = invalidServings,
            Difficulty = 1
        };

        var result = _service.ValidateRecipe(recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, err => err.Contains("порций"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void ValidateRecipe_DifficultyOutOfRange_ReturnsError(int invalidDifficulty)
    {
        var recipe = new Recipe
        {
            Title = "Том Ям с морепродуктами",
            Category = RecipeConstants.CategorySoups,
            CookingTimeMinutes = 45,
            Servings = 2,
            Difficulty = invalidDifficulty
        };

        var result = _service.ValidateRecipe(recipe);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, err => err.Contains("сложности"));
    }

    [Fact]
    public void CreateRecipe_ValidRecipe_PersistsToLiteDbAndAssignsId()
    {
        var recipe = new Recipe
        {
            Title = "Греческий салат с сыром фета",
            Category = RecipeConstants.CategorySalads,
            Description = "Свежие овощи с оливковым маслом и сыром.",
            CookingTimeMinutes = 15,
            Servings = 2,
            Difficulty = 1,
            Ingredients =
            [
                new("Помидоры", 3, "шт."),
                new("Огурцы", 2, "шт."),
                new("Сыр Фета", 150, "г.")
            ]
        };

        var (isSuccess, validation, newId) = _service.CreateRecipe(recipe);

        Assert.True(isSuccess);
        Assert.True(validation.IsValid);
        Assert.True(newId > 0);

        var retrieved = _service.GetRecipeDetails(newId);
        Assert.NotNull(retrieved);
        Assert.Equal("Греческий салат с сыром фета", retrieved.Title);
        Assert.Equal(3, retrieved.Ingredients.Count);
    }

    [Fact]
    public void SearchRecipes_ByPartialName_FindsMatchingRecords()
    {
        var results = _service.GetRecipes("Борщ");

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Contains("Борщ", r.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SearchRecipes_WithCriteria_FiltersByTimeAndDifficulty()
    {
        var criteria = new RecipeFilterCriteria
        {
            MinCookingTime = 20,
            MaxCookingTime = 30,
            Difficulties = [2, 4]
        };

        var results = _service.GetRecipes(criteria);

        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.True(r.CookingTimeMinutes >= 20 && r.CookingTimeMinutes <= 30);
            Assert.True(r.Difficulty == 2 || r.Difficulty == 4);
        });
    }

    [Fact]
    public void SearchRecipes_WithCriteria_FiltersOnlyFavorites()
    {
        var criteria = new RecipeFilterCriteria
        {
            OnlyFavorites = true
        };

        var results = _service.GetRecipes(criteria);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.True(r.IsFavorite));
    }

    [Fact]
    public void ToggleFavorite_InvertsFavoriteStatus()
    {
        var recipe = _service.GetRecipeDetails(101);
        Assert.NotNull(recipe);
        bool initialStatus = recipe.IsFavorite;

        var toggled = _service.ToggleFavorite(101);

        Assert.True(toggled);
        var updated = _service.GetRecipeDetails(101);
        Assert.NotNull(updated);
        Assert.Equal(!initialStatus, updated.IsFavorite);
    }

    public void Dispose()
    {
        _repository.Dispose();
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
            }
        }
    }
}
