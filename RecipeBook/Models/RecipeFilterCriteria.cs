namespace RecipeBook.Models;

public class RecipeFilterCriteria
{
    public string? SearchText { get; set; }
    public string? Category { get; set; }
    public int? MinCookingTime { get; set; }
    public int? MaxCookingTime { get; set; }
    public List<int> Difficulties { get; set; } = [];
    public bool OnlyFavorites { get; set; }
    public int? Servings { get; set; }

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(SearchText) &&
        (string.IsNullOrWhiteSpace(Category) || Category == RecipeConstants.CategoryAll) &&
        !MinCookingTime.HasValue &&
        !MaxCookingTime.HasValue &&
        Difficulties.Count == 0 &&
        !OnlyFavorites &&
        !Servings.HasValue;

    public void Reset()
    {
        SearchText = null;
        Category = RecipeConstants.CategoryAll;
        MinCookingTime = null;
        MaxCookingTime = null;
        Difficulties.Clear();
        OnlyFavorites = false;
        Servings = null;
    }

    public RecipeFilterCriteria Clone()
    {
        return new RecipeFilterCriteria
        {
            SearchText = SearchText,
            Category = Category,
            MinCookingTime = MinCookingTime,
            MaxCookingTime = MaxCookingTime,
            Difficulties = [.. Difficulties],
            OnlyFavorites = OnlyFavorites,
            Servings = Servings
        };
    }
}
