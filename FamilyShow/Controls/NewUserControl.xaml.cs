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
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for NewUserControl.xaml
    /// </summary>

    public partial class NewUserControl : System.Windows.Controls.UserControl
    {
        #region fields

        // The list of people, this is a global list shared by the application.
        PeopleCollection family = App.Family;

        string avatarPhotoPath;

        #endregion

        public NewUserControl()
        {
            InitializeComponent();

            SetDefaultFocus();
        }

        #region routed events

        public static readonly RoutedEvent AddButtonClickEvent = EventManager.RegisterRoutedEvent(
            "AddButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NewUserControl));

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NewUserControl));

        // Expose this event for this control's container
        public event RoutedEventHandler AddButtonClick
        {
            add { AddHandler(AddButtonClickEvent, value); }
            remove { RemoveHandler(AddButtonClickEvent, value); }
        }

        // Expose this event for this control's container
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region event handlers

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new person with the specified inputs
            Person newPerson = new Person(FirstNameInputTextBox.Text, LastNameInputTextBox.Text);

            // Setup the properties based on the input
            newPerson.Gender = ((bool)MaleRadioButton.IsChecked) ? Gender.Male : Gender.Female;
            newPerson.BirthPlace = BirthPlaceInputTextBox.Text;
            newPerson.IsLiving = true;

            DateTime birthdate = App.StringToDate(BirthDateInputTextBox.Text);
            if (birthdate != DateTime.MinValue)
                newPerson.BirthDate = birthdate;

            // Setup the avatar photo
            if (!string.IsNullOrEmpty(avatarPhotoPath))
            {
                Photo photo = new Photo(avatarPhotoPath);
                photo.IsAvatar = true;

                // Add the avatar photo to the person photos
                newPerson.Photos.Add(photo);
            }

            family.Current = newPerson;
            family.Add(newPerson);

            family.OnContentChanged();

            RaiseEvent(new RoutedEventArgs(AddButtonClickEvent));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new unknown person
            Person newPerson = new Person();

            family.Current = newPerson;
            family.Add(newPerson);

            family.OnContentChanged();
            
            family.IsDirty = false;

            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        /// <summary>
        /// Handles Drop Event for Avatar photo.
        /// </summary>
        private void AvatarPhoto_Drop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            if (fileNames.Length > 0)
            {
                avatarPhotoPath = fileNames[0];
                AvatarPhoto.Source = new BitmapImage(new Uri(avatarPhotoPath));
            }

            // Mark the event as handled, so the control's native Drop handler is not called.
            e.Handled = true;
        }

        /// <summary>
        /// Handles selecting the avatar photo
        /// </summary>
        private void AvatarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CommonDialog dialog = new CommonDialog();
            dialog.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
            dialog.Filter.Add(new FilterEntry(Properties.Resources.JpegFiles, Properties.Resources.JpegExtension));
            dialog.Filter.Add(new FilterEntry(Properties.Resources.PngFiles, Properties.Resources.PngExtension));
            dialog.Title = Properties.Resources.Open;
            dialog.ShowOpen();

            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                avatarPhotoPath = dialog.FileName;
                AvatarPhoto.Source = new BitmapImage(new Uri(avatarPhotoPath));
            }
        }

        #endregion

        #region helper methods

        public void SetDefaultFocus()
        {
            // Set the focus to the first name textbox for quick entry
            FirstNameInputTextBox.Focus();
        }

        /// <summary>
        /// Clear the input fields
        /// </summary>
        public void ClearInputFields()
        {
            FirstNameInputTextBox.Clear();
            LastNameInputTextBox.Clear();
            BirthDateInputTextBox.Clear();
            BirthPlaceInputTextBox.Clear();
            MaleRadioButton.IsChecked = true;
            AvatarPhoto.Source = null;
            avatarPhotoPath = string.Empty;
        }

        #endregion
    }
}