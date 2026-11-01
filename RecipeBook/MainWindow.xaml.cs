using System.Windows;
using System.Windows.Input;
using RecipeBook.Data;
using RecipeBook.Models;
using RecipeBook.Services;
using RecipeBook.ViewModels;
using RecipeBook.Views;

namespace RecipeBook;

public partial class MainWindow : Window
{
    private readonly IRecipeRepository _repository;
    private readonly IRecipeService _recipeService;
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _repository = new LiteDbRecipeRepository();
        _recipeService = new RecipeService(_repository);

        _viewModel = new MainViewModel(_recipeService);
        DataContext = _viewModel;

        _viewModel.RequestCreateRecipe += OpenCreateRecipeDialog;
        _viewModel.RequestViewRecipe += OpenRecipeDetailsDialog;
        _viewModel.RequestEditRecipe += OpenEditRecipeDialog;
        _viewModel.ConfirmDeleteRecipe += ShowDeleteConfirmationDialog;
        _viewModel.RequestFilterDialog += ShowFilterDialog;

        Closed += (_, _) => _repository.Dispose();
    }

    private void OpenCreateRecipeDialog()
    {
        var addVm = new AddEditRecipeViewModel(_recipeService);
        var dialog = new AddEditRecipeWindow(addVm) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.ReloadRecipes();
        }
    }

    private void OpenRecipeDetailsDialog(Recipe recipe)
    {
        var detailsVm = new RecipeDetailsViewModel(_recipeService, recipe);
        var dialog = new RecipeDetailsWindow(detailsVm) { Owner = this };

        detailsVm.RequestEdit += r =>
        {
            dialog.Close();
            OpenEditRecipeDialog(r);
        };

        detailsVm.RecipeDeleted += r =>
        {
            if (ShowDeleteConfirmationDialog(r))
            {
                _recipeService.DeleteRecipe(r.Id);
                dialog.Close();
                _viewModel.ReloadRecipes(keepPage: true);
            }
        };

        detailsVm.RecipeChanged += () => _viewModel.ReloadRecipes(keepPage: true);

        dialog.ShowDialog();
        _viewModel.ReloadRecipes(keepPage: true);
    }

    private void OpenEditRecipeDialog(Recipe recipe)
    {
        var editVm = new AddEditRecipeViewModel(_recipeService, recipe);
        var dialog = new AddEditRecipeWindow(editVm) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.ReloadRecipes(keepPage: true);
        }
    }

    private bool ShowDeleteConfirmationDialog(Recipe recipe)
    {
        var message = $"Удалить рецепт № {recipe.Id}?\n\n" +
                      $"Рецепт «{recipe.Title}» будет удален из базы данных LiteDB. " +
                      $"Это действие нельзя отменить.";

        var result = MessageBox.Show(
            this,
            message,
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }

    private RecipeFilterCriteria? ShowFilterDialog(RecipeFilterCriteria current)
    {
        var filterVm = new FilterDialogViewModel(current);
        var dialog = new FilterWindow(filterVm) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            return dialog.SelectedCriteria;
        }

        return null;
    }

    private void OnRecipeGridDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.SelectedRecipe != null)
        {
            OpenRecipeDetailsDialog(_viewModel.SelectedRecipe);
        }
    }
}
