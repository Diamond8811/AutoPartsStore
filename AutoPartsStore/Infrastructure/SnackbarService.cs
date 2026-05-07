using System.Windows;

namespace AutoPartsStore.Infrastructure
{
    public static class SnackbarService
    {
        public static void Show(string message, string actionText = null, System.Action actionHandler = null)
        {
            if (actionText != null && actionHandler != null)
            {
                var result = MessageBox.Show(message, "Уведомление", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                    actionHandler?.Invoke();
            }
            else
            {
                MessageBox.Show(message, "Уведомление", MessageBoxButton.OK);
            }
        }
    }
}