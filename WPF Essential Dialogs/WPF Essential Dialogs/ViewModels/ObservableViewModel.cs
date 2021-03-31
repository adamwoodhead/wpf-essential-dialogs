using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EssentialDialogs.ViewModels
{
    public abstract class ObservableViewModel : INotifyPropertyChanged
    {
        public ObservableViewModel() { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
    }
}
