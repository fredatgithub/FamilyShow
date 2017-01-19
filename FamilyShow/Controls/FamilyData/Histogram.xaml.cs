using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using System.Collections.Specialized;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : System.Windows.Controls.UserControl
    {
        private ListCollectionView view;
        private Dictionary<object, string> categoryLabels;

        /// <summary>
        /// Get the number of items in the current view.
        /// </summary>
        public int Count
        {
            get { return this.View.Count; }
        }

        public Dictionary<object, string> CategoryLabels
        {
            get { return categoryLabels; }
        }

        #region dependency properties

        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(ListCollectionView), typeof(Histogram),
            new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.AffectsRender,
            new PropertyChangedCallback(ViewProperty_Changed)));

        public ListCollectionView View
        {
            get { return (ListCollectionView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        public static readonly DependencyProperty CategoryFillProperty =
            DependencyProperty.Register("CategoryFill", typeof(Brush), typeof(Histogram),
            new FrameworkPropertyMetadata(Brushes.Transparent,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush CategoryFill
        {
            get { return (Brush)GetValue(CategoryFillProperty); }
            set { SetValue(CategoryFillProperty, value); }
        }

        public static readonly DependencyProperty CategoryStrokeProperty =
            DependencyProperty.Register("CategoryStroke", typeof(Brush), typeof(Histogram),
            new FrameworkPropertyMetadata(null,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush CategoryStroke
        {
            get { return (Brush)GetValue(CategoryStrokeProperty); }
            set { SetValue(CategoryStrokeProperty, value); }
        }

        public static readonly DependencyProperty AxisBrushProperty =
            DependencyProperty.Register("AxisBrush", typeof(Brush), typeof(Histogram),
            new FrameworkPropertyMetadata(SystemColors.WindowTextBrush,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush AxisBrush
        {
            get { return (Brush)GetValue(AxisBrushProperty); }
            set { SetValue(AxisBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(Histogram),
            new FrameworkPropertyMetadata(SystemColors.HighlightBrush,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        public static readonly DependencyProperty DisabledForegroundBrushProperty =
            DependencyProperty.Register("DisabledForegroundBrush", typeof(Brush), typeof(Histogram),
            new FrameworkPropertyMetadata(SystemColors.GrayTextBrush,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush DisabledForegroundBrush
        {
            get { return (Brush)GetValue(DisabledForegroundBrushProperty); }
            set { SetValue(DisabledForegroundBrushProperty, value); }
        }

        #endregion

        #region routed events

        public static readonly RoutedEvent CategorySelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "CategorySelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Histogram));

        public event RoutedEventHandler CategorySelectionChanged
        {
            add { AddHandler(CategorySelectionChangedEvent, value); }
            remove { RemoveHandler(CategorySelectionChangedEvent, value); }
        }

        #endregion

        public Histogram()
        {
            categoryLabels = new Dictionary<object, string>();
            InitializeComponent();
        }

        public string GetCategoryLabel(object columnValue)
        {
            if (this.CategoryLabels.ContainsKey(columnValue))
                return this.CategoryLabels[columnValue];
            return columnValue.ToString();
        }

        private static void ViewProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Histogram histogram = (Histogram)sender;
            ListCollectionView view = (ListCollectionView)args.NewValue;
            histogram.HistogramListBox.ItemsSource = view.Groups;
            histogram.TotalCountLabel.Content = view.Count;
            histogram.view = view;
        }

        private void HistogramListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewGroup selected = (CollectionViewGroup)((ListBox)sender).SelectedItem;
            if (selected != null)
            {
                string categoryLabel = GetCategoryLabel(selected.Name);
                RaiseEvent(new RoutedEventArgs(CategorySelectionChangedEvent, categoryLabel));
            }
        }

        internal void Refresh()
        {
            view.Refresh();

            // Update the total count if items exist in the list view collection. Otherwise, if there
            // are no items, hide the histogram.
            if (view.Count == 0)
            {
                this.LayoutRoot.Visibility = Visibility.Hidden;
            }
            else
            {
                this.LayoutRoot.Visibility = Visibility.Visible;
                this.TotalCountLabel.Content = view.Count;
            }
        }

        internal void ClearSelection()
        {
            HistogramListBox.UnselectAll();
        }
    }

    /// <summary>
    /// Converts a category count to a value between 1 and 100.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class HistogramValueToPercentageConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double count = System.Convert.ToDouble(values[0], CultureInfo.CurrentCulture);
                Histogram histogram = values[1] as Histogram;

                // The count of all groups in the ListCollectionView is used to 'normalize' 
                // the values each category
                double total = System.Convert.ToDouble(histogram.Count);

                if (total <= 0)
                    return 0;
                else
                    return System.Convert.ToDouble((count / total) * 100);
            }
            catch { } 
          
            return 0; 
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Converts a person's age group to a text label that can be used on the histogram. Text is 
    /// retrieved from the resource file for the project.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class HistogramColumnLabelConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object columnValue = values[0];
                Histogram histogram = values[1] as Histogram;
                return histogram.GetCategoryLabel(columnValue);
            }
            catch { }
            return null; 

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}