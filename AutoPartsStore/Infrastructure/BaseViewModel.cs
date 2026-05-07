using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutoPartsStore.Infrastructure
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        protected async Task ExecuteAsync(string operationName, System.Func<Task> asyncAction)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await asyncAction();
            }
            catch (System.Exception ex)
            {
                SnackbarService.Show($"Ошибка при {operationName}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());
            }
        }

        protected async Task ExecuteAsync(string operationName, System.Action syncAction)
        {
            await ExecuteAsync(operationName, () => Task.Run(syncAction));
        }
    }
}