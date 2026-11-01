using System.Windows;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

public partial class RecipeDetailsWindow : Window
{
    public RecipeDetailsWindow(RecipeDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        viewModel.RequestClose += Close;
    }
}
