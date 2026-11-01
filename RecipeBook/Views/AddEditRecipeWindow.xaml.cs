using System.Windows;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

public partial class AddEditRecipeWindow : Window
{
    public AddEditRecipeWindow(AddEditRecipeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        viewModel.RequestClose += () =>
        {
            DialogResult = viewModel.IsSavedSuccessfully;
            Close();
        };
    }
}
