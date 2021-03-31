using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static EssentialDialogs.Enums;

namespace EssentialDialogs.ViewModels
{
    public class DialogViewModel : ObservableViewModel
    {
        internal Window Window { get; set; }
        private string _title = "";
        private string _message = "";
        private EssentialDialogsOptions _options = EssentialDialogsOptions.Ok;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterScreen;
        private MaterialDesignThemes.Wpf.PackIconKind _icon;
        private List<object> _selectionList;
        private string _selectionDisplayMember;
        private Thickness _titleMargin;
        private object _selectionResult = null;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private DateTime? _selectedDate = DateTime.Today;
        private DateTime? _selectedTime = DateTime.Now;
        private bool _showTime;
        private string _inputText;
        private string _inputTextHint;
        private SelectionMode _selectionMode;
        private string buttonOkContent = "Ok";
        private string buttonYesContent = "Yes";
        private string buttonNoContent = "No";
        private string buttonSelectContent = "Select";
        private string buttonCancelContent = "Cancel";

        public EssentialDialogsResult DialogResult { get; internal set; }

        #region GeneralProperties

        public string Title { get => _title; set => _title = value; }

        public string Message { get => _message; set => _message = value; }

        public EssentialDialogsOptions Options
        {
            get => _options;
            set
            {
                _options = value;

                switch (_options)
                {
                    case EssentialDialogsOptions.Ok:
                        Button_Ok_Visibility = Visibility.Visible;
                        break;
                    case EssentialDialogsOptions.OkCancel:
                        Button_Ok_Visibility = Visibility.Visible;
                        Button_Cancel_Visibility = Visibility.Visible;
                        break;
                    case EssentialDialogsOptions.Select:
                        Button_Select_Visibility = Visibility.Visible;
                        break;
                    case EssentialDialogsOptions.SelectCancel:
                        Button_Select_Visibility = Visibility.Visible;
                        Button_Cancel_Visibility = Visibility.Visible;
                        break;
                    case EssentialDialogsOptions.YesNoCancel:
                        Button_Yes_Visibility = Visibility.Visible;
                        Button_No_Visibility = Visibility.Visible;
                        Button_Cancel_Visibility = Visibility.Visible;
                        break;
                    case EssentialDialogsOptions.YesNo:
                        Button_Yes_Visibility = Visibility.Visible;
                        Button_No_Visibility = Visibility.Visible;
                        break;
                    default:
                        Button_Ok_Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        public MaterialDesignThemes.Wpf.PackIconKind Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                IconVisibility = Visibility.Visible;
            }
        }

        public ImageSource LogoSource { get => GlobalOptions.LogoImageSource ?? null; }

        public WindowStartupLocation StartupLocation { get => _startupLocation; set => _startupLocation = value; }

        public Thickness TitleMargin { get => _titleMargin; set => _titleMargin = value; }

        #endregion

        #region Commands

        public ICommand CommandYes { get; set; }

        public ICommand CommandOk { get; set; }

        public ICommand CommandSelect { get; set; }

        public ICommand CommandNo { get; set; }

        public ICommand CommandCancel { get; set; }

        #endregion

        #region Visibility

        public Visibility Card_Title_Visibility { get; set; } = Visibility.Visible;

        public Visibility IconVisibility { get; set; } = Visibility.Collapsed;

        public Visibility Button_Yes_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility Button_Ok_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility Button_Select_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility Button_No_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility Button_Cancel_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility DatePicker_Selection_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility TimePicker_Selection_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility TextBox_Visibility { get; set; } = Visibility.Collapsed;

        public Visibility ComboBox_Selection_Visibility { get; set; } = Visibility.Collapsed;

        #endregion

        #region ButtonContentProperties

        public string ButtonOkContent
        {
            get => buttonOkContent;
            set
            {
                if (buttonOkContent != value)
                {
                    buttonOkContent = value;
                    OnPropertyChanged(nameof(ButtonOkContent));
                }
            }
        }

        public string ButtonYesContent
        {
            get => buttonYesContent;
            set
            {
                if (buttonYesContent != value)
                {
                    buttonYesContent = value;
                    OnPropertyChanged(nameof(ButtonYesContent));
                }
            }
        }

        public string ButtonNoContent
        {
            get => buttonNoContent;
            set
            {
                if (buttonNoContent != value)
                {
                    buttonNoContent = value;
                    OnPropertyChanged(nameof(ButtonNoContent));
                }
            }
        }

        public string ButtonSelectContent
        {
            get => buttonSelectContent;
            set
            {
                if (buttonSelectContent != value)
                {
                    buttonSelectContent = value;
                    OnPropertyChanged(nameof(ButtonSelectContent));
                }
            }
        }

        public string ButtonCancelContent
        {
            get => buttonCancelContent;
            set
            {
                if (buttonCancelContent != value)
                {
                    buttonCancelContent = value;
                    OnPropertyChanged(nameof(ButtonCancelContent));
                }
            }
        }

        #endregion

        #region SelectorProperties

        public List<object> SelectionList
        {
            get => _selectionList;
            set
            {
                _selectionList = value;
                ComboBox_Selection_Visibility = Visibility.Visible;
            }
        }

        public SelectionMode SelectionMode { get => _selectionMode; set => _selectionMode = value; }

        public string SelectionDisplayMember { get; set; }

        public object SelectionResult { get; set; }

        #endregion

        #region DateTimeProperties

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                DatePicker_Selection_Visibility = Visibility.Visible;
            }
        }

