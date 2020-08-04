using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : UserControl
  {
        public Welcome()
        {
            InitializeComponent();
            CreateRecentFiles();
            DisplayVersion();
        }

        #region routed events

        public static readonly RoutedEvent NewButtonClickEvent = EventManager.RegisterRoutedEvent(
            "NewButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Welcome));

        public static readonly RoutedEvent OpenButtonClickEvent = EventManager.RegisterRoutedEvent(
            "OpenButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Welcome));

        public static readonly RoutedEvent ImportButtonClickEvent = EventManager.RegisterRoutedEvent(
            "ImportButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Welcome));

        public static readonly RoutedEvent OpenRecentFileButtonClickEvent = EventManager.RegisterRoutedEvent(
            "OpenRecentFileButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Welcome));

        // Expose this event for this control's container
        public event RoutedEventHandler NewButtonClick
        {
            add { AddHandler(NewButtonClickEvent, value); }
            remove { RemoveHandler(NewButtonClickEvent, value); }
        }

        // Expose this event for this control's container
        public event RoutedEventHandler OpenButtonClick
        {
            add { AddHandler(OpenButtonClickEvent, value); }
            remove { RemoveHandler(OpenButtonClickEvent, value); }
        }

        // Expose this event for this control's container
        public event RoutedEventHandler ImportButtonClick
        {
            add { AddHandler(ImportButtonClickEvent, value); }
            remove { RemoveHandler(ImportButtonClickEvent, value); }
        }


        // Expose this event for this control's container
        public event RoutedEventHandler OpenRecentFileButtonClick
        {
            add { AddHandler(OpenRecentFileButtonClickEvent, value); }
            remove { RemoveHandler(OpenRecentFileButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(NewButtonClickEvent));
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OpenButtonClickEvent));
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ImportButtonClickEvent));
        }

        private void OpenRecentFile_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OpenRecentFileButtonClickEvent, sender));
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Creates the Recent Files list.
        /// </summary>
        private void CreateRecentFiles()
        {
            foreach (string file in App.RecentFiles)
            {
                Button fileButton = new Button();
                fileButton.Content = System.IO.Path.GetFileName(file);
                fileButton.CommandParameter = file;
                fileButton.Style = (Style)FindResource("RecentFileButtonStyle");
                fileButton.Click += new RoutedEventHandler(OpenRecentFile_Click);

                RecentFilesStackPanel.Children.Add(fileButton);
            }
        }
        
        /// <summary>
        /// Display the application version.
        /// </summary>
        private void DisplayVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content += string.Format(CultureInfo.CurrentCulture, 
                "{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        #endregion
    }
}