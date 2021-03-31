using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static EssentialDialogs.Enums;

namespace Examples
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            EssentialDialogs.Views.DialogWindow.ShowDialog("Just some text to see in the dialog.", "Title", EssentialDialogsOptions.Ok);


            EssentialDialogs.Views.DialogWindow.ShowDialog("Just some text to see in the dialog.", "Title", MaterialDesignThemes.Wpf.PackIconKind.Error, EssentialDialogsOptions.Ok, 120, EssentialDialogsResult.Ok);
        }
    }
}
