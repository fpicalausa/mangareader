using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ookii.Dialogs.Wpf;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double animationTime = 0.3;

        private enum ViewKind
        {
            MangaView,
            OptionView,
            StartupView,
        }

        private Manga book = null;
        private MangaPage currentMangaPage;

        private Rect? currentTarget = null;
        private System.Drawing.Rectangle? currentPageTarget = null;

        private bool placeholderBefore = false;

        private bool disableAnimation = false;

        private ViewKind view = ViewKind.StartupView;

        Storyboard pagesAnimation = null;

        private IViewHandler viewHandler = new NullViewHandler();

        private DispatcherTimer _resizeEnding = new DispatcherTimer
        {
            Interval = new TimeSpan(0,0,0,0,500),
            IsEnabled = false
        };

        public MainWindow()
        {
            InitializeComponent();
            showView(ViewKind.StartupView);

            _resizeEnding.Tick += resizeEnding_Tick;
        }

        private void resizeEnding_Tick(object sender, EventArgs e)
        {
            _resizeEnding.Stop();
            updateView(true);
        }


        private void showView(ViewKind kind)
        {
            view = kind;
            MangaView.Visibility = (kind == ViewKind.MangaView || kind == ViewKind.OptionView).toVisibility();
            OptionsPage.Visibility = (kind == ViewKind.OptionView).toVisibility();
            StartScreen.Visibility = (kind == ViewKind.StartupView).toVisibility();
            btnOptions.IsEnabled = kind == ViewKind.MangaView;
            btnBack.IsEnabled = kind == ViewKind.MangaView;
            btnTranslate.Visibility = (book != null && book.HasTranslation).toVisibility();
            btnTranslate.IsEnabled = (kind == ViewKind.MangaView && book != null && book.HasTranslation && currentMangaPage.HasTranslation);
        }

        private double GetPageSizeFactor(double width, double height)
        {
            double resize_x_factor = width / PageContainer.ActualWidth;
            double resize_y_factor = height / PageContainer.ActualHeight;
            return Math.Max(Math.Max(resize_x_factor, resize_y_factor), 1.0);
        }

        private static Rect MoveToLeftOf(Image target, double resize_factor, Rect mainPage, Storyboard story)
        {
            if (target.Source == null) return Rect.Empty;
            double actualWidth = target.Source.Width / resize_factor;
            double actualHeight = target.Source.Height / resize_factor;

            Rect rect = new Rect(
                mainPage.Left - actualWidth - 10,
                mainPage.Top - (actualHeight - mainPage.Height) / 2,
                actualWidth,
                actualHeight);

            target.MoveToLTW(rect, story, animationTime, story != null);

            return rect;
        }

        private static Rect MoveToRightOf(Image target, double resize_factor, Rect mainPage, Storyboard story)
        {
            if (target.Source == null) return Rect.Empty;
            double actualWidth = target.Source.Width / resize_factor;
            double actualHeight = target.Source.Height / resize_factor;

            Rect rect = new Rect(
                mainPage.Right + 10,
                mainPage.Top - (actualHeight - mainPage.Height) / 2,
                actualWidth,
                actualHeight);

            target.MoveToLTW(rect, story, animationTime, story != null);

            return rect;
        }

        private bool isAfterRight()
        {
            switch (book.Configuration.ReadingDirection)
            {
                case MangaParser.Reader.Page.ReadingDirection.LeftDown:
                case MangaParser.Reader.Page.ReadingDirection.DownLeft:
                    return false;
                default:
                    return true;
            }
        }

        private Rect MoveAfter(Image target, double resize_factor, Rect mainPage, Storyboard story)
        {
            return isAfterRight() ? 
                MoveToRightOf(target, resize_factor, mainPage, story) :
                MoveToLeftOf(target, resize_factor, mainPage, story);
        }

        private Rect MoveBefore(Image target, double resize_factor, Rect mainPage, Storyboard story)
        {
            return isAfterRight() ? 
                MoveToLeftOf(target, resize_factor, mainPage, story) :
                MoveToRightOf(target, resize_factor, mainPage, story);
        }

        private void StopAnimatePages()
        {
            if (pagesAnimation == null) return;
            pagesAnimation.Stop();
            pagesAnimation.Remove();
        }

        private void StartAnimatePage()
        {
            if (this.pagesAnimation != null)
            {
                pagesAnimation.Freeze();
                pagesAnimation.Begin(this);
            }
        }

        private void MoveIntoView(Rect target, bool withAnimation = false)
        {
            double resize_factor = GetPageSizeFactor(target.Width, target.Height);
            Rect targetRect = new Rect(
                -(target.Left + target.Width / 2) / resize_factor + PageContainer.ActualWidth / 2,
                -(target.Top + target.Height / 2) / resize_factor + PageContainer.ActualHeight / 2,
                CurrentPage.Source.Width / resize_factor,
                CurrentPage.Source.Height / resize_factor);

            currentTarget = target;

            StopAnimatePages();
            this.pagesAnimation = withAnimation ? new Storyboard(): null;
            CurrentPage.MoveToLTW(targetRect, pagesAnimation, animationTime, withAnimation);
            var previousTarget = MoveBefore(PreviousPage, resize_factor, targetRect, pagesAnimation);
            var nextTarget = MoveAfter(NextPage, resize_factor, targetRect, pagesAnimation);

            if (PlaceholderPage.Source != null) {
                if (placeholderBefore)
                {
                    MoveBefore(PlaceholderPage, resize_factor, previousTarget, pagesAnimation);
                }
                else
                {
                    MoveAfter(PlaceholderPage, resize_factor, nextTarget, pagesAnimation);
                }
            }

            double maskWidth = Math.Max(0.0, (PageContainer.ActualWidth - target.Width / resize_factor) / 2 - 10),
                   maskHeight = Math.Max(0.0, (PageContainer.ActualHeight - target.Height / resize_factor) / 2 - 10);

            maskLeft.SizeW(maskWidth, pagesAnimation, animationTime, withAnimation);
            maskRight.SizeW(maskWidth, pagesAnimation, animationTime, withAnimation);
            maskTop.SizeH(maskHeight, pagesAnimation, animationTime, withAnimation);
            maskBottom.SizeH(maskHeight, pagesAnimation, animationTime, withAnimation);

            /*
            DebugRect.MoveTo(new Rect(
                targetRect.Left + target.Left / resize_factor,
                targetRect.Top + target.Top / resize_factor,
                target.Width / resize_factor,
                target.Height / resize_factor));
            */
            StartAnimatePage();
        }

        private void MoveIntoView(System.Drawing.Rectangle target, bool withAnimation = false)
        {
            BitmapSource src = (BitmapSource)CurrentPage.Source;
            var scale = src.PixelWidth / src.Width;

            if (scale != 1.0)
            {
                //Scale the targets to correspond to image coordinates
                MoveIntoView(
                          new Rect((double)(target.X / scale), (int)(target.Y / scale),
                                  (double)(target.Width / scale), (int)(target.Height / scale)),
                          withAnimation);
            }
            else {
                MoveIntoView(target.toRect(), withAnimation);
            }
        }

        private void MoveIntoView(System.Drawing.Rectangle target)
        {
            MoveIntoView(target, !disableAnimation);
        }

        private static void LoadPage(MangaPage page, Image target, bool visible = true)
        {
            if (visible)
            {
                target.Source = page.Source;
                target.Source.Freeze();
            }
            target.Visibility = visible.toVisibility();
        }

        private Image PreviousPage
        {
            get
            {
                switch (book.Configuration.ReadingDirection)
                {
                    case MangaParser.Reader.Page.ReadingDirection.LeftDown:
                    case MangaParser.Reader.Page.ReadingDirection.DownLeft:
                        return RightPage;
                    default:
                        return LeftPage;
                }
            }
        }

        private Image NextPage
        {
            get
            {
                switch (book.Configuration.ReadingDirection)
                {
                    case MangaParser.Reader.Page.ReadingDirection.LeftDown:
                    case MangaParser.Reader.Page.ReadingDirection.DownLeft:
                        return LeftPage;
                    default:
                        return RightPage;
                }
            }
        }

        private void LoadPages()
        {
            LoadPage(this.currentMangaPage.Previous, PreviousPage, this.currentMangaPage.HasPrevious);
            LoadPage(this.currentMangaPage, CurrentPage);
            LoadPage(this.currentMangaPage.Next, NextPage, this.currentMangaPage.HasNext);
        }

        private void unbindViewHandler()
        {
            this.viewHandler.NextPageDisplay -= viewHandler_NextPageDisplay;
            this.viewHandler.PreviousPageDisplay -= viewHandler_PreviousPageDisplay;
            this.viewHandler.Display -= viewHandler_Display;
        }

        private void viewHandler_Display(object sender, DisplayEventArgs e)
        {
            MoveIntoView(e.Rectangle);
        }

        private void viewHandler_PreviousPageDisplay(object sender, DisplayEventArgs e)
        {
            ShowPreviousPage(e.Page);
            MoveIntoView(e.Rectangle);
        }

        private void viewHandler_NextPageDisplay(object sender, DisplayEventArgs e)
        {
            ShowNextPage(e.Page);
            MoveIntoView(e.Rectangle);
        }

        private void bindViewHandler(IViewHandler handler) {
            unbindViewHandler();

            this.viewHandler = handler;
            this.viewHandler.NextPageDisplay += viewHandler_NextPageDisplay;
            this.viewHandler.PreviousPageDisplay += viewHandler_PreviousPageDisplay;
            this.viewHandler.Display += viewHandler_Display;
        }

        private IViewHandler createViewHandler()
        {
            switch (book.Configuration.ViewMode)
            {
                case MangaConfiguration.ViewModeKind.Free:
                    return (new FreeviewHandler(book, currentMangaPage));
                case MangaConfiguration.ViewModeKind.FullPage:
                    return (new FullPageViewHandler(currentMangaPage));
                default:
                    return (new ParsedViewHandler(currentMangaPage));
            }
        }

        private void applyOptions()
        {
            // We need to ensure that the bitmaps are loaded according to the reading order;
            LoadPages();

            // The view is handled according to the user preference
            IViewHandler newHandler = createViewHandler();
            bindViewHandler(newHandler);

            // Then we update the display according to the settings
            updateView();
        }

        private void LoadManga(string BaseFile)
        {
            book = new Manga(BaseFile);
            SetCurrentPage(book.GetPage(BaseFile));

            applyOptions();

            showView(ViewKind.OptionView);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeEnding.Stop();
            // Quick resize using the same target
            if (currentTarget.HasValue) {
                MoveIntoView(currentTarget.Value, false);
            }

            // We will check later if we need to recompute the view.
            _resizeEnding.Start();
        }

        private void ConfirmOptions_Click(object sender, RoutedEventArgs e)
        {
            updateOptions();
            applyOptions();

            showView(ViewKind.MangaView);
        }

        private void updateOptions()
        {
            if (this.readLRTB.IsChecked.Value == true) book.Configuration.ReadingDirection = MangaParser.Reader.Page.ReadingDirection.DownRight;
            if (this.readRLTB.IsChecked.Value == true) book.Configuration.ReadingDirection = MangaParser.Reader.Page.ReadingDirection.DownLeft;
            if (this.readTBRL.IsChecked.Value == true) book.Configuration.ReadingDirection = MangaParser.Reader.Page.ReadingDirection.LeftDown;

            if (this.viewCells.IsChecked == true) book.Configuration.ViewMode = MangaConfiguration.ViewModeKind.Cell;
            if (this.viewStrips.IsChecked == true) book.Configuration.ViewMode = MangaConfiguration.ViewModeKind.Strip;
            if (this.viewFree.IsChecked == true) book.Configuration.ViewMode = MangaConfiguration.ViewModeKind.Free;

            book.Configuration.Translation = this.txtAltLang.Text;
        }

        private void updateView(bool updateTargets = true)
        {
            if (book == null) return;

            book.ViewHeight = (int)(PageContainer.ActualHeight);
            book.ViewWidth = (int)(PageContainer.ActualWidth);
            viewHandler.Refresh(updateTargets ? currentPageTarget : null);
        }

        private double GetSizeFactor()
        {
            double size_factor = 1.0;
            if (currentTarget.HasValue)
                size_factor = GetPageSizeFactor(currentTarget.Value.Width, currentTarget.Value.Height);

            return size_factor;
        }

        private static void TransferPage(Image destination, Image source)
        {
            destination.Source = source.Source;
            destination.MoveToLTW(source.Rectangle());
        }

        private static void ShiftPages(params Image[] images)
        {
            for (int i = 0; i < images.Length - 1; i++)
            {
                TransferPage(images[i], images[i + 1]);
            }
        }

        private void SetCurrentPage(MangaPage page)
        {
            currentMangaPage = page;
            btnTranslate.IsEnabled = page.HasTranslation;
        }

        private void ShowNextPage(MangaPage newPage)
        {
            MangaPage Next = newPage;
            MangaPage NextNext = Next.Next;
            double size_factor = GetSizeFactor();

            StopAnimatePages();

            ShiftPages(PlaceholderPage, PreviousPage, CurrentPage, NextPage);

            LoadPage(NextNext, NextPage, Next.HasNext);
            MoveAfter(NextPage, size_factor, CurrentPage.Rectangle(), null);
            this.placeholderBefore = true;

            SetCurrentPage(Next);
        }

        private void ShowPreviousPage(MangaPage newPage)
        {
            MangaPage Prev = newPage;
            MangaPage PrevPrev = Prev.Previous;
            double size_factor = GetSizeFactor();

            StopAnimatePages();

            ShiftPages(PlaceholderPage, NextPage, CurrentPage, PreviousPage);
            LoadPage(PrevPrev, PreviousPage, Prev.HasPrevious);
            MoveBefore(PreviousPage, size_factor, CurrentPage.Rectangle(), null);
            this.placeholderBefore = false;

            SetCurrentPage(Prev);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (view != ViewKind.MangaView) return;

            e.Handled = true;
            switch (e.Key)
            {
                case Key.R:
                    viewHandler.Refresh(null);
                    break;
                case Key.Right:
                    viewHandler.Next();
                    break;
                case Key.Left:
                    viewHandler.Previous();
                    break;
                default:
                    e.Handled = false;
                    break;
            }

            if (e.Handled) CurrentPage.Focus();
        }

        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            showView(ViewKind.OptionView);
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Manga Page"; // Default file name
            dlg.Filter = "Image files|*.png;*.jpg;*.bmp"; // Filter files by extension
            bool? result = dlg.ShowDialog(this);

            if (result.HasValue && result.Value)
            {
                LoadManga(dlg.FileName);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            showView(ViewKind.StartupView);
        }

        private void CancelOptions_Click(object sender, RoutedEventArgs e)
        {
            this.readLRTB.IsChecked = book.Configuration.ReadingDirection == MangaParser.Reader.Page.ReadingDirection.DownRight;
            this.readRLTB.IsChecked = book.Configuration.ReadingDirection == MangaParser.Reader.Page.ReadingDirection.DownLeft;
            this.readTBRL.IsChecked = book.Configuration.ReadingDirection == MangaParser.Reader.Page.ReadingDirection.LeftDown;

            this.viewCells.IsChecked =  book.Configuration.ViewMode == MangaConfiguration.ViewModeKind.Cell;
            this.viewStrips.IsChecked = book.Configuration.ViewMode == MangaConfiguration.ViewModeKind.Strip;
            this.viewFree.IsChecked =   book.Configuration.ViewMode == MangaConfiguration.ViewModeKind.Free;

            this.txtAltLang.Text = book.Configuration.Translation;

            showView(ViewKind.MangaView);
        }

        private void btnTranslationSelection_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browseForFolder = new VistaFolderBrowserDialog();
            var response = browseForFolder.ShowDialog();

            if (!response.HasValue || !response.Value)
            {
                txtAltLang.Text = "";
            }
            else
            {
                txtAltLang.Text = browseForFolder.SelectedPath;
            }
        }

        private void btnTranslate_Checked(object sender, RoutedEventArgs e)
        {
            CurrentPage.Source = currentMangaPage.TranslationSource;
        }

        private void btnTranslate_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentPage.Source = currentMangaPage.Source;
        }
    }
}
