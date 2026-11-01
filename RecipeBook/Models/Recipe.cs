namespace RecipeBook.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = RecipeConstants.CategoryMainDishes;
    public string Description { get; set; } = string.Empty;
    public int CookingTimeMinutes { get; set; }
    public int Servings { get; set; } = 4;
    public int Difficulty { get; set; } = 2;
    public string ImagePath { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<RecipeIngredient> Ingredients { get; set; } = [];
    public List<string> Instructions { get; set; } = [];

    public string InstructionsText
    {
        get => string.Join(Environment.NewLine, Instructions);
        set
        {
            Instructions = string.IsNullOrWhiteSpace(value)
                ? []
                : value.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrEmpty(s))
                       .ToList();
        }
    }

    public string DifficultyDescription => Difficulty switch
    {
        1 => "1 из 5 (Очень легко)",
        2 => "2 из 5 (Легко)",
        3 => "3 из 5 (Средне)",
        4 => "4 из 5 (Сложно)",
        5 => "5 из 5 (Мастер-шеф)",
        _ => $"{Difficulty} из 5"
    };

    public string DifficultyShort => $"{Difficulty} / 5";
    public string CookingTimeDisplay => $"{CookingTimeMinutes} мин.";
    public string FavoriteDisplay => IsFavorite ? "Да" : "Нет";
}
