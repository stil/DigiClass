using System.Runtime;
using System.Windows;
using MathNet.Numerics;

namespace DigiClass.Demo
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Control.ConfigureAuto();
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }
    }
}