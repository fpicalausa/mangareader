using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Reader;

namespace MangaReader
{
    public class MangaConfiguration: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public enum ViewModeKind
        {
            Strip,
            Cell,
            Free,
            FullPage,
        }

        private ViewModeKind _viewMode = ViewModeKind.FullPage;
        private Page.ReadingDirection _readingDirection = Page.ReadingDirection.DownRight;
        private String _translation = null;

        public ViewModeKind ViewMode { 
            get { return _viewMode; } 
            set { _viewMode = value; NotifyPropertyChanged(); } }
        
        public Page.ReadingDirection ReadingDirection { 
            get { return _readingDirection; }  
            set { _readingDirection = value; NotifyPropertyChanged(); } }

        public string Translation {
            get { return _translation; }
            set { _translation = value; NotifyPropertyChanged(); } }
    }
}
