using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IPZ.BreathAnalyzer
{
    public class MotherWaveletViewModel : INotifyPropertyChanged
    {
        private string _name;
        private Func<double, int, double> _function;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public Func<double, int, double> Function
        {
            get { return _function; }
            set
            {
                _function = value;
                OnPropertyChanged(nameof(Function));
            }
        }
    }
}
