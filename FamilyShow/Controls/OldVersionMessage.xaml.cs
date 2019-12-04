using System.Windows;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// Interaction logic for Welcome.xaml
  /// </summary>
  public partial class OldVersionMessage : System.Windows.Controls.UserControl
    {
        public OldVersionMessage()
        {
            InitializeComponent();

            DontShowOldVersionMessage = Properties.Settings.Default.DontShowOldVersionMessage;
        }

        #region routed events

        public static readonly RoutedEvent ContinueButtonClickEvent = EventManager.RegisterRoutedEvent(
            "ContinueButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OldVersionMessage));


        // Expose this event for this control's container
        public event RoutedEventHandler ContinueButtonClick
        {
            add { AddHandler(ContinueButtonClickEvent, value); }
            remove { RemoveHandler(ContinueButtonClickEvent, value); }
        }

        #endregion

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ContinueButtonClickEvent));            
        }

        public bool DontShowOldVersionMessage
        {       
            get { return (bool)GetValue(ShowOldVersionMessageProperty); }
            set { SetValue(ShowOldVersionMessageProperty, value); }
        }

        public static readonly DependencyProperty ShowOldVersionMessageProperty =
            DependencyProperty.Register("ShowOldVersionMessage", typeof(bool), typeof(OldVersionMessage),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnShowOldVersionMessageChanged)));

        private static void OnShowOldVersionMessageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Properties.Settings.Default.DontShowOldVersionMessage = (bool)args.NewValue;
            Properties.Settings.Default.Save();
        }
    }
}