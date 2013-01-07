using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Reader;

namespace MangaReader
{
    /// <summary>
    /// Defines a PageViewer that adapts to the parameters set in a 
    /// MangaConfiguration.
    /// </summary>
    public class MangaPageViewer: INotifyPropertyChanged, IPageViewer
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IPageViewer _viewer = null;
        private Matrix _viewTransformation;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            _viewer = null;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private IPageViewer getViewer()
        {
            if (_viewer == null) { 

                switch (_viewMode)
                {
                    case MangaConfiguration.ViewModeKind.Strip:
                        _viewer = new StripPageViewer(_width, _height);
                        break;
                    case MangaConfiguration.ViewModeKind.Cell:
                        _viewer = new CellsPageViewer(_width, _height);
                        break;
                    case MangaConfiguration.ViewModeKind.Free:
                        _viewer = new FullPageViewer();
                        break;
                    default:
                        _viewer = null;
                        break;
                }

                if (_viewer != null) _viewer.ViewTransformation = _viewTransformation;
            }

            return _viewer;
        }

        private MangaConfiguration.ViewModeKind _viewMode;

        public MangaConfiguration.ViewModeKind ViewMode
        {
            get { return _viewMode; }
            set { _viewMode = value; this.NotifyPropertyChanged(); }
        }

        private int _width;

        /// <summary>
        /// An hint of the maximum width expected for a view.
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; NotifyPropertyChanged(); }
        }

        private int _height;

        /// <summary>
        /// An hint of the maximum height expected for a view.
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// Constructs a page viewer that adapts to the specified MangaConfiguration
        /// </summary>
        /// <param name="configuration">Parameters indicating how the manga is to be read.</param>
        public MangaPageViewer(MangaConfiguration configuration)
        {
            configuration.PropertyChanged+=configuration_PropertyChanged;
            _viewTransformation = new Matrix();
        }

        private void configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ViewMode")
            {
                ViewMode = (sender as MangaConfiguration).ViewMode;
            }
        }

        public IEnumerable<System.Drawing.Rectangle> ComputeView(IEnumerable<MangaParser.Graphics.IPolygon> polygons)
        {
            return getViewer().ComputeView(polygons);
        }

        public System.Drawing.Drawing2D.Matrix ViewTransformation
        { 
            get { return _viewTransformation; } 
            set { _viewTransformation = value; 
                if (_viewer != null) 
                    _viewer.ViewTransformation = value; } }
    }
}
