using System.Windows;
using YTDLManager.ViewModels;

namespace YTDLManager.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