        public DateTime? DateTo { get; set; }

        public bool ShowTime
        {
            get => _showTime;
            set
            {
                _showTime = value;
                if (_showTime)
                {
                    TimePicker_Selection_Visibility = Visibility.Visible;
                }
            }
        }

        public DateTime? SelectedDate { get => _selectedDate; set => _selectedDate = new DateTime(value.Value.Year, value.Value.Month, value.Value.Day, 0, 0, 0); }

        public DateTime? SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (_selectedTime.Value.TimeOfDay < DateFrom.Value.TimeOfDay)
                {
                    _selectedTime = new DateTime(_selectedTime.Value.Year, _selectedTime.Value.Month, _selectedTime.Value.Day, DateFrom.Value.TimeOfDay.Hours, DateFrom.Value.TimeOfDay.Minutes, DateFrom.Value.TimeOfDay.Seconds);
                }
                else if (DateTo.HasValue && _selectedTime.Value.TimeOfDay > DateTo.Value.TimeOfDay)
                {
                    _selectedTime = new DateTime(_selectedTime.Value.Year, _selectedTime.Value.Month, _selectedTime.Value.Day, DateTo.Value.TimeOfDay.Hours, DateTo.Value.TimeOfDay.Minutes, DateTo.Value.TimeOfDay.Seconds);
                }
                else
                {
                    _selectedTime = value;
                }
            }
        }

        #endregion

        #region TextProperties

        public string InputTextHint
        {
            get => _inputTextHint;
            set
            {
                _inputTextHint = value;
                TextBox_Visibility = Visibility.Visible;
            }
        }

        public string InputText { get => _inputText; set => _inputText = value; }

        #endregion

        #region AutoClosePropeties

        public bool AutoClose { get; internal set; }

        public int AutoCloseSeconds { get; internal set; }

        public EssentialDialogsResult AutoCloseResult { get; internal set; }

        #endregion

        public DialogViewModel()
        {
            CommandYes = new CommandImplementation(o => { 
                DialogResult = EssentialDialogsResult.Yes; 
                Window.Close(); 
            });

            CommandOk = new CommandImplementation(o => { 
                DialogResult = EssentialDialogsResult.Ok; 
                Window.Close(); 
            });

            CommandSelect = new CommandImplementation(o => { 
                DialogResult = EssentialDialogsResult.Selected; 
                Window.Close(); 
            });

            CommandNo = new CommandImplementation(o => { 
                DialogResult = EssentialDialogsResult.No; 
                Window.Close(); 
            });

            CommandCancel = new CommandImplementation(o => { 
                DialogResult = EssentialDialogsResult.Cancel; 
                Window.Close(); 
            });
        }
    }
}
