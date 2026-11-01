using System.Windows.Input;
using RecipeBook.Models;

namespace RecipeBook.ViewModels;

public class FilterDialogViewModel : ViewModelBase
{
    private bool _filterByCategory;
    private string _selectedCategory = RecipeConstants.CategoryMainDishes;

    private bool _filterByCookingTime;
    private int _minCookingTime = 10;
    private int _maxCookingTime = 60;

    private bool _filterByDifficulty;
    private bool _diff1 = true;
    private bool _diff2 = true;
    private bool _diff3 = true;
    private bool _diff4;
    private bool _diff5;

    private bool _filterByCollection;
    private bool _onlyFavorites = true;

    private bool _filterByServings;
    private int? _servings;

    public event Action<RecipeFilterCriteria?>? RequestClose;

    public FilterDialogViewModel(RecipeFilterCriteria? current = null)
    {
        Categories = RecipeConstants.AllCategories;

        if (current != null)
        {
            if (!string.IsNullOrWhiteSpace(current.Category) && current.Category != RecipeConstants.CategoryAll)
            {
                FilterByCategory = true;
                SelectedCategory = current.Category;
            }

            if (current.MinCookingTime.HasValue || current.MaxCookingTime.HasValue)
            {
                FilterByCookingTime = true;
                MinCookingTime = current.MinCookingTime ?? 10;
                MaxCookingTime = current.MaxCookingTime ?? 60;
            }

            if (current.Difficulties.Count > 0)
            {
                FilterByDifficulty = true;
                Diff1 = current.Difficulties.Contains(1);
                Diff2 = current.Difficulties.Contains(2);
                Diff3 = current.Difficulties.Contains(3);
                Diff4 = current.Difficulties.Contains(4);
                Diff5 = current.Difficulties.Contains(5);
            }

            if (current.OnlyFavorites)
            {
                FilterByCollection = true;
                OnlyFavorites = true;
            }

            if (current.Servings.HasValue)
            {
                FilterByServings = true;
                Servings = current.Servings;
            }
        }

        ApplyCommand = new RelayCommand(Apply);
        ResetCommand = new RelayCommand(Reset);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke(null));
    }

    public IReadOnlyList<string> Categories { get; }

    public bool FilterByCategory
    {
        get => _filterByCategory;
        set => SetField(ref _filterByCategory, value);
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public bool FilterByCookingTime
    {
        get => _filterByCookingTime;
        set => SetField(ref _filterByCookingTime, value);
    }

    public int MinCookingTime
    {
        get => _minCookingTime;
        set => SetField(ref _minCookingTime, value);
    }

    public int MaxCookingTime
    {
        get => _maxCookingTime;
        set => SetField(ref _maxCookingTime, value);
    }

    public bool FilterByDifficulty
    {
        get => _filterByDifficulty;
        set => SetField(ref _filterByDifficulty, value);
    }

    public bool Diff1 { get => _diff1; set => SetField(ref _diff1, value); }
    public bool Diff2 { get => _diff2; set => SetField(ref _diff2, value); }
    public bool Diff3 { get => _diff3; set => SetField(ref _diff3, value); }
    public bool Diff4 { get => _diff4; set => SetField(ref _diff4, value); }
    public bool Diff5 { get => _diff5; set => SetField(ref _diff5, value); }

    public bool FilterByCollection
    {
        get => _filterByCollection;
        set => SetField(ref _filterByCollection, value);
    }

    public bool OnlyFavorites
    {
        get => _onlyFavorites;
        set => SetField(ref _onlyFavorites, value);
    }

    public bool FilterByServings
    {
        get => _filterByServings;
        set => SetField(ref _filterByServings, value);
    }

    public int? Servings
    {
        get => _servings;
        set => SetField(ref _servings, value);
    }

    public ICommand ApplyCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand CancelCommand { get; }

    private void Apply()
    {
        var criteria = new RecipeFilterCriteria();

        if (FilterByCategory)
        {
            criteria.Category = SelectedCategory;
        }

        if (FilterByCookingTime)
        {
            criteria.MinCookingTime = MinCookingTime;
            criteria.MaxCookingTime = MaxCookingTime;
        }

        if (FilterByDifficulty)
        {
            if (Diff1) criteria.Difficulties.Add(1);
            if (Diff2) criteria.Difficulties.Add(2);
            if (Diff3) criteria.Difficulties.Add(3);
            if (Diff4) criteria.Difficulties.Add(4);
            if (Diff5) criteria.Difficulties.Add(5);
        }

        if (FilterByCollection)
        {
            criteria.OnlyFavorites = OnlyFavorites;
        }

        if (FilterByServings && Servings.HasValue && Servings.Value > 0)
        {
            criteria.Servings = Servings.Value;
        }

        RequestClose?.Invoke(criteria);
    }

    private void Reset()
    {
        FilterByCategory = false;
        FilterByCookingTime = false;
        FilterByDifficulty = false;
        FilterByCollection = false;
        FilterByServings = false;
        Servings = null;
        RequestClose?.Invoke(new RecipeFilterCriteria());
    }
}
