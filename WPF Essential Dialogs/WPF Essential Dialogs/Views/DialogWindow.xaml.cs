using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using EssentialDialogs.ViewModels;
using static EssentialDialogs.Enums;

namespace EssentialDialogs.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        public DialogViewModel ViewModel { get; set; }

        private DialogWindow(DialogViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            ViewModel.Window = this;
            this.Topmost = true;

            

            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsInitialized && Application.Current.MainWindow.IsLoaded)
            {
                this.Owner = Application.Current.MainWindow;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            this.DataContext = this.ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.SetWindowLong(hwnd, GWL_STYLE, NativeMethods.GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            if (ViewModel.AutoClose)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                _ = Task.Run(async() => {
                    DateTime closureTime = DateTime.Now.AddSeconds(ViewModel.AutoCloseSeconds + 1);

                    while (!cancellationTokenSource.IsCancellationRequested && closureTime.AddSeconds(-1) > DateTime.Now)
                    {
                        await Task.Delay(10, cancellationTokenSource.Token);

                        switch (ViewModel.AutoCloseResult)
                        {
                            case EssentialDialogsResult.Cancel:
                                ViewModel.ButtonCancelContent = $"Cancel ({(closureTime - DateTime.Now):ss})";
                                break;
                            case EssentialDialogsResult.No:
                                ViewModel.ButtonNoContent = $"No ({(closureTime - DateTime.Now):ss})";
                                break;
                            case EssentialDialogsResult.Ok:
                                ViewModel.ButtonOkContent = $"Ok ({(closureTime - DateTime.Now):ss})";
                                break;
                            case EssentialDialogsResult.Selected:
                                ViewModel.ButtonSelectContent = $"Select ({(closureTime - DateTime.Now):ss})";
                                break;
                            case EssentialDialogsResult.Yes:
                                ViewModel.ButtonYesContent = $"Yes ({(closureTime - DateTime.Now):ss})";
                                break;
                            default:
                                break;
                        }
                    }

                    if (!cancellationTokenSource.IsCancellationRequested)
                    {
                        this.Dispatcher.Invoke(() => {
                            ViewModel.DialogResult = ViewModel.AutoCloseResult;
                            this.Close();
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                    }
                }, cancellationTokenSource.Token);

                this.Closing += delegate { cancellationTokenSource.Cancel(); };
            }
        }

        private static bool OptionsContainsResult(EssentialDialogsOptions options, EssentialDialogsResult result)
        {
            switch (options)
            {
                case EssentialDialogsOptions.Ok:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => false,
                        EssentialDialogsResult.No => false,
                        EssentialDialogsResult.Ok => true,
                        EssentialDialogsResult.Selected => false,
                        EssentialDialogsResult.Yes => false,
                        _ => false,
                    };
                case EssentialDialogsOptions.OkCancel:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => true,
                        EssentialDialogsResult.No => false,
                        EssentialDialogsResult.Ok => true,
                        EssentialDialogsResult.Selected => false,
                        EssentialDialogsResult.Yes => false,
                        _ => false,
                    };
                case EssentialDialogsOptions.Select:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => false,
                        EssentialDialogsResult.No => false,
                        EssentialDialogsResult.Ok => false,
                        EssentialDialogsResult.Selected => true,
                        EssentialDialogsResult.Yes => false,
                        _ => false,
                    };
                case EssentialDialogsOptions.SelectCancel:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => true,
                        EssentialDialogsResult.No => false,
                        EssentialDialogsResult.Ok => false,
                        EssentialDialogsResult.Selected => true,
                        EssentialDialogsResult.Yes => false,
                        _ => false,
                    };
                case EssentialDialogsOptions.YesNo:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => false,
                        EssentialDialogsResult.No => true,
                        EssentialDialogsResult.Ok => false,
                        EssentialDialogsResult.Selected => false,
                        EssentialDialogsResult.Yes => true,
                        _ => false,
                    };
                case EssentialDialogsOptions.YesNoCancel:
                    return result switch
                    {
                        EssentialDialogsResult.Cancel => true,
                        EssentialDialogsResult.No => true,
                        EssentialDialogsResult.Ok => false,
                        EssentialDialogsResult.Selected => false,
                        EssentialDialogsResult.Yes => true,
                        _ => false,
                    };
                default:
                    return false;
            }
        }

        #region ShowDialog

        public static EssentialDialogsResult ShowDialog(string message, EssentialDialogsOptions options = EssentialDialogsOptions.Ok, int timedAutoCloseSeconds = 0, EssentialDialogsResult autoCloseResult = EssentialDialogsResult.Ok)
        {
            if (timedAutoCloseSeconds != 0 && !OptionsContainsResult(options, autoCloseResult))
            {
                throw new ArgumentException($"The auto close result <{autoCloseResult}> is not provided in the given options <{options}>.");
            }

            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Options = options,
                AutoClose = (timedAutoCloseSeconds != 0),
                AutoCloseSeconds = timedAutoCloseSeconds,
                AutoCloseResult = autoCloseResult
            };

            return ShowDialog(inputViewModel);
        }

        public static EssentialDialogsResult ShowDialog(string message, string title, EssentialDialogsOptions options = EssentialDialogsOptions.Ok, int timedAutoCloseSeconds = 0, EssentialDialogsResult autoCloseResult = EssentialDialogsResult.Ok)
        {
            if (timedAutoCloseSeconds != 0 && !OptionsContainsResult(options, autoCloseResult))
            {
                throw new ArgumentException($"The auto close result <{autoCloseResult}> is not provided in the given options <{options}>.");
            }

            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = options,
                AutoClose = (timedAutoCloseSeconds != 0),
                AutoCloseSeconds = timedAutoCloseSeconds,
                AutoCloseResult = autoCloseResult
            };

            return ShowDialog(inputViewModel);
        }

        public static EssentialDialogsResult ShowDialog(string message, MaterialDesignThemes.Wpf.PackIconKind icon, EssentialDialogsOptions options = EssentialDialogsOptions.Ok, int timedAutoCloseSeconds = 0, EssentialDialogsResult autoCloseResult = EssentialDialogsResult.Ok)
        {
            if (timedAutoCloseSeconds != 0 && !OptionsContainsResult(options, autoCloseResult))
            {
                throw new ArgumentException($"The auto close result <{autoCloseResult}> is not provided in the given options <{options}>.");
            }

            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Options = options,
                Icon = icon,
                AutoClose = (timedAutoCloseSeconds != 0),
                AutoCloseSeconds = timedAutoCloseSeconds,
                AutoCloseResult = autoCloseResult
            };

            return ShowDialog(inputViewModel);
        }

        public static EssentialDialogsResult ShowDialog(string message, string title, MaterialDesignThemes.Wpf.PackIconKind icon, EssentialDialogsOptions options = EssentialDialogsOptions.Ok, int timedAutoCloseSeconds = 0, EssentialDialogsResult autoCloseResult = EssentialDialogsResult.Ok)
        {
            if (timedAutoCloseSeconds != 0 && !OptionsContainsResult(options, autoCloseResult))
            {
                throw new ArgumentException($"The auto close result <{autoCloseResult}> is not provided in the given options <{options}>.");
            }

            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = options,
                Icon = icon,
                AutoClose = (timedAutoCloseSeconds != 0),
                AutoCloseSeconds = timedAutoCloseSeconds,
                AutoCloseResult = autoCloseResult
            };

            return ShowDialog(inputViewModel);
        }

        public static (EssentialDialogsResult, object) ShowDialog(string message, string title, List<object> selectionList, string displayMember = null, SelectionMode selectionMode = SelectionMode.Single, bool forceSelect = false)
        {
            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = (forceSelect) ? EssentialDialogsOptions.Select : EssentialDialogsOptions.SelectCancel,
                SelectionMode = selectionMode,
                SelectionList = selectionList,
                SelectionDisplayMember = displayMember
            };

            return ShowSelectionDialog(inputViewModel);
        }

        public static (EssentialDialogsResult, object) ShowDialog(string message, string title, MaterialDesignThemes.Wpf.PackIconKind icon, List<object> selectionList, string displayMember = null, SelectionMode selectionMode = SelectionMode.Single, bool forceSelect = false)
        {
            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = (forceSelect) ? EssentialDialogsOptions.Select : EssentialDialogsOptions.SelectCancel,
                Icon = icon,
                SelectionMode = selectionMode,
                SelectionList = selectionList,
                SelectionDisplayMember = displayMember
            };

            return ShowSelectionDialog(inputViewModel);
        }

        public static (EssentialDialogsResult, DateTime?) ShowDialog(string message, string title, DateTime rangeMin, DateTime? rangeMax = null, bool showTime = false)
        {
            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = EssentialDialogsOptions.SelectCancel,
                DateFrom = rangeMin,
                DateTo = (rangeMax.HasValue) ? rangeMax : null,
                ShowTime = showTime,
                SelectedDate = rangeMin,
                SelectedTime = rangeMin
            };

            return ShowDateTimeDialog(inputViewModel);
        }

        public static (EssentialDialogsResult, string) ShowDialog(string message, string title, string hint, EssentialDialogsOptions inputDialogOptions = EssentialDialogsOptions.OkCancel)
        {
            DialogViewModel inputViewModel = new DialogViewModel()
            {
                Message = message,
                Title = title,
                Options = inputDialogOptions,
                InputTextHint = hint
            };

            return ShowTextInputDialog(inputViewModel);
        }

        #endregion

        [STAThread]
        private static EssentialDialogsResult ShowDialog(DialogViewModel inputViewModel)
        {
            EssentialDialogsResult returnIt()
            {
                DialogWindow DialogWindow = new DialogWindow(inputViewModel);

                if (DialogWindow.ViewModel.IconVisibility == Visibility.Collapsed
                    && string.IsNullOrEmpty(DialogWindow.ViewModel.Title))
                {
                    DialogWindow.ViewModel.Card_Title_Visibility = Visibility.Collapsed;
                }

                DialogWindow.WindowStartupLocation = DialogWindow.Owner != null ? inputViewModel.StartupLocation : WindowStartupLocation.CenterScreen;
                DialogWindow.ShowDialog();

                return DialogWindow.ViewModel.DialogResult;
            }

            if (Application.Current.MainWindow != null)
            {
                return Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    return returnIt();
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            else
            {
                return returnIt();
            }
        }

        [STAThread]
        private static (EssentialDialogsResult, object) ShowSelectionDialog(DialogViewModel inputViewModel)
        {
            (EssentialDialogsResult, object) returnIt()
            {
                DialogWindow DialogWindow = new DialogWindow(inputViewModel);

                if (DialogWindow.ViewModel.IconVisibility == Visibility.Collapsed
                    && string.IsNullOrEmpty(DialogWindow.ViewModel.Title))
                {
                    DialogWindow.ViewModel.Card_Title_Visibility = Visibility.Collapsed;
                }

                DialogWindow.WindowStartupLocation = DialogWindow.Owner != null ? inputViewModel.StartupLocation : WindowStartupLocation.CenterScreen;
                DialogWindow.ShowDialog();

                if (inputViewModel.SelectionMode == SelectionMode.Multiple)
                {
                    List<object> objects = new List<object>();
                    foreach (object item in DialogWindow.ComboBox_Select.SelectedItems)
                    {
                        objects.Add(item);
                    }

                    return (DialogWindow.ViewModel.DialogResult, objects);
                }
                else
                {
                    return (DialogWindow.ViewModel.DialogResult, DialogWindow.ViewModel.SelectionResult);
                }
            }

            if (Application.Current.MainWindow != null)
            {
                return Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    return returnIt();
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            else
            {
                return returnIt();
            }
        }

        [STAThread]
        private static (EssentialDialogsResult, DateTime?) ShowDateTimeDialog(DialogViewModel inputViewModel)
        {
            (EssentialDialogsResult, DateTime?) returnIt()
            {
                DialogWindow DialogWindow = new DialogWindow(inputViewModel);

                if (DialogWindow.ViewModel.IconVisibility == Visibility.Collapsed
                    && string.IsNullOrEmpty(DialogWindow.ViewModel.Title))
                {
                    DialogWindow.ViewModel.Card_Title_Visibility = Visibility.Collapsed;
                }

                DialogWindow.WindowStartupLocation = DialogWindow.Owner != null ? inputViewModel.StartupLocation : WindowStartupLocation.CenterScreen;
                DialogWindow.ShowDialog();

                DateTime? selectedDate = DialogWindow.ViewModel.SelectedDate;

                if (DialogWindow.ViewModel.ShowTime
                    && DialogWindow.ViewModel.SelectedTime.HasValue
                    && selectedDate.HasValue)
                {
                    selectedDate = selectedDate.Value.Add(DialogWindow.ViewModel.SelectedTime.Value.TimeOfDay);
                }

                return (DialogWindow.ViewModel.DialogResult, selectedDate);
            }

            if (Application.Current.MainWindow != null)
            {
                return Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    return returnIt();
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            else
            {
                return returnIt();
            }
        }

        [STAThread]
        private static (EssentialDialogsResult, string) ShowTextInputDialog(DialogViewModel inputViewModel)
        {
            (EssentialDialogsResult, string) returnIt()
            {
                DialogWindow DialogWindow = new DialogWindow(inputViewModel);

                if (DialogWindow.ViewModel.IconVisibility == Visibility.Collapsed
                    && string.IsNullOrEmpty(DialogWindow.ViewModel.Title))
                {
                    DialogWindow.ViewModel.Card_Title_Visibility = Visibility.Collapsed;
                }

                DialogWindow.WindowStartupLocation = DialogWindow.Owner != null ? inputViewModel.StartupLocation : WindowStartupLocation.CenterScreen;
                DialogWindow.ShowDialog();

                return (DialogWindow.ViewModel.DialogResult, DialogWindow.ViewModel.InputText);
            }

            if (Application.Current.MainWindow != null)
            {
                return Application.Current.MainWindow.Dispatcher.Invoke(() =>
                {
                    return returnIt();
                }, System.Windows.Threading.DispatcherPriority.Normal);
            }
            else
            {
                return returnIt();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                e.Handled = true;

                switch (ViewModel.Options)
                {
                    case EssentialDialogsOptions.Ok:
                        ViewModel.CommandOk.Execute(null);
                        break;
                    case EssentialDialogsOptions.OkCancel:
                        ViewModel.CommandOk.Execute(null);
                        break;
                    case EssentialDialogsOptions.Select:
                        ViewModel.CommandSelect.Execute(null);
                        break;
                    case EssentialDialogsOptions.SelectCancel:
                        ViewModel.CommandSelect.Execute(null);
                        break;
                    case EssentialDialogsOptions.YesNo:
                        ViewModel.CommandYes.Execute(null);
                        break;
                    case EssentialDialogsOptions.YesNoCancel:
                        ViewModel.CommandYes.Execute(null);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
