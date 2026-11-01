using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RecipeBook.Models;
using RecipeBook.Services;

namespace RecipeBook.ViewModels;

public class AddEditRecipeViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;
    private readonly int _editingRecipeId;

    private string _title = string.Empty;
    private string _selectedCategory = RecipeConstants.CategoryMainDishes;
    private int _difficulty = 2;
    private int _cookingTimeMinutes = 30;
    private int _servings = 4;
    private string _imagePath = string.Empty;
    private string _description = string.Empty;
    private string _instructionsText = string.Empty;
    private bool _isFavorite;

    private string _newIngredientName = string.Empty;
    private double _newIngredientQuantity = 100;
    private string _newIngredientUnit = "грамм";

    private bool _hasTitleError;
    private string _titleErrorMessage = string.Empty;
    private bool _hasCookingTimeError;
    private string _cookingTimeErrorMessage = string.Empty;
    private bool _hasServingsError;
    private string _servingsErrorMessage = string.Empty;
    private string _generalErrorMessage = string.Empty;

    public event Action? RequestClose;

    public bool IsSavedSuccessfully { get; private set; }

    public AddEditRecipeViewModel(IRecipeService recipeService, Recipe? existingRecipe = null)
    {
        _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));

        Categories = RecipeConstants.AllCategories;
        DifficultyOptions =
        [
            new(1, "1 - Очень легко"),
            new(2, "2 - Легко"),
            new(3, "3 - Средне"),
            new(4, "4 - Сложно"),
            new(5, "5 - Мастер-шеф")
        ];

        Ingredients = [];

        if (existingRecipe != null)
        {
            _editingRecipeId = existingRecipe.Id;
            Title = existingRecipe.Title;
            SelectedCategory = existingRecipe.Category;
            Difficulty = existingRecipe.Difficulty;
            CookingTimeMinutes = existingRecipe.CookingTimeMinutes;
            Servings = existingRecipe.Servings;
            ImagePath = existingRecipe.ImagePath;
            Description = existingRecipe.Description;
            InstructionsText = existingRecipe.InstructionsText;
            IsFavorite = existingRecipe.IsFavorite;

            foreach (var ing in existingRecipe.Ingredients)
            {
                Ingredients.Add(new RecipeIngredient(ing.Name, ing.Quantity, ing.Unit));
            }

            WindowCaption = $"Редактирование рецепта № {_editingRecipeId}";
        }
        else
        {
            WindowCaption = "Новый рецепт";
            Ingredients.Add(new RecipeIngredient("Основной ингредиент", 200, "г."));
        }

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        BrowseImageCommand = new RelayCommand(BrowseImage);
        AddIngredientCommand = new RelayCommand(AddIngredient, CanAddIngredient);
        RemoveIngredientCommand = new RelayCommand(RemoveIngredient);
    }

    public string WindowCaption { get; }
    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<DifficultyOption> DifficultyOptions { get; }
    public ObservableCollection<RecipeIngredient> Ingredients { get; }

    public string Title
    {
        get => _title;
        set
        {
            if (SetField(ref _title, value))
            {
                ValidateTitle();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public int Difficulty
    {
        get => _difficulty;
        set => SetField(ref _difficulty, value);
    }

    public int CookingTimeMinutes
    {
        get => _cookingTimeMinutes;
        set
        {
            if (SetField(ref _cookingTimeMinutes, value))
            {
                ValidateCookingTime();
            }
        }
    }

    public int Servings
    {
        get => _servings;
        set
        {
            if (SetField(ref _servings, value))
            {
                ValidateServings();
            }
        }
    }

    public string ImagePath
    {
        get => _imagePath;
        set => SetField(ref _imagePath, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public string InstructionsText
    {
        get => _instructionsText;
        set => SetField(ref _instructionsText, value);
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetField(ref _isFavorite, value);
    }

    public string NewIngredientName
    {
        get => _newIngredientName;
        set => SetField(ref _newIngredientName, value);
    }

    public double NewIngredientQuantity
    {
        get => _newIngredientQuantity;
        set => SetField(ref _newIngredientQuantity, value);
    }

    public string NewIngredientUnit
    {
        get => _newIngredientUnit;
        set => SetField(ref _newIngredientUnit, value);
    }

    public bool HasTitleError
    {
        get => _hasTitleError;
        set => SetField(ref _hasTitleError, value);
    }

    public string TitleErrorMessage
    {
        get => _titleErrorMessage;
        set => SetField(ref _titleErrorMessage, value);
    }

    public bool HasCookingTimeError
    {
        get => _hasCookingTimeError;
        set => SetField(ref _hasCookingTimeError, value);
    }

    public string CookingTimeErrorMessage
    {
        get => _cookingTimeErrorMessage;
        set => SetField(ref _cookingTimeErrorMessage, value);
    }

    public bool HasServingsError
    {
        get => _hasServingsError;
        set => SetField(ref _hasServingsError, value);
    }

    public string ServingsErrorMessage
    {
        get => _servingsErrorMessage;
        set => SetField(ref _servingsErrorMessage, value);
    }

    public string GeneralErrorMessage
    {
        get => _generalErrorMessage;
        set => SetField(ref _generalErrorMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseImageCommand { get; }
    public ICommand AddIngredientCommand { get; }
    public ICommand RemoveIngredientCommand { get; }

    private void ValidateTitle()
    {
        if (string.IsNullOrWhiteSpace(_title))
        {
            HasTitleError = true;
            TitleErrorMessage = "Поле «Название блюда» обязательно для заполнения.";
        }
        else if (_title.Trim().Length < RecipeConstants.MinTitleLength)
        {
            HasTitleError = true;
            TitleErrorMessage = $"Название должно быть не короче {RecipeConstants.MinTitleLength} символов.";
        }
        else
        {
            HasTitleError = false;
            TitleErrorMessage = string.Empty;
        }
    }

    private void ValidateCookingTime()
    {
        if (_cookingTimeMinutes < RecipeConstants.MinCookingTimeMinutes)
        {
            HasCookingTimeError = true;
            CookingTimeErrorMessage = "Время приготовления должно быть больше 0 минут";
        }
        else if (_cookingTimeMinutes > RecipeConstants.MaxCookingTimeMinutes)
        {
            HasCookingTimeError = true;
            CookingTimeErrorMessage = $"Время не может превышать {RecipeConstants.MaxCookingTimeMinutes} минут.";
        }
        else
        {
            HasCookingTimeError = false;
            CookingTimeErrorMessage = string.Empty;
        }
    }

    private void ValidateServings()
    {
        if (_servings < RecipeConstants.MinServingsCount)
        {
            HasServingsError = true;
            ServingsErrorMessage = "Количество порций должно быть не менее 1.";
        }
        else
        {
            HasServingsError = false;
            ServingsErrorMessage = string.Empty;
        }
    }

    private void Save()
    {
        ValidateTitle();
        ValidateCookingTime();
        ValidateServings();

        if (HasTitleError || HasCookingTimeError || HasServingsError)
        {
            GeneralErrorMessage = "Пожалуйста, исправьте ошибки заполнения полей.";
            return;
        }

        var recipe = new Recipe
        {
            Id = _editingRecipeId,
            Title = _title.Trim(),
            Category = _selectedCategory,
            Difficulty = _difficulty,
            CookingTimeMinutes = _cookingTimeMinutes,
            Servings = _servings,
            ImagePath = _imagePath,
            Description = _description.Trim(),
            InstructionsText = _instructionsText,
            IsFavorite = _isFavorite,
            Ingredients = [.. Ingredients]
        };

        if (_editingRecipeId == 0)
        {
            var (isSuccess, validation, _) = _recipeService.CreateRecipe(recipe);
            if (!isSuccess)
            {
                GeneralErrorMessage = validation.GetFormattedErrors();
                return;
            }
        }
        else
        {
            var (isSuccess, validation) = _recipeService.UpdateRecipe(recipe);
            if (!isSuccess)
            {
                GeneralErrorMessage = validation.GetFormattedErrors();
                return;
            }
        }

        IsSavedSuccessfully = true;
        RequestClose?.Invoke();
    }

    private void Cancel()
    {
        IsSavedSuccessfully = false;
        RequestClose?.Invoke();
    }

    private void BrowseImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
            Title = "Выберите фотографию блюда"
        };

        if (dialog.ShowDialog() == true)
        {
            ImagePath = dialog.FileName;
        }
    }

    private bool CanAddIngredient()
    {
        return !string.IsNullOrWhiteSpace(_newIngredientName) && _newIngredientQuantity > 0;
    }

    private void AddIngredient()
    {
        Ingredients.Add(new RecipeIngredient(_newIngredientName.Trim(), _newIngredientQuantity, _newIngredientUnit.Trim()));
        NewIngredientName = string.Empty;
        NewIngredientQuantity = 100;
    }

    private void RemoveIngredient(object? parameter)
    {
        if (parameter is RecipeIngredient ingredient)
        {
            Ingredients.Remove(ingredient);
        }
    }
}

public record DifficultyOption(int Value, string DisplayName);
