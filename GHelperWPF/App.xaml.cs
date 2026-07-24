using System.Windows;
using System.Drawing;
using GHelper;

namespace GHelperWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Initialize ACPI
        GHelper.Program.acpi = new AsusACPI();
        
        // 2. Initialize Hardware Control
        HardwareControl.RecreateGpuControl();

        // 3. Create System Tray Icon
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = SystemIcons.Application, // Fallback icon
            Visible = true,
            Text = "G-Helper (ROG Edition)"
        };

        // Try to load the standard G-Helper icon if available
        try
        {
            _notifyIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/GHelperWPF;component/Resources/standard.ico")).Stream);
        }
        catch { }

        _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();

        // Add Context Menu to Tray Icon
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (s, args) => ShowMainWindow());
        contextMenu.Items.Add("Quit", null, (s, args) => Shutdown());
        _notifyIcon.ContextMenuStrip = contextMenu;
        
        // Ensure app doesn't close when main window closes
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Show window initially
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null || !_mainWindow.IsLoaded)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (s, e) => _mainWindow = null;
            _mainWindow.Show();
        }
        else
        {
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.OnExit(e);
    }
}
