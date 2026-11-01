using System.Collections.ObjectModel;
using System.Windows.Input;
using RecipeBook.Models;
using RecipeBook.Services;

namespace RecipeBook.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private readonly RecipeFilterCriteria _criteria = new();

    private string _searchText = string.Empty;
    private string _selectedCategory = RecipeConstants.CategoryAll;
    private bool _isFavoritesMode;
    private Recipe? _selectedRecipe;
    private string _statusText = string.Empty;

    private int _currentPage = 1;
    private const int DefaultPageSize = 6;
    private int _totalFilteredCount;

    private int _totalCount;
    private int _soupsCount;
    private int _saladsCount;
    private int _mainDishesCount;
    private int _bakingCount;
    private int _dessertsCount;
    private int _beveragesCount;
    private int _favoritesCount;

    public event Action? RequestCreateRecipe;
    public event Action<Recipe>? RequestViewRecipe;
    public event Action<Recipe>? RequestEditRecipe;
    public event Func<Recipe, bool>? ConfirmDeleteRecipe;
    public event Func<RecipeFilterCriteria, RecipeFilterCriteria?>? RequestFilterDialog;

    public MainViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));

        Recipes = [];
        CategoriesFilterList = new List<string> { RecipeConstants.CategoryAll }
            .Concat(RecipeConstants.AllCategories)
            .ToList();

        CreateRecipeCommand = new RelayCommand(() => RequestCreateRecipe?.Invoke());
        ViewRecipeCommand = new RelayCommand(ViewRecipe, CanOperateWithSelected);
        EditRecipeCommand = new RelayCommand(EditRecipe, CanOperateWithSelected);
        DeleteRecipeCommand = new RelayCommand(DeleteRecipe, CanOperateWithSelected);
        ToggleFavoriteCommand = new RelayCommand(ToggleFavorite, CanOperateWithSelected);

        OpenFiltersCommand = new RelayCommand(OpenFilters);
        ResetFiltersCommand = new RelayCommand(ResetFilters);

        NextPageCommand = new RelayCommand(() =>
        {
            if (CanNextPage)
            {
                CurrentPage++;
                ReloadRecipes(keepPage: true);
            }
        });

        PrevPageCommand = new RelayCommand(() =>
        {
            if (CanPrevPage)
            {
                CurrentPage--;
                ReloadRecipes(keepPage: true);
            }
        });

        SelectCategoryCommand = new RelayCommand(category =>
        {
            if (category is string cat)
            {
                _isFavoritesMode = false;
                OnPropertyChanged(nameof(IsFavoritesMode));
                SelectedCategory = cat;
            }
        });

        ShowFavoritesCommand = new RelayCommand(() =>
        {
            _isFavoritesMode = true;
            OnPropertyChanged(nameof(IsFavoritesMode));
            _criteria.OnlyFavorites = true;
            _criteria.Category = null;
            CurrentPage = 1;
            ReloadRecipes();
        });

        ShowAllCommand = new RelayCommand(() =>
        {
            _isFavoritesMode = false;
            OnPropertyChanged(nameof(IsFavoritesMode));
            SelectedCategory = RecipeConstants.CategoryAll;
        });

        ReloadRecipes();
    }

    public ObservableCollection<Recipe> Recipes { get; }
    public IReadOnlyList<string> CategoriesFilterList { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                _criteria.SearchText = value;
                CurrentPage = 1;
                ReloadRecipes();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetField(ref _selectedCategory, value))
            {
                _criteria.Category = value;
                CurrentPage = 1;
                ReloadRecipes();
            }
        }
    }

    public bool IsFavoritesMode
    {
        get => _isFavoritesMode;
        set
        {
            if (SetField(ref _isFavoritesMode, value))
            {
                _criteria.OnlyFavorites = value;
                CurrentPage = 1;
                ReloadRecipes();
            }
        }
    }

    public Recipe? SelectedRecipe
    {
        get => _selectedRecipe;
        set => SetField(ref _selectedRecipe, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetField(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(CanPrevPage));
                OnPropertyChanged(nameof(CanNextPage));
            }
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalFilteredCount / DefaultPageSize));
    public bool CanPrevPage => CurrentPage > 1;
    public bool CanNextPage => CurrentPage < TotalPages;

    public int TotalCount { get => _totalCount; private set => SetField(ref _totalCount, value); }
    public int SoupsCount { get => _soupsCount; private set => SetField(ref _soupsCount, value); }
    public int SaladsCount { get => _saladsCount; private set => SetField(ref _saladsCount, value); }
    public int MainDishesCount { get => _mainDishesCount; private set => SetField(ref _mainDishesCount, value); }
    public int BakingCount { get => _bakingCount; private set => SetField(ref _bakingCount, value); }
    public int DessertsCount { get => _dessertsCount; private set => SetField(ref _dessertsCount, value); }
    public int BeveragesCount { get => _beveragesCount; private set => SetField(ref _beveragesCount, value); }
    public int FavoritesCount { get => _favoritesCount; private set => SetField(ref _favoritesCount, value); }

    public ICommand CreateRecipeCommand { get; }
    public ICommand ViewRecipeCommand { get; }
    public ICommand EditRecipeCommand { get; }
    public ICommand DeleteRecipeCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand OpenFiltersCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand ShowFavoritesCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }

    public void ReloadRecipes(bool keepPage = false)
    {
        if (!keepPage)
        {
            CurrentPage = 1;
        }

        var allFiltered = _recipeService.GetRecipes(_criteria);
        _totalFilteredCount = allFiltered.Count;

        if (CurrentPage > TotalPages)
        {
            CurrentPage = TotalPages;
        }

        var pagedItems = allFiltered
            .Skip((CurrentPage - 1) * DefaultPageSize)
            .Take(DefaultPageSize)
            .ToList();

        Recipes.Clear();
        foreach (var recipe in pagedItems)
        {
            Recipes.Add(recipe);
        }

        UpdateStatistics();
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(CanPrevPage));
        OnPropertyChanged(nameof(CanNextPage));

        StatusText = $"Показано {Recipes.Count} из {_totalFilteredCount} записей | Страница {CurrentPage} из {TotalPages}";
    }

    private void UpdateStatistics()
    {
        var stats = _recipeService.GetStatistics();
        TotalCount = stats.GetValueOrDefault("Total", 0);
        FavoritesCount = stats.GetValueOrDefault("Favorites", 0);
        SoupsCount = stats.GetValueOrDefault(RecipeConstants.CategorySoups, 0);
        SaladsCount = stats.GetValueOrDefault(RecipeConstants.CategorySalads, 0);
        MainDishesCount = stats.GetValueOrDefault(RecipeConstants.CategoryMainDishes, 0);
        BakingCount = stats.GetValueOrDefault(RecipeConstants.CategoryBaking, 0);
        DessertsCount = stats.GetValueOrDefault(RecipeConstants.CategoryDesserts, 0);
        BeveragesCount = stats.GetValueOrDefault(RecipeConstants.CategoryBeverages, 0);
    }

    private bool CanOperateWithSelected(object? parameter) => SelectedRecipe != null;

    private void ViewRecipe(object? parameter)
    {
        if (SelectedRecipe != null)
        {
            RequestViewRecipe?.Invoke(SelectedRecipe);
        }
    }

    private void EditRecipe(object? parameter)
    {
        if (SelectedRecipe != null)
        {
            RequestEditRecipe?.Invoke(SelectedRecipe);
        }
    }

    private void DeleteRecipe(object? parameter)
    {
        if (SelectedRecipe == null) return;

        var confirmed = ConfirmDeleteRecipe?.Invoke(SelectedRecipe) ?? false;
        if (confirmed)
        {
            _recipeService.DeleteRecipe(SelectedRecipe.Id);
            ReloadRecipes(keepPage: true);
        }
    }

    private void ToggleFavorite(object? parameter)
    {
        if (SelectedRecipe != null)
        {
            _recipeService.ToggleFavorite(SelectedRecipe.Id);
            ReloadRecipes(keepPage: true);
        }
    }

    private void OpenFilters()
    {
        var result = RequestFilterDialog?.Invoke(_criteria);
        if (result != null)
        {
            _criteria.Category = result.Category;
            _criteria.MinCookingTime = result.MinCookingTime;
            _criteria.MaxCookingTime = result.MaxCookingTime;
            _criteria.Difficulties = [.. result.Difficulties];
            _criteria.OnlyFavorites = result.OnlyFavorites;
            _criteria.Servings = result.Servings;

            if (!string.IsNullOrWhiteSpace(result.Category))
            {
                _selectedCategory = result.Category;
                OnPropertyChanged(nameof(SelectedCategory));
            }

            _isFavoritesMode = result.OnlyFavorites;
            OnPropertyChanged(nameof(IsFavoritesMode));

            ReloadRecipes();
        }
    }

    private void ResetFilters()
    {
        _searchText = string.Empty;
        OnPropertyChanged(nameof(SearchText));
        _selectedCategory = RecipeConstants.CategoryAll;
        OnPropertyChanged(nameof(SelectedCategory));
        _isFavoritesMode = false;
        OnPropertyChanged(nameof(IsFavoritesMode));

        _criteria.Reset();
        ReloadRecipes();
    }
}
