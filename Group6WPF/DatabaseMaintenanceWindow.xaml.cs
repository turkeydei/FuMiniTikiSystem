using FUMiniTikiSystem.BLL;
using System.Windows;

namespace Group6WPF
{
    public partial class DatabaseMaintenanceWindow : Window
    {
        private readonly DatabaseMaintenanceService _maintenanceService;

        public DatabaseMaintenanceWindow(DatabaseMaintenanceService maintenanceService)
        {
            InitializeComponent();
            _maintenanceService = maintenanceService;
        }

        private void btnResetIdentitySeeds_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will reset the identity seed for all tables. Continue?",
                "Confirm Reset Identity Seeds",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _maintenanceService.ResetAllIdentitySeeds();
                    MessageBox.Show(
                        "Identity seeds have been reset successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error resetting identity seeds: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void btnClearAllData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "⚠️ WARNING: This will delete ALL data in the database!\n\n" +
                "This action cannot be undone. Are you absolutely sure?",
                "Confirm Clear All Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var confirmResult = MessageBox.Show(
                    "Are you REALLY sure? This will delete everything!",
                    "Final Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        _maintenanceService.ClearAllDataAndResetSeeds();
                        MessageBox.Show(
                            "All data has been cleared and identity seeds reset!",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Error clearing data: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}