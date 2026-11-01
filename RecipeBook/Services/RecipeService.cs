using RecipeBook.Data;
using RecipeBook.Models;

namespace RecipeBook.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _repository;

    public RecipeService(IRecipeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ValidationResult ValidateRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var result = new ValidationResult();
        ValidateTitle(recipe.Title, result);
        ValidateCategory(recipe.Category, result);
        ValidateCookingTime(recipe.CookingTimeMinutes, result);
        ValidateServings(recipe.Servings, result);
        ValidateDifficulty(recipe.Difficulty, result);

        return result;
    }

    public (bool IsSuccess, ValidationResult Validation, int NewRecipeId) CreateRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var validation = ValidateRecipe(recipe);
        if (!validation.IsValid)
        {
            return (false, validation, 0);
        }

        var newId = _repository.AddRecipe(recipe);
        return (newId > 0, validation, newId);
    }

    public (bool IsSuccess, ValidationResult Validation) UpdateRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        var validation = ValidateRecipe(recipe);
        if (!validation.IsValid)
        {
            return (false, validation);
        }

        var isUpdated = _repository.UpdateRecipe(recipe);
        return (isUpdated, validation);
    }

    public bool DeleteRecipe(int recipeId)
    {
        if (recipeId <= 0) return false;
        return _repository.DeleteRecipe(recipeId);
    }

    public bool ToggleFavorite(int recipeId)
    {
        if (recipeId <= 0) return false;

        var recipe = _repository.GetRecipeById(recipeId);
        if (recipe == null) return false;

        recipe.IsFavorite = !recipe.IsFavorite;
        return _repository.UpdateRecipe(recipe);
    }

    public IReadOnlyList<Recipe> GetRecipes(string? searchTitle, string? category = null, bool onlyFavorites = false)
    {
        return _repository.SearchRecipes(searchTitle, category, onlyFavorites);
    }

    public IReadOnlyList<Recipe> GetRecipes(RecipeFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        return _repository.SearchRecipes(criteria);
    }

    public Recipe? GetRecipeDetails(int recipeId)
    {
        if (recipeId <= 0) return null;
        return _repository.GetRecipeById(recipeId);
    }

    public IReadOnlyDictionary<string, int> GetStatistics()
    {
        var stats = new Dictionary<string, int>
        {
            ["Total"] = _repository.GetTotalCount(),
            ["Favorites"] = _repository.GetFavoritesCount()
        };

        foreach (var category in RecipeConstants.AllCategories)
        {
            stats[category] = _repository.GetCountByCategory(category);
        }

        return stats;
    }

    private static void ValidateTitle(string? title, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            result.AddError("Поле «Название блюда» обязательно для заполнения.");
            return;
        }

        var trimmed = title.Trim();
        if (trimmed.Length < RecipeConstants.MinTitleLength)
        {
            result.AddError($"Название блюда должно содержать не менее {RecipeConstants.MinTitleLength} символов.");
        }
        else if (trimmed.Length > RecipeConstants.MaxTitleLength)
        {
            result.AddError($"Название блюда не должно превышать {RecipeConstants.MaxTitleLength} символов.");
        }
    }

    private static void ValidateCategory(string? category, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            result.AddError("Категория блюда должна быть указана.");
            return;
        }

        if (!RecipeConstants.AllCategories.Contains(category))
        {
            result.AddError($"Выбрана неизвестная категория: «{category}».");
        }
    }

    private static void ValidateCookingTime(int timeMinutes, ValidationResult result)
    {
        if (timeMinutes < RecipeConstants.MinCookingTimeMinutes)
        {
            result.AddError("Время приготовления должно быть больше 0 минут.");
        }
        else if (timeMinutes > RecipeConstants.MaxCookingTimeMinutes)
        {
            result.AddError($"Время приготовления не может превышать {RecipeConstants.MaxCookingTimeMinutes} минут (24 часа).");
        }
    }

    private static void ValidateServings(int servings, ValidationResult result)
    {
        if (servings < RecipeConstants.MinServingsCount)
        {
            result.AddError("Количество порций должно быть не менее 1 персоны.");
        }
        else if (servings > RecipeConstants.MaxServingsCount)
        {
            result.AddError($"Количество порций не должно превышать {RecipeConstants.MaxServingsCount}.");
        }
    }

    private static void ValidateDifficulty(int difficulty, ValidationResult result)
    {
        if (difficulty < RecipeConstants.MinDifficultyLevel || difficulty > RecipeConstants.MaxDifficultyLevel)
        {
            result.AddError($"Уровень сложности должен находиться в интервале от {RecipeConstants.MinDifficultyLevel} до {RecipeConstants.MaxDifficultyLevel}.");
        }
    }
}
