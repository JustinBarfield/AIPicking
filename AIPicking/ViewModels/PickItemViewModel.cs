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
        private int currentIndex; // Add an index field

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
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(ItemsLeft));
                OnPropertyChanged(nameof(SerialNumber));
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
            get => PickingItem?.Quantity;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.Quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => PickingItem?.Title;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Location
        {
            get => PickingItem?.Location;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.Location = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => PickingItem?.Description;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ItemsLeft
        {
            get => PickingItem?.ItemsLeft;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.ItemsLeft = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SerialNumber
        {
            get => PickingItem?.SerialNumber;
            set
            {
                if (PickingItem != null)
                {
                    PickingItem.SerialNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand SkipItemCommand { get; }
        public ICommand HomeCommand { get; }
        private readonly SpeechToTextViewModel speechToTextViewModel;
        private readonly TextToSpeechViewModel textToSpeechViewModel;
        private readonly IntentViewModel _intentViewModel;

        // Define the array of PickingItem objects
        public PickingItem[] PickingItems { get; set; }

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

            // Initialize the array of PickingItem objects
            PickingItems = new PickingItem[]
            {
                new PickingItem { CartID = cartID, Quantity = "10", Title = "Sample Item 1", Location = "Aisle 3, Shelf 2", Description = "This is a sample.", ItemsLeft = "2", SerialNumber = "1" },
                new PickingItem { CartID = cartID, Quantity = "5", Title = "Sample Item 2", Location = "Aisle 4, Shelf 1", Description = "This is another sample.", ItemsLeft = "1", SerialNumber = "2" },
                new PickingItem { CartID = cartID, Quantity = "5", Title = "Sample Item 3", Location = "Aisle 4, Shelf 1", Description = "This is another sample.", ItemsLeft = "0", SerialNumber = "3" },
            };

            ConfirmCommand = new RelayCommand(OnConfirm);
            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);

            currentIndex = 0; // Initialize the index
            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (currentIndex < PickingItems.Length)
            {
                var item = PickingItems[currentIndex];
                PickingItem = item; // Set the PickingItem property
                await textToSpeechViewModel.SynthesizeAllInfo(item.CartID, item.Title, item.Quantity, item.Location, item.Description, item.ItemsLeft, item.SerialNumber);
                await textToSpeechViewModel.SynthesizeSpeech("are you at the shelf?");
                IsRecording = true;
                await speechToTextViewModel.RecognizeSpeechFromMic();
                RecognizedText = speechToTextViewModel.RecognizedText;
                IsRecording = false;
                var intent = await _intentViewModel.AnalyzeConversationAsync(RecognizedText, "en");

                // Handle the intent using the new method
                await HandleIntent(intent);
            }
            else
            {
                await textToSpeechViewModel.SynthesizeSpeech("All items have been picked.");
            }
        }

        private async Task HandleIntent(string intent)
        {
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
            // Go to the next item
            currentIndex++;
            await textToSpeechViewModel.SynthesizeSpeech("Great, let's move on to the next item on the ticket");
            await InitializeAsync(); // Process the next item
        }

        public async Task HandleNoResponse()
        {
            // Implement the logic for handling "no" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Please try again");
            await InitializeAsync();
        }

        public async Task HandleArrivedResponse()
        {
            // Implement the logic for handling "arrived" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've arrived");
        }

        public async Task HandlePickedItemResponse()
        {
            // Implement the logic for handling "picked item" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've picked the item");
        }

        #region Buttons
        private async Task OnConfirm()
        {
            // Implement the logic for the Confirm button
        }

        private async Task OnSkipItem()
        {
            // Skip the current item and move to the next item
            await textToSpeechViewModel.SynthesizeSpeech("skipping item");
            currentIndex++;
            await InitializeAsync(); // Process the next item
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
