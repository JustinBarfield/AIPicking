using AIPicking.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AIPicking.ViewModels
{
    public class PickItemViewModel : INotifyPropertyChanged
    {
        private PickingItem pickingItem;
        private string cartID;

        public PickingItem PickingItem
        {
            get => pickingItem;
            set
            {
                pickingItem = value;
                OnPropertyChanged();
            }
        }

        public string CartID
        {
            get => cartID;
            set
            {
                cartID = value;
                OnPropertyChanged();
            }
        }

        public string Quantity
        {
            get => PickingItem.Quantity;
            set
            {
                PickingItem.Quantity = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => PickingItem.Title;
            set
            {
                PickingItem.Title = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => PickingItem.Location;
            set
            {
                PickingItem.Location = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => PickingItem.Description;
            set
            {
                PickingItem.Description = value;
                OnPropertyChanged();
            }
        }

        public string ItemsLeft
        {
            get => PickingItem.ItemsLeft;
            set
            {
                PickingItem.ItemsLeft = value;
                OnPropertyChanged();
            }
        }

        public string SerialNumber
        {
            get => PickingItem.SerialNumber;
            set
            {
                PickingItem.SerialNumber = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand SkipItemCommand { get; }
        public ICommand HomeCommand { get; }

        public PickItemViewModel()
        {
            PickingItem = new PickingItem();
            ConfirmCommand = new RelayCommand(OnConfirm);
            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);

            var cartIDViewModel = new CartIDViewModel(); // Create an instance of CartIDViewModel
            cartID = cartIDViewModel.CartID;

            // Generate random picking items when the view model is instantiated
            var randomItems = GenerateRandomPickingItems(1);
            if (randomItems.Length > 0)
            {
                PickingItem = randomItems[0];
            }
        }

        public PickingItem[] GenerateRandomPickingItems(int count)
        {
            var random = new Random();
            var items = new PickingItem[count];

            for (int i = 0; i < count; i++)
            {
                items[i] = new PickingItem
                {
                    Quantity = random.Next(1, 100).ToString(),
                    Title = $"Item {i + 1}",
                    Location = $"Location {random.Next(1, 50)}",
                    Description = $"Description for item {i + 1}",
                    ItemsLeft = random.Next(0, 100).ToString(),
                    SerialNumber = Guid.NewGuid().ToString()
                };
            }

            return items;
        }

        private async Task OnConfirm()
        {
            // Implement the logic for the Confirm button
        }

        private async Task OnSkipItem()
        {
            // Implement the logic for the Skip Item button
        }

        private async Task OnHome()
        {
            var viewModel = new ViewModel();
            var view = new HomePageUC { DataContext = viewModel };

            // Assuming you have a reference to the current window
            var currentWindow = System.Windows.Application.Current.MainWindow;

            // Update the content of the current window
            currentWindow.Content = view;

            // Optionally, you can update the title or other properties of the current window
            currentWindow.Title = "Main Window";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
