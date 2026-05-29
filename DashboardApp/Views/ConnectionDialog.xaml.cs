using DashboardApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DashboardApp.Views;

public partial class ConnectionDialog : Window
{
    public ConnectionDialog()
    {
        InitializeComponent();

        // Подписываемся на закрытие окна из ViewModel
        Loaded += ConnectionDialog_Loaded;
    }

    private void ConnectionDialog_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConnectionViewModel vm)
        {
            vm.CloseAction = () => this.Close();
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ConnectionViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }
}