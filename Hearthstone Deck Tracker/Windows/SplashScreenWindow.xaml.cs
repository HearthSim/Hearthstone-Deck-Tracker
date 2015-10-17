namespace Hearthstone_Deck_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for SplashScreenWindow.xaml
    /// </summary>
    public partial class SplashScreenWindow
    {
        public string VersionString { get; private set; }

        public SplashScreenWindow()
        {
            var version = Helper.GetCurrentVersion();
            VersionString = string.Format("v{0}.{1}.{2}", version.Major, version.Minor, version.Build);
            InitializeComponent();
        }
    }
}
