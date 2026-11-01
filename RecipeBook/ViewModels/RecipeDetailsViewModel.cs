using System.Windows.Input;
using RecipeBook.Models;
using RecipeBook.Services;

namespace RecipeBook.ViewModels;

public class RecipeDetailsViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private Recipe _recipe;

    public event Action? RequestClose;
    public event Action<Recipe>? RequestEdit;
    public event Action<Recipe>? RecipeDeleted;
    public event Action? RecipeChanged;

    public RecipeDetailsViewModel(IRecipeService recipeService, Recipe recipe)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
        _recipe = recipe ?? throw new ArgumentNullException(nameof(recipe));

        BackCommand = new RelayCommand(() => RequestClose?.Invoke());
        EditCommand = new RelayCommand(Edit);
        DeleteCommand = new RelayCommand(Delete);
        ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
    }

    public Recipe Recipe
    {
        get => _recipe;
        private set => SetField(ref _recipe, value);
    }

    public string FavoriteButtonText => _recipe.IsFavorite ? "Удалить из избранного" : "В избранное";

    public ICommand BackCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    private void ToggleFavorite()
    {
        _recipeService.ToggleFavorite(_recipe.Id);
        var updated = _recipeService.GetRecipeDetails(_recipe.Id);
        if (updated != null)
        {
            Recipe = updated;
            OnPropertyChanged(nameof(FavoriteButtonText));
            RecipeChanged?.Invoke();
        }
    }

    private void Edit() => RequestEdit?.Invoke(_recipe);

    private void Delete() => RecipeDeleted?.Invoke(_recipe);

    public void RefreshRecipe()
    {
        var updated = _recipeService.GetRecipeDetails(_recipe.Id);
        if (updated != null)
        {
            Recipe = updated;
            OnPropertyChanged(nameof(FavoriteButtonText));
            RecipeChanged?.Invoke();
        }
    }
}
