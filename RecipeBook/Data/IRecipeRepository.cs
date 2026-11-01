using RecipeBook.Models;

namespace RecipeBook.Data;

public interface IRecipeRepository : IDisposable
{
    IReadOnlyList<Recipe> GetAllRecipes();
    Recipe? GetRecipeById(int id);
    int AddRecipe(Recipe recipe);
    bool UpdateRecipe(Recipe recipe);
    bool DeleteRecipe(int id);
    IReadOnlyList<Recipe> SearchRecipes(string? searchTitle, string? category = null, bool? onlyFavorites = null);
    IReadOnlyList<Recipe> SearchRecipes(RecipeFilterCriteria criteria);
    int GetCountByCategory(string category);
    int GetFavoritesCount();
    int GetTotalCount();
}
