using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MangaReader
{
    static class WPFUtil
    {
        /// <summary>
        /// Converts a System.Drawing.Rectangle into a System.Windows.Rect
        /// </summary>
        public static Rect toRect(this System.Drawing.Rectangle rectangle)
        {
            return new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Computes the center point of a System.Drawing.Rectangle
        /// </summary>
        public static Point center(this System.Drawing.Rectangle rectangle)
        {
            return new Point(
                rectangle.X + rectangle.Width / 2,
                rectangle.Y + rectangle.Height / 2);
        }

        /// <summary>
        /// Determines the square of the distance between two points
        /// </summary>
        /// <remarks>
        /// Since the square root is a monotonous function, this function can be 
        /// used when comparing the distance between points.
        /// </remarks>
        public static double SqDist(this Point p1, Point p2)
        {
            return (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
        }

        /// <summary>
        /// Move the specified Canvas-contained WPF element to the location
        /// at the specified rectangle. The width (but not the height) of the 
        /// element will also be set.
        /// </summary>
        /// <param name="element">The element to be moved</param>
        /// <param name="rectangle">The location to which the elemnt is to be moved</param>
        public static void MoveToLTW(this FrameworkElement element, Rect rectangle)
        {
            element.BeginAnimation(Canvas.LeftProperty, null);
            element.BeginAnimation(Canvas.TopProperty, null);

            Canvas.SetLeft(element, rectangle.Left);
            Canvas.SetTop(element, rectangle.Top);

            element.SizeW(rectangle.Width);
        }

        /// <summary>
        /// Get the location of an element positionned in a canvas as a Rect.
        /// </summary>
        /// <param name="element">The element whose location is to be determined</param>
        /// <returns>The Rect that delimits the element</returns>
        public static Rect Rectangle(this FrameworkElement element)
        {
            return new Rect( Canvas.GetLeft(element), Canvas.GetTop(element), element.ActualWidth, element.ActualHeight);
        }

        /// <summary>
        /// Move the specified Canvas-contained WPF element to the location
        /// at the specified rectangle, animating the move. The width 
        /// (but not the height) of the element will also be set.
        /// </summary>
        /// <param name="element">The element to be moved</param>
        /// <param name="rectangle">The location to which the elemnt is to be moved</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        public static void MoveToLTW(this FrameworkElement element, Rect rectangle, Storyboard story, double animationSeconds)
        {
            DoubleAnimation animTop = new DoubleAnimation(rectangle.Top, TimeSpan.FromSeconds(animationSeconds));
            story.Children.Add(animTop);
            Storyboard.SetTarget(animTop, element);
            Storyboard.SetTargetProperty(animTop, new PropertyPath(Canvas.TopProperty));

            DoubleAnimation animLeft = new DoubleAnimation(rectangle.Left, TimeSpan.FromSeconds(animationSeconds));
            story.Children.Add(animLeft);
            Storyboard.SetTarget(animLeft, element);
            Storyboard.SetTargetProperty(animLeft, new PropertyPath(Canvas.LeftProperty));

            element.SizeW(rectangle.Width, story, animationSeconds);
        }

        /// <summary>
        /// Move the specified Canvas-contained WPF element to the location
        /// at the specified rectangle, possibly animating the move. The width 
        /// (but not the height) of the element will also be set.
        /// </summary>
        /// <param name="element">The element to be moved</param>
        /// <param name="rectangle">The location to which the elemnt is to be moved</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        /// <param name="animated">Whether an animation must take place</param>
        public static void MoveToLTW(this FrameworkElement element, Rect rectangle, Storyboard story, double animationSeconds = 0.0, bool animated = false)
        {
            // If some starting position is NaN, we cannot animate. Fall back on default
            if (double.IsNaN(Canvas.GetTop(element)) || double.IsNaN(Canvas.GetLeft(element)) || double.IsNaN(element.Width))
            {
                animated = false;
            }

            if (animated == false || story == null) element.MoveToLTW(rectangle);
            else element.MoveToLTW(rectangle, story, animationSeconds);
        }

        /// <summary>
        /// Set the width of the specified element.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="width">The new width of the element</param>
        public static void SizeW(this FrameworkElement element, double width)
        {
            element.BeginAnimation(Canvas.WidthProperty, null);
            element.Width = width;
        }

        /// <summary>
        /// Set the width of the specified element, in an animation.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="width">The new width of the element</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        public static void SizeW(this FrameworkElement element, double width, Storyboard story, double animationSeconds = 0.0)
        {
            DoubleAnimation animWidth = new DoubleAnimation(width, TimeSpan.FromSeconds(animationSeconds));
            story.Children.Add(animWidth);
            Storyboard.SetTarget(animWidth, element);
            Storyboard.SetTargetProperty(animWidth, new PropertyPath(Canvas.WidthProperty));
        }

        /// <summary>
        /// Set the width of the specified element, possibly in an animation.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="width">The new width of the element</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        /// <param name="animated">Whether an animation must take place</param>
        public static void SizeW(this FrameworkElement element, double width, Storyboard story, double animationSeconds = 0.0, bool animated = false)
        {
            if (double.IsNaN(element.Width)) animated = false;
            if (animated == false || story == null) element.SizeW(width);
            else element.SizeW(width, story, animationSeconds);
        }

        /// <summary>
        /// Set the height of the specified element.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="height">The new height of the element</param>
        public static void SizeH(this FrameworkElement element, double height)
        {
            element.BeginAnimation(Canvas.WidthProperty, null);
            element.Height = height;
        }

        /// <summary>
        /// Set the height of the specified element, in an animation.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="height">The new height of the element</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        public static void SizeH(this FrameworkElement element, double height, Storyboard story, double animationSeconds = 0.0)
        {
            DoubleAnimation animHeight = new DoubleAnimation(height, TimeSpan.FromSeconds(animationSeconds));
            story.Children.Add(animHeight);
            Storyboard.SetTarget(animHeight, element);
            Storyboard.SetTargetProperty(animHeight, new PropertyPath(Canvas.HeightProperty));
        }

        /// <summary>
        /// Set the height of the specified element, possibly in an animation.
        /// </summary>
        /// <param name="element">The element to be resized</param>
        /// <param name="height">The new height of the element</param>
        /// <param name="story">The Storyboard that will contain the movement animation</param>
        /// <param name="animationSeconds">The number of second the animation will last</param>
        /// <param name="animated">Whether an animation must take place</param>
        public static void SizeH(this FrameworkElement element, double height, Storyboard story, double animationSeconds = 0.0, bool animated = false)
        {
            if (double.IsNaN(element.Height)) animated = false;
            if (animated == false || story == null) element.SizeH(height);
            else element.SizeH(height, story, animationSeconds);
        }

        /// <summary>
        /// Compute a WPF visibility of Hidden or Visible, depending on
        /// a boolean value.
        /// </summary>
        public static Visibility toVisibility(this bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
