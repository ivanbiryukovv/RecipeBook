using System.Windows;
using RecipeBook.Models;
using RecipeBook.ViewModels;

namespace RecipeBook.Views;

public partial class FilterWindow : Window
{
    public RecipeFilterCriteria? SelectedCriteria { get; private set; }

    public FilterWindow(FilterDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        viewModel.RequestClose += criteria =>
        {
            SelectedCriteria = criteria;
            DialogResult = criteria != null;
            Close();
        };
    }
}
