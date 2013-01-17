using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MangaReader
{
    [Serializable]
    public class MangaState: INotifyPropertyChanged
    {
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            var evt = PropertyChanged;

            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(caller));
            }
        }

        public DirectoryInfo MangaPath { get { return new DirectoryInfo(Path.GetDirectoryName(CurrentPage)); } }

        private string _coverPage = null;

        public String CoverPage
        {
            get
            {
                if (_coverPage == null)
                {
                    MangaDirectoryManager dir = new MangaDirectoryManager(_currentPage);
                    _coverPage = dir.GetPagePath(0);
                }
                return _coverPage;
            }
        }

        private string _currentPage;

        public string CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        public string CurrentPageFilename { get { return Path.GetFileName(_currentPage); } }

        private Rectangle _pageLocation;

        public Rectangle PageLocation
        {
            get { return _pageLocation; }
            set { _pageLocation = value; }
        }

        private MangaConfiguration _configuration;

        public MangaConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        private bool _pinned;

        public bool Pinned
        {
            get { return _pinned; }
            set { _pinned = value; }
        }
        
        public MangaState(string currentPage, Rectangle pageLocation, MangaConfiguration configuration, bool pinned) {
            this.CurrentPage = currentPage;
            this.PageLocation = pageLocation;
            this.Configuration = configuration;
            this.Pinned = pinned;
        }

        private MangaState() { }

    }
}
