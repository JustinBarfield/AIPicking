using AIPicking.Views;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static AIPicking.ViewModel;

namespace AIPicking.ViewModels
{
    public class PickItemViewModel : INotifyPropertyChanged
    {
        #region Properties
       
        private PickingItem pickingItem;
        private string recognizedText;
        private string ticketNumber;
        
        private string recognizedLang;
        private bool isRecording;

        public string RecognizedText
        {
            get => recognizedText;
            set
            {
                recognizedText = value;
                OnPropertyChanged();
            }
        }

        public string TicketNumber
        {
            get => ticketNumber;
            set
            {
                ticketNumber = value;
                OnPropertyChanged();
            }
        }

        public bool IsRecording
        {
            get => isRecording;
            set
            {
                isRecording = value;
                OnPropertyChanged();
            }
        }
        public PickingItem PickingItem
        {
            get => pickingItem;
            set
            {
                pickingItem = value;
                OnPropertyChanged();
            }
        }

        private string cartID;
        public string CartID
        {
            get { return cartID; }
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
        private readonly SpeechToTextViewModel speechToTextViewModel;
        private readonly TextToSpeechViewModel textToSpeechViewModel;
        #endregion
        public PickItemViewModel()
        {
            textToSpeechViewModel = new TextToSpeechViewModel();

            PickingItem = new PickingItem
            {
                CartID = "1234",
                Quantity = "10",
                Title = "Sample Item",
                Location = "Aisle 3, Shelf 2",
                Description = "This is a sample.",
                ItemsLeft = "50",
                SerialNumber = "123"
            };
            ConfirmCommand = new RelayCommand(OnConfirm);
            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);

            textToSpeechViewModel.SynthesizeAllInfo(CartID, Quantity, Title, Location, Description, ItemsLeft, SerialNumber);
        }

        
        #region Buttons
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

        #endregion

      
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public PickItemViewModel(string cartID)
        {
            textToSpeechViewModel = new TextToSpeechViewModel();

            PickingItem = new PickingItem
            {
                CartID = cartID, // Set the CartID here
                Quantity = "10",
                Title = "Sample Item",
                Location = "Aisle 3, Shelf 2",
                Description = "This is a sample.",
                ItemsLeft = "50",
                SerialNumber = "123"
            };
            ConfirmCommand = new RelayCommand(OnConfirm);
            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);

            textToSpeechViewModel.SynthesizeAllInfo(cartID, Quantity, Title, Location, Description, ItemsLeft, SerialNumber);
        }
    }
}
