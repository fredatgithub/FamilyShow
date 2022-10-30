using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for TagCloud.xaml
  /// </summary>
  public partial class TagCloud : UserControl
  {
    private ListCollectionView view;

    #region dependency properties

    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(ListCollectionView), typeof(TagCloud), new FrameworkPropertyMetadata(new PropertyChangedCallback(ViewProperty_Changed)));

    public ListCollectionView View
    {
      get { return (ListCollectionView)GetValue(ViewProperty); }
      set { SetValue(ViewProperty, value); }
    }

    public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(TagCloud), new FrameworkPropertyMetadata(SystemColors.HighlightBrush, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush SelectedBrush
    {
      get { return (Brush)GetValue(SelectedBrushProperty); }
      set { SetValue(SelectedBrushProperty, value); }
    }

    public static readonly DependencyProperty DisabledForegroundBrushProperty = DependencyProperty.Register("DisabledForegroundBrush", typeof(Brush), typeof(TagCloud), new FrameworkPropertyMetadata(SystemColors.GrayTextBrush, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush DisabledForegroundBrush
    {
      get { return (Brush)GetValue(DisabledForegroundBrushProperty); }
      set { SetValue(DisabledForegroundBrushProperty, value); }
    }

    public static readonly DependencyProperty ListBackgroundBrushProperty = DependencyProperty.Register("ListBackgroundBrush", typeof(Brush), typeof(TagCloud), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush ListBackgroundBrush
    {
      get { return (Brush)GetValue(ListBackgroundBrushProperty); }
      set { SetValue(ListBackgroundBrushProperty, value); }
    }

    public static readonly DependencyProperty ListBorderBrushProperty = DependencyProperty.Register("ListBorderBrush", typeof(Brush), typeof(TagCloud), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush ListBorderBrush
    {
      get { return (Brush)GetValue(ListBorderBrushProperty); }
      set { SetValue(ListBorderBrushProperty, value); }
    }

    public static readonly DependencyProperty TagMinimumSizeProperty = DependencyProperty.Register("TagMinimumSize", typeof(double), typeof(TagCloud), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double TagMinimumSize
    {
      get { return (double)GetValue(TagMinimumSizeProperty); }
      set { SetValue(TagMinimumSizeProperty, value); }
    }

    public static readonly DependencyProperty TagMaximumSizeProperty = DependencyProperty.Register("TagMaximumSize", typeof(double), typeof(TagCloud), new FrameworkPropertyMetadata(38.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double TagMaximumSize
    {
      get { return (double)GetValue(TagMaximumSizeProperty); }
      set { SetValue(TagMaximumSizeProperty, value); }
    }

    public static readonly DependencyProperty TagIncrementSizeProperty = DependencyProperty.Register("TagIncrementSize", typeof(double), typeof(TagCloud), new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double TagIncrementSize
    {
      get { return (double)GetValue(TagIncrementSizeProperty); }
      set { SetValue(TagIncrementSizeProperty, value); }
    }

    #endregion

    #region routed events

    public static readonly RoutedEvent TagSelectionChangedEvent = EventManager.RegisterRoutedEvent("TagSelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagCloud));

    public event RoutedEventHandler TagSelectionChanged
    {
      add { AddHandler(TagSelectionChangedEvent, value); }
      remove { RemoveHandler(TagSelectionChangedEvent, value); }
    }

    #endregion

    public TagCloud()
    {
      InitializeComponent();
    }

    private static void ViewProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
      ListCollectionView view = (ListCollectionView)args.NewValue;
      TagCloud tagCloud = ((TagCloud)sender);
      tagCloud.TagCloudListBox.ItemsSource = view.Groups;
      tagCloud.view = view;
    }

    private void TagCloudListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      CollectionViewGroup selected = (CollectionViewGroup)((ListBox)sender).SelectedItem;

      if (selected != null)
      {
        RaiseEvent(new RoutedEventArgs(TagSelectionChangedEvent, selected.Name));
      }
    }

    internal void Refresh()
    {
      view.Refresh();
    }

    internal void ClearSelection()
    {
      TagCloudListBox.UnselectAll();
    }
  }

  [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
  class CountToFontSizeConverter : IMultiValueConverter
  {
    #region IMultiValueConverter Members

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      int count = (int)values[0];
      TagCloud tagCloud = values[1] as TagCloud;

      return ((tagCloud.TagMinimumSize + count + tagCloud.TagIncrementSize) < tagCloud.TagMaximumSize) ? (tagCloud.TagMinimumSize + count + tagCloud.TagIncrementSize) : tagCloud.TagMaximumSize;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      // not implemented yet
      return new object[0];
    }

    #endregion
  }
}