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
        private readonly TranslatorViewModel translatorViewModel; // Add TranslatorViewModel

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

        public string RecognizedLang
        {
            get { return recognizedLang; }
            set
            {
                recognizedLang = value;
                OnPropertyChanged();
            }
        }
        

        // New properties for labels and button text
        public string CartIDLabel { get; set; } = "Cart ID";
        public string TitleLabel { get; set; } = "Title";
        public string LocationLabel { get; set; } = "Location";
        public string QuantityLabel { get; set; } = "Quantity";
        public string DescriptionLabel { get; set; } = "Description";
        public string ItemsLeftLabel { get; set; } = "Items Left";
        public string SerialNumberLabel { get; set; } = "Serial Number";
        public string SkipItemButtonText { get; set; } = "Skip Item";
        public string HomeButtonText { get; set; } = "Home";

        public ICommand SkipItemCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand TranslateCommand { get; } // Add TranslateCommand
        private readonly SpeechToTextViewModel speechToTextViewModel;
        private readonly TextToSpeechViewModel textToSpeechViewModel;

        private readonly IntentViewModel _intentViewModel;

        // Define the array of Cart objects
        public Cart[] Carts { get; set; }
        public class Cart
        {
            public string CartID { get; set; }
            public List<PickingItem> Items { get; set; }

            public Cart(string cartID)
            {
                CartID = cartID;
                Items = new List<PickingItem>();
            }
        }

        #endregion
        public PickItemViewModel()
        {
        }
        public PickItemViewModel(string cartID, string RecognizedLang)
        {
            recognizedLang = RecognizedLang;
            textToSpeechViewModel = new TextToSpeechViewModel();

            speechToTextViewModel = new SpeechToTextViewModel();
            _intentViewModel = new IntentViewModel(this);
            translatorViewModel = new TranslatorViewModel(); // Initialize TranslatorViewModel
            CartID = cartID;

            // Initialize the array of Cart objects
            Carts = new Cart[]
            {
                        new Cart("1")
                        {
                            Items = new List<PickingItem>
                            {
                                new PickingItem { CartID = "1", Quantity = "10", Title = "Sample Item 1", Location = "Aisle 3, Shelf 2", Description = "This is a sample.", ItemsLeft = "2", SerialNumber = "1" },
                                new PickingItem { CartID = "1", Quantity = "5", Title = "Sample Item 2", Location = "Aisle 4, Shelf 1", Description = "This is another sample.", ItemsLeft = "1", SerialNumber = "2" }
                            }
                        },
                        new Cart("2")
                        {
                            Items = new List<PickingItem>
                            {
                                new PickingItem { CartID = "2", Quantity = "5", Title = "Sample Item 3", Location = "Aisle 4, Shelf 1", Description = "This is another sample.", ItemsLeft = "0", SerialNumber = "3" }
                            }
                        },
                        new Cart("3")
                        {
                            Items = new List<PickingItem>
                            {
                                new PickingItem { CartID = "3", Quantity = "8", Title = "Sample Item 4", Location = "Aisle 5, Shelf 3", Description = "This is yet another sample.", ItemsLeft = "3", SerialNumber = "4" }
                            }
                        }
            };

            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);
            TranslateCommand = new RelayCommand(async () => await TranslateItemAttributes(RecognizedLang)); // Initialize TranslateCommand

            currentIndex = 0; // Initialize the index
            InitializeAsync(RecognizedLang);
        }
        #region Tasks

        private async Task InitializeAsync(string RecognizedLang)
        {
            recognizedLang = RecognizedLang;
            if (CartID.EndsWith(".") == true)
                CartID = CartID.Substring(0, CartID.Length - 1);

            var cart = Carts.FirstOrDefault(c => c.CartID == CartID);

            if (cart != null && currentIndex < cart.Items.Count)
            {
                PickingItem = cart.Items[currentIndex]; // Set the PickingItem property to the current item in the cart
                await TranslateItemAttributes(RecognizedLang);
                await textToSpeechViewModel.SynthesizeAllInfo(
                    PickingItem.CartID,
                    PickingItem.Title,
                    PickingItem.Quantity,
                    PickingItem.Location,
                    PickingItem.Description,
                    PickingItem.ItemsLeft,
                    PickingItem.SerialNumber,
                    CartIDLabel,
                    TitleLabel,
                    QuantityLabel,
                    LocationLabel,
                    DescriptionLabel,
                    ItemsLeftLabel,
                    SerialNumberLabel,
                    RecognizedLang,
                    translatorViewModel
                );
                IsRecording = true;
                await speechToTextViewModel.RecognizeSpeechFromMic();
                RecognizedText = speechToTextViewModel.RecognizedText;
                RecognizedLang = speechToTextViewModel.RecognizedLang;
                IsRecording = false;
                var intent = await _intentViewModel.AnalyzeConversationAsync(RecognizedText, "en");

                // Handle the intent using the new method
                await HandleIntent(intent);
            }
            else
            {
                await textToSpeechViewModel.SynthesizeSpeech("All items have been picked.", RecognizedLang);

                Title = "";
                Description = "";
                Location = "";              
                Quantity = "";
                ItemsLeft = "";
                SerialNumber = "";
                 await OnHome();
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
                    await HandleYesResponse();
                    break;
                case "Picked item":
                    await HandleYesResponse();
                    break;
                default:
                    await HandleYesResponse();
                    break;
            }
        }

        public async Task HandleYesResponse()
        {
            // Translate the message based on the recognized language
            string message = "Great, let's move on to the next item on the ticket";
            if (RecognizedLang == "es")
            {
                await translatorViewModel.TranslateTextToSpanish(message);
                message = translatorViewModel.TranslationResult;
            }

            // Get the current cart
            var cart = Carts.FirstOrDefault(c => c.CartID == CartID);
            if (cart != null)
            {
                cart.Items.RemoveAt(currentIndex); // Remove the picked item
                if (currentIndex >= cart.Items.Count)
                {
                    currentIndex = 0; // Reset the index if it exceeds the number of items
                }

                // Synthesize the translated message
                await textToSpeechViewModel.SynthesizeSpeech(message, RecognizedLang);

                // Process the next item
                await InitializeAsync(RecognizedLang);
            }
        }

        public async Task HandleNoResponse()
        {
            // Implement the logic for handling "no" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Please try again", RecognizedLang);
            await InitializeAsync(RecognizedLang);
        }

        public async Task HandleArrivedResponse()
        {
            // Implement the logic for handling "arrived" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've arrived", RecognizedLang);
        }

        public async Task HandlePickedItemResponse()
        {
            // Implement the logic for handling "picked item" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've picked the item", RecognizedLang);
        }

        async Task TranslateItemAttributes(string RecognizedLang)
        {
            if (PickingItem != null)
            {
                if (RecognizedLang == "es")
                {
                    // Translate labels to Spanish
                    CartIDLabel = await TranslateLabelToSpanish(CartIDLabel);
                    TitleLabel = await TranslateLabelToSpanish(TitleLabel);
                    LocationLabel = await TranslateLabelToSpanish(LocationLabel);
                    QuantityLabel = await TranslateLabelToSpanish(QuantityLabel);
                    DescriptionLabel = await TranslateLabelToSpanish(DescriptionLabel);
                    ItemsLeftLabel = await TranslateLabelToSpanish(ItemsLeftLabel);
                    SerialNumberLabel = await TranslateLabelToSpanish(SerialNumberLabel);

                    // Translate item attributes to Spanish
                    await translatorViewModel.TranslateTextToSpanish(Title);
                    Title = translatorViewModel.TranslationResult;

                    await translatorViewModel.TranslateTextToSpanish(Description);
                    Description = translatorViewModel.TranslationResult;

                    await translatorViewModel.TranslateTextToSpanish(Location);
                    Location = translatorViewModel.TranslationResult;

                    await translatorViewModel.TranslateTextToSpanish(Quantity);
                    Quantity = translatorViewModel.TranslationResult;

                    await translatorViewModel.TranslateTextToSpanish(ItemsLeft);
                    ItemsLeft = translatorViewModel.TranslationResult;

                    await translatorViewModel.TranslateTextToSpanish(SerialNumber);
                    SerialNumber = translatorViewModel.TranslationResult;
                }
                else
                {
                    // Translate labels to English
                    //CartIDLabel = await TranslateLabelToEnglish(CartIDLabel);
                    //TitleLabel = await TranslateLabelToEnglish(TitleLabel);
                    //LocationLabel = await TranslateLabelToEnglish(LocationLabel);
                    //QuantityLabel = await TranslateLabelToEnglish(QuantityLabel);
                    //DescriptionLabel = await TranslateLabelToEnglish(DescriptionLabel);
                    //ItemsLeftLabel = await TranslateLabelToEnglish(ItemsLeftLabel);
                    //SerialNumberLabel = await TranslateLabelToEnglish(SerialNumberLabel);

                    //// Translate item attributes to English
                    //await translatorViewModel.TranslateTextToEnglish(Title);
                    //Title = translatorViewModel.TranslationResult;

                    //await translatorViewModel.TranslateTextToEnglish(Description);
                    //Description = translatorViewModel.TranslationResult;

                    //await translatorViewModel.TranslateTextToEnglish(Location);
                    //Location = translatorViewModel.TranslationResult;

                    //await translatorViewModel.TranslateTextToEnglish(Quantity);
                    //Quantity = translatorViewModel.TranslationResult;

                    //await translatorViewModel.TranslateTextToEnglish(ItemsLeft);
                    //ItemsLeft = translatorViewModel.TranslationResult;

                    //await translatorViewModel.TranslateTextToEnglish(SerialNumber);
                    //SerialNumber = translatorViewModel.TranslationResult;
                }

                // Notify UI of label changes
                OnPropertyChanged(nameof(CartIDLabel));
                OnPropertyChanged(nameof(TitleLabel));
                OnPropertyChanged(nameof(LocationLabel));
                OnPropertyChanged(nameof(QuantityLabel));
                OnPropertyChanged(nameof(DescriptionLabel));
                OnPropertyChanged(nameof(ItemsLeftLabel));
                OnPropertyChanged(nameof(SerialNumberLabel));

                // Notify UI of item attribute changes
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Location));
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ItemsLeft));
                OnPropertyChanged(nameof(SerialNumber));
            }
        }
        private async Task<string> TranslateLabelToSpanish(string label)
        {
            await translatorViewModel.TranslateTextToSpanish(label);
            return translatorViewModel.TranslationResult;
        }

        private async Task<string> TranslateLabelToEnglish(string label)
        {
            await translatorViewModel.TranslateTextToEnglish(label);
            return translatorViewModel.TranslationResult;
        }
        #endregion

        #region Buttons

        private async Task OnSkipItem()
        {
            // Skip the current item and move to the next item
            await textToSpeechViewModel.SynthesizeSpeech("skipping item", RecognizedLang);
            var cart = Carts.FirstOrDefault(c => c.CartID == CartID);
            if (cart != null)
            {
                cart.Items.RemoveAt(currentIndex); // Remove the skipped item
                if (currentIndex >= cart.Items.Count)
                {
                    currentIndex = 0; // Reset the index if it exceeds the number of items
                }
                await InitializeAsync(RecognizedLang); // Process the next item
            }
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
