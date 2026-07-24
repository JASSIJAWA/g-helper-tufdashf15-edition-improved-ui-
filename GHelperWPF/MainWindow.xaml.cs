using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using GHelper.Battery;
using GHelper.Display;

namespace GHelperWPF;

/// <summary>
/// Main window code-behind for the G-Helper WPF dashboard.
/// Handles window chrome events, sensor polling, and user interactions.
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer? _sensorTimer;
    private DispatcherTimer? _batteryDebounceTimer;

    public MainWindow()
    {
        InitializeComponent();

        // Wire up battery slider
        sliderBatteryLimit.ValueChanged += SliderBatteryLimit_ValueChanged;
        
        _batteryDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _batteryDebounceTimer.Tick += (s, e) =>
        {
            _batteryDebounceTimer.Stop();
            BatteryControl.SetBatteryChargeLimit((int)sliderBatteryLimit.Value);
        };

        // Wire up performance mode buttons
        rbSilent.Checked += (s, e) => SetPerformanceMode(2);
        rbBalanced.Checked += (s, e) => SetPerformanceMode(0);
        rbTurbo.Checked += (s, e) => SetPerformanceMode(1);

        // Wire up GPU modes
        rbEco.Checked += (s, e) => SetGpuMode(1); // Eco
        rbStandard.Checked += (s, e) => SetGpuMode(0); // Standard
        rbUltimate.Checked += (s, e) => SetGpuMode(2); // Ultimate

        // Wire up Display toggles
        chkHighRefresh.Checked += (s, e) => ScreenControl.SetScreen(ScreenControl.MAX_REFRESH);
        chkHighRefresh.Unchecked += (s, e) => ScreenControl.SetScreen(ScreenControl.MIN_RATE);
        
        chkOverdrive.Checked += (s, e) => ScreenControl.SetScreen(overdrive: 1);
        chkOverdrive.Unchecked += (s, e) => ScreenControl.SetScreen(overdrive: 0);

        chkFNLock.Checked += (s, e) => SetFnLock(1);
        chkFNLock.Unchecked += (s, e) => SetFnLock(0);

        // Start sensor polling
        StartSensorPolling();
    }

    // ═══════════ TITLE BAR ═══════════

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _sensorTimer?.Stop();
        this.Close();
    }

    // ═══════════ SENSOR POLLING ═══════════

    private void StartSensorPolling()
    {
        _sensorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _sensorTimer.Tick += SensorTimer_Tick;
        _sensorTimer.Start();

        // Initial read
        UpdateSensorDisplays();
    }

    private void SensorTimer_Tick(object? sender, EventArgs e)
    {
        UpdateSensorDisplays();
    }

    private void UpdateSensorDisplays()
    {
        try
        {
            // Read from G-Helper's static HardwareControl fields
            var cpuT = HardwareControl.cpuTemp;
            var gpuT = HardwareControl.gpuTemp;
            var cpuF = HardwareControl.cpuFanRPM;
            var gpuF = HardwareControl.gpuFanRPM;

            txtCpuTemp.Text = cpuT.HasValue && cpuT > 0 ? $"{cpuT:F0}" : "--";
            txtGpuTemp.Text = gpuT.HasValue && gpuT > 0 ? $"{gpuT:F0}" : "--";
            txtCpuFan.Text = cpuF.HasValue && cpuF > 0 ? $"{cpuF}" : "--";
            txtGpuFan.Text = gpuF.HasValue && gpuF > 0 ? $"{gpuF}" : "--";

            // Battery info
            if (HardwareControl.batteryHealth > 0)
                txtBatteryHealth.Text = $"{HardwareControl.batteryHealth:F0}%";

            if (HardwareControl.batteryRate.HasValue)
                txtDischargeRate.Text = $"{Math.Abs((decimal)HardwareControl.batteryRate):F1} W";
        }
        catch
        {
            // Silently ignore if hardware is not yet initialized
        }
    }

    // ═══════════ HARDWARE CONTROLS ═══════════

    private void SetPerformanceMode(int mode)
    {
        try { GHelper.Mode.Modes.SetCurrent(mode); } catch { }
    }

    private void SetGpuMode(int mode)
    {
        try
        {
            if (GHelper.Program.acpi == null) return;
            switch(mode)
            {
                case 0: // Standard
                    GHelper.Program.acpi.DeviceSet(AsusACPI.GPUEco, 0, "GPUEco");
                    GHelper.Program.acpi.DeviceSet(AsusACPI.GPUMux, 0, "GPUMux");
                    break;
                case 1: // Eco
                    GHelper.Program.acpi.DeviceSet(AsusACPI.GPUEco, 1, "GPUEco");
                    GHelper.Program.acpi.DeviceSet(AsusACPI.GPUMux, 0, "GPUMux");
                    break;
                case 2: // Ultimate
                    GHelper.Program.acpi.DeviceSet(AsusACPI.GPUMux, 1, "GPUMux");
                    break;
            }
        } catch { }
    }

    private void SetFnLock(int lockState)
    {
        try
        {
            GHelper.Program.acpi?.DeviceSet(AsusACPI.FnLock, lockState, "FnLock");
        } catch { }
    }

    private void SliderBatteryLimit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int limit = (int)e.NewValue;
        txtBatteryLimit.Text = $"{limit}%";
        
        // Restart debounce timer
        _batteryDebounceTimer?.Stop();
        _batteryDebounceTimer?.Start();
    }
}