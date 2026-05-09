using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using AutoPartsStore.Infrastructure;
using AutoPartsStore.Models;

namespace AutoPartsStore.ViewModels
{
    public class LogsViewModel : BaseViewModel
    {
        public ObservableCollection<ActivityLog> Logs { get; set; }
        public ObservableCollection<Users> Users { get; set; }
        public ObservableCollection<string> ActionTypes { get; set; }

        private Users _selectedUserFilter;
        public Users SelectedUserFilter
        {
            get => _selectedUserFilter;
            set { _selectedUserFilter = value; OnPropertyChanged(); }
        }

        private string _selectedActionType;
        public string SelectedActionType
        {
            get => _selectedActionType;
            set { _selectedActionType = value; OnPropertyChanged(); }
        }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ResetFiltersCommand { get; }

        public LogsViewModel()
        {
            Logs = new ObservableCollection<ActivityLog>();
            Users = new ObservableCollection<Users>();
            ActionTypes = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(_ => LoadLogs());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            LoadUsers();
            LoadActionTypes();
            LoadLogs();
        }

        private void LoadUsers()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                Users.Clear();
                foreach (var u in db.Users.ToList())
                    Users.Add(u);
            }
        }

        private void LoadActionTypes()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var types = db.ActivityLog
                    .Select(l => l.ActionType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                ActionTypes.Clear();
                foreach (var t in types)
                    ActionTypes.Add(t);
            }
        }

        private void LoadLogs()
        {
            using (var db = new AutoPartsStoreDBEntities())
            {
                var query = db.ActivityLog
                    .Include(l => l.Users)
                    .AsQueryable();

                if (SelectedUserFilter != null)
                    query = query.Where(l => l.UserId == SelectedUserFilter.UserId);

                if (!string.IsNullOrWhiteSpace(SelectedActionType))
                    query = query.Where(l => l.ActionType == SelectedActionType);

                if (DateFrom.HasValue)
                    query = query.Where(l => l.ActionDate >= DateFrom.Value);

                if (DateTo.HasValue)
                    query = query.Where(l => l.ActionDate <= DateTo.Value);

                var list = query
                    .OrderByDescending(l => l.ActionDate)
                    .ToList();

                Logs.Clear();
                foreach (var log in list)
                    Logs.Add(log);
            }
        }

        private void ResetFilters()
        {
            SelectedUserFilter = null;
            SelectedActionType = null;
            DateFrom = null;
            DateTo = null;
            LoadLogs();
        }
    }
}