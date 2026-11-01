using RecipeBook.Models;

namespace RecipeBook.Services;

public interface IRecipeService
{
    ValidationResult ValidateRecipe(Recipe recipe);
    (bool IsSuccess, ValidationResult Validation, int NewRecipeId) CreateRecipe(Recipe recipe);
    (bool IsSuccess, ValidationResult Validation) UpdateRecipe(Recipe recipe);
    bool DeleteRecipe(int recipeId);
    bool ToggleFavorite(int recipeId);
    IReadOnlyList<Recipe> GetRecipes(string? searchTitle, string? category = null, bool onlyFavorites = false);
    IReadOnlyList<Recipe> GetRecipes(RecipeFilterCriteria criteria);
    Recipe? GetRecipeDetails(int recipeId);
    IReadOnlyDictionary<string, int> GetStatistics();
}
