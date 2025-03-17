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
    public class PickItemViewModel : INotifyPropertyChanged, IResponseHandler
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
        private readonly IntentViewModel _intentViewModel;
        #endregion

        public PickItemViewModel()
        {
        }

        public PickItemViewModel(string cartID)
        {
            textToSpeechViewModel = new TextToSpeechViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            _intentViewModel = new IntentViewModel(this);
            CartID = cartID;
            //need to make fake cartID's and items
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

            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            //need logic to loop through all items in the cart

            await textToSpeechViewModel.SynthesizeAllInfo(CartID, Quantity, Title, Location, Description, ItemsLeft, SerialNumber);
            await textToSpeechViewModel.SynthesizeSpeech("are you at the shelf?");
            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            RecognizedText = speechToTextViewModel.RecognizedText;
            IsRecording = false;
            var intent = await _intentViewModel.AnalyzeConversationAsync(RecognizedText, "en");

            // Handle the intent using a switch case statement
            switch (intent)
            {
                case "yes":
                    await HandleYesResponse();
                    break;
                case "No":
                    await HandleNoResponse();
                    break;
                case "Arrived":
                    await HandleArrivedResponse();
                    break;
                case "Picked item":
                    await HandlePickedItemResponse();
                    break;
                default:
                    await textToSpeechViewModel.SynthesizeSpeech("I didn't understand that. Please try again.");
                    break;
            }
        }

        public async Task HandleYesResponse()
        {
            // Implement the logic for handling "yes" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Great, let's move on to the next item in PickItemViewModel");
        }

        public async Task HandleNoResponse()
        {
            // Implement the logic for handling "no" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Please try again in PickItemViewModel");
        }

        public async Task HandleArrivedResponse()
        {
            // Implement the logic for handling "arrived" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've arrived in PickItemViewModel");
        }

        public async Task HandlePickedItemResponse()
        {
            // Implement the logic for handling "picked item" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've picked the item in PickItemViewModel");
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
    }
}
