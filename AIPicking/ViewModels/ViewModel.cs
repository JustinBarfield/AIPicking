using AIPicking.ViewModels;
using AIPicking.Views;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AIPicking
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Properties
        private string textBoxValue;

        public string TextBoxValue
        {
            get { return textBoxValue; }
            set
            {
                textBoxValue = value;
                OnPropertyChanged();
            }
        }

       
        private string ticketNumber;
        public string TicketNumber
        {
            get { return ticketNumber; }
            set
            {
                ticketNumber = value;
                OnPropertyChanged();
            }
        }

       
        public ICommand RecognizeSpeechFromMicCommand { get; }
        public ICommand OpenScanCartIDViewCommand { get; }
        public ICommand OpenPickItemViewCommand { get; }
        public ICommand SynthesizeSpeechCommand { get; }

      
        #endregion

      
        private readonly TextToSpeechViewModel textToSpeechViewModel;
        private readonly SpeechToTextViewModel speechToTextViewModel;
        public ViewModel()
        {
            textToSpeechViewModel = new TextToSpeechViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            SynthesizeSpeechCommand = new RelayCommand(async () => await textToSpeechViewModel.SynthesizeSpeech(TextBoxValue));
            RecognizeSpeechFromMicCommand = new RelayCommand(async () =>
            {
                await speechToTextViewModel.RecognizeSpeechFromMic();
                TextBoxValue = speechToTextViewModel.RecognizedText;
            });
            OpenScanCartIDViewCommand = new RelayCommand(async () => await OpenScanCartIDView(null, null));
            OpenPickItemViewCommand = new RelayCommand(async () => await OpenPickItemView(null, null));
        }
       

        public async Task OpenScanCartIDView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel();
            var cartIDView = new CartID { DataContext = cartIDViewModel };

            // Assuming you have a reference to the current window
            var currentWindow = System.Windows.Application.Current.MainWindow;

            // Update the content of the current window
            currentWindow.Content = cartIDView;

            // Optionally, you can update the title or other properties of the current window
            currentWindow.Title = "Scan Cart ID";
            currentWindow.Width = 400;
            currentWindow.Height = 300;

            // Start the speech synthesis without awaiting it
            // cartIDViewModel.SynthesizeSpeech();
            await textToSpeechViewModel.SynthesizeSpeech("Say the cart ID");
            await speechToTextViewModel.RecognizeSpeechFromMic();
        }

        public async Task OpenPickItemView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel(); // Create an instance of CartIDViewModel
            var pickItemViewModel = new PickItemViewModel();
            var pickItemView = new PickItemUC { DataContext = pickItemViewModel };

            // Assuming you have a reference to the current window
            var currentWindow = System.Windows.Application.Current.MainWindow;

            // Update the content of the current window
            currentWindow.Content = pickItemView;

            // Optionally, you can update the title or other properties of the current window
            currentWindow.Title = "Pick Item";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class RelayCommand : ICommand
        {
            private readonly Func<Task> _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute();
            }

            public async void Execute(object parameter)
            {
                await _execute();
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
