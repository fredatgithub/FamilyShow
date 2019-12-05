using Microsoft.FamilyShowLib;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for PersonInfo.xaml
  /// </summary>
  public partial class PersonInfo : UserControl
  {
    public PersonInfo()
    {
      InitializeComponent();

      foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
      {
        FontsComboBox.Items.Add(fontFamily.Source);
      }
    }

    public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent("CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PersonInfo));

    // Expose the PersonInfo Close Button click event
    public event RoutedEventHandler CloseButtonClick
    {
      add { AddHandler(CloseButtonClickEvent, value); }
      remove { RemoveHandler(CloseButtonClickEvent, value); }
    }

    #region event handlers

    private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (Visibility == Visibility.Visible)
      {
        // Show the story, hide the editor
        StoryViewBorder.Visibility = Visibility.Visible;
        StoryEditBorder.Visibility = Visibility.Hidden;

        // Load the person story into the viewer
        LoadStoryText(StoryViewer.Document);

        // Display all text in constrast color to the StoryViewer background.
        TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));

        // Hide the photo tags and photo edit buttons if there is no main photo.
        if (DisplayPhoto.Source == null)
        {
          TagsStackPanel.Visibility = Visibility.Hidden;
          PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
        }

        // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
        StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
      }
    }

    /// <summary>
    /// Handler for the CloseButton click event
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      // Raise the CloseButtonClickEvent to notify the container to close this control
      RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
    }

    #region Rich Text event handlers and helper methods

    /// <summary>
    /// Cancels editting a story.
    /// </summary>
    private void CancelStoryButton_Click(object sender, RoutedEventArgs e)
    {
      StoryEditBorder.Visibility = Visibility.Hidden;
      StoryViewBorder.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Switch from the view mode to edit mode
    /// </summary>
    private void EditStoryButton_Click(object sender, RoutedEventArgs e)
    {
      LoadStoryText(StoryRichTextBox.Document);

      StoryEditBorder.Visibility = Visibility.Visible;
      StoryViewBorder.Visibility = Visibility.Hidden;

      StoryRichTextBox.Focus();
    }

    private void SaveStoryButton_Click(object sender, RoutedEventArgs e)
    {
      Person person = (Person)DataContext;

      if (person != null)
      {
        // Pass in a TextRange object to save the story     
        TextRange textRange = new TextRange(StoryRichTextBox.Document.ContentStart, StoryRichTextBox.Document.ContentEnd);
        person.Story = new Story();
        string storyFileName = new StringBuilder(person.Name).Append(".rtf").ToString();
        person.Story.Save(textRange, storyFileName);

        // Display the rich text in the viewer
        LoadStoryText(StoryViewer.Document);

        // Display all text in constrast color to the StoryViewer background.
        TextRange textRange2 = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
        textRange2.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
      }

      // Switch to view mode
      StoryEditBorder.Visibility = Visibility.Hidden;
      StoryViewBorder.Visibility = Visibility.Visible;

      // Workaround to get the StoryViewer to display the first page instead of the last page when first loaded
      StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
      StoryViewer.ViewingMode = FlowDocumentReaderViewingMode.Page;
    }

    private void LoadStoryText(FlowDocument flowDocument)
    {
      // Ignore null cases
      if (flowDocument == null || flowDocument.Blocks == null || DataContext == null)
      {
        return;
      }

      // Clear out any existing text in the viewer 
      flowDocument.Blocks.Clear();

      Person person = (Person)DataContext;

      // Load the story into the story viewer
      if (person != null && person.Story != null)
      {
        TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
        person.Story.Load(textRange);
      }
      else
      {
        // This person doesn't have a story.
        // Load the default story text
        TextRange textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
        textRange.Text = Properties.Resources.DefaultStory;

        textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, Properties.Resources.StoryFontFamily);
        textRange.ApplyPropertyValue(TextElement.FontSizeProperty, Properties.Resources.StoryFontSize);
      }
    }

    private void FontsComboBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
      StoryRichTextBox.Selection.ApplyPropertyValue(FontFamilyProperty, FontsComboBox.SelectedValue);
    }

    void StoryRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
      // Update the toolbar controls based on the current selected text.
      UpdateButtons();
    }

    void StoryRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      // Update the toolbar controls based on the current selected text.
      UpdateButtons();
    }

    /// <summary>
    /// Update the toolbar controls based on the current selected text.
    /// </summary>
    private void UpdateButtons()
    {
      // Need to use object since GetPropertyValue returns different types
      // depending on the text selected.
      object result;

      // Bold button.
      result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontWeightProperty);
      BoldButton.IsChecked = (result != null && result is FontWeight && (FontWeight)result == FontWeights.Bold);

      // Italic button.
      result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontStyleProperty);
      ItalicButton.IsChecked = (result != null && result is FontStyle && (FontStyle)result == FontStyles.Italic);

      // Font list.
      result = StoryRichTextBox.Selection.GetPropertyValue(FlowDocument.FontFamilyProperty);
      if (result != null && result is FontFamily)
      {
        FontsComboBox.SelectedItem = result.ToString();
      }

      // Align buttons.
      result = StoryRichTextBox.Selection.GetPropertyValue(Block.TextAlignmentProperty);
      AlignLeftButton.IsChecked = (result != null && result is TextAlignment && (TextAlignment)result == TextAlignment.Left);
      AlignCenterButton.IsChecked = (result != null && result is TextAlignment && (TextAlignment)result == TextAlignment.Center);
      AlignRightButton.IsChecked = (result != null && result is TextAlignment && (TextAlignment)result == TextAlignment.Right);
      AlignFullButton.IsChecked = (result != null && result is TextAlignment && (TextAlignment)result == TextAlignment.Justify);

      // Underline button.
      result = StoryRichTextBox.Selection.GetPropertyValue(Paragraph.TextDecorationsProperty);
      if (result != null && result is TextDecorationCollection)
      {
        TextDecorationCollection decorations = (TextDecorationCollection)result;
        UnderlineButton.IsChecked = (decorations.Count > 0 && decorations[0].Location == TextDecorationLocation.Underline);
      }
      else
      {
        UnderlineButton.IsChecked = false;
      }

      // bullets
      UpdateBulletButtons();
    }

    /// <summary>
    /// Update the bullet toolbar buttons.
    /// </summary>
    private void UpdateBulletButtons()
    {
      // The bullet information takes a little more work, need
      // to walk the tree and look for a ListItem element.
      TextElement element = StoryRichTextBox.Selection.Start.Parent as TextElement;
      while (element != null)
      {
        if (element is ListItem)
        {
          // Found a bullet item, determine the type of bullet.
          ListItem item = element as ListItem;
          BulletsButton.IsChecked = (item.List.MarkerStyle != TextMarkerStyle.Decimal);
          NumberingButton.IsChecked = (item.List.MarkerStyle == TextMarkerStyle.Decimal);
          return;
        }

        element = element.Parent as TextElement;
      }

      // Did not find a bullet item.
      BulletsButton.IsChecked = false;
      NumberingButton.IsChecked = false;
    }

    #endregion

    #region Photos event handlers and helper methods

    private void PhotosListBox_Drop(object sender, DragEventArgs e)
    {
      Person person = (Person)DataContext;

      // Retrieve the dropped files
      string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

      // Get the files that is supported and add them to the photos for the person
      foreach (string fileName in fileNames)
      {
        // Handles photo files
        if (IsFileSupported(fileName))
        {
          Photo photo = new Photo(fileName);

          // Make the first photo added the person's avatar
          if (person.Photos.Count == 0)
          {
            photo.IsAvatar = true;
            PhotosListBox.SelectedIndex = 0;
          }

          // Associate the photo with the person.
          person.Photos.Add(photo);

          // Setter for property change notification
          person.Avatar = string.Empty;
        }
      }

      // Mark the event as handled, so the control's native Drop handler is not called.
      e.Handled = true;
    }

    private void PhotosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ListBox photosListBox = sender as ListBox;
      if (photosListBox.SelectedIndex != -1)
      {
        // Get the path to the selected photo
        String path = photosListBox.SelectedItem.ToString();

        // Make sure that the file exists
        FileInfo fi = new FileInfo(path);
        if (fi.Exists)
        {
          SetDisplayPhoto(path);
        }

        PhotoButtonsDockPanel.Visibility = Visibility.Visible;
      }
      else
      {
        // Clear the display photo
        DisplayPhoto.Source = new BitmapImage();

        // Hide the photos and tags
        PhotoButtonsDockPanel.Visibility = Visibility.Hidden;
        TagsStackPanel.Visibility = Visibility.Hidden;

        // Clear tags and caption
        TagsListBox.ItemsSource = null;
        CaptionTextBlock.Text = string.Empty;
      }
    }

    /// <summary>
    /// Set the display photo
    /// </summary>
    private void SetDisplayPhoto(String path)
    {
      DisplayPhoto.Source = new BitmapImage(new Uri(path));

      // Make sure the photo supports meta data before retrieving and displaying it
      if (HasMetaData(path))
      {
        // Extract the photo's metadata
        BitmapMetadata metadata = (BitmapMetadata)BitmapFrame.Create(new Uri(path)).Metadata;

        // Display the photo's tags
        TagsStackPanel.Visibility = Visibility.Visible;
        TagsListBox.ItemsSource = metadata.Keywords;

        // Display the photo's comment
        CaptionTextBlock.Text = metadata.Title;
      }
      else
      {
        // Clear tags and caption
        TagsStackPanel.Visibility = Visibility.Hidden;
        TagsListBox.ItemsSource = null;
        CaptionTextBlock.Text = string.Empty;
      }
    }

    private void SetPrimaryButton_Click(object sender, RoutedEventArgs e)
    {
      Person person = (Person)DataContext;

      if (person.Photos != null && PhotosListBox.SelectedItem != null)
      {
        // Set IsAvatar to false for existing photos
        foreach (Photo existingPhoto in person.Photos)
        {
          existingPhoto.IsAvatar = false;
        }

        Photo photo = (Photo)PhotosListBox.SelectedItem;
        photo.IsAvatar = true;
        person.Avatar = photo.FullyQualifiedPath;
      }
    }

    private void RemovePhotoButton_Click(object sender, RoutedEventArgs e)
    {
      Person person = (Person)DataContext;

      Photo photo = (Photo)PhotosListBox.SelectedItem;
      if (photo != null)
      {
        person.Photos.Remove(photo);

        // Removed photo is an avatar, set a different avatar photo
        if (photo.IsAvatar && person.Photos.Count > 0)
        {
          person.Photos[0].IsAvatar = true;
          person.Avatar = person.Photos[0].FullyQualifiedPath;
        }
        else
          // Setter for property change notification
          person.Avatar = "";
      }
    }

    /// <summary>
    /// Only allow the most common photo formats (JPEG, PNG, and GIF)
    /// </summary>
    private static bool IsFileSupported(string fileName)
    {
      string extension = Path.GetExtension(fileName);

      if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
          string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0 ||
          string.Compare(extension, ".png", true, CultureInfo.InvariantCulture) == 0 ||
          string.Compare(extension, ".gif", true, CultureInfo.InvariantCulture) == 0)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Only JPEG photos support metadata.
    /// </summary>
    private static bool HasMetaData(string fileName)
    {
      string extension = Path.GetExtension(fileName);

      if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 || string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0)
      {
        return true;
      }

      return false;
    }

    #endregion

    #endregion

    #region helper methods

    public void SetDefaultFocus()
    {
      CloseButton.Focus();
    }

    /// <summary>
    /// The details of a person changed.
    /// </summary>
    public void OnSkinChanged()
    {
      // Display all text in constrast color to the StoryViewer background.
      TextRange textRange = new TextRange(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
      textRange.Select(StoryViewer.Document.ContentStart, StoryViewer.Document.ContentEnd);
      textRange.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("FlowDocumentFontColor"));
    }

    #endregion
  }
}