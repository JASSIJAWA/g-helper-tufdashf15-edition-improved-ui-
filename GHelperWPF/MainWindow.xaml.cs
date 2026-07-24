using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GHelperWPF;

/// <summary>
/// Main window code-behind for the G-Helper WPF dashboard.
/// Handles window chrome events, sensor polling, and user interactions.
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer? _sensorTimer;

    public MainWindow()
    {
        InitializeComponent();

        // Wire up slider value changed
        sliderBatteryLimit.ValueChanged += SliderBatteryLimit_ValueChanged;

        // Wire up performance mode buttons
        rbSilent.Checked += (s, e) => SetPerformanceMode(2);
        rbBalanced.Checked += (s, e) => SetPerformanceMode(0);
        rbTurbo.Checked += (s, e) => SetPerformanceMode(1);

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

    // ═══════════ PERFORMANCE MODES ═══════════

    private void SetPerformanceMode(int mode)
    {
        try
        {
            GHelper.Mode.Modes.SetCurrent(mode);
        }
        catch
        {
            // Hardware not initialized yet
        }
    }

    // ═══════════ BATTERY ═══════════

    private void SliderBatteryLimit_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int limit = (int)e.NewValue;
        txtBatteryLimit.Text = $"{limit}%";
    }
}