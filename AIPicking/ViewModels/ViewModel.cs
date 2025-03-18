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
        private bool isRecording;
        public bool IsRecording
        {
            get { return isRecording; }
            set
            {
                isRecording = value;
                OnPropertyChanged();
            }
        }
        private string recognizedText;
        public string RecognizedText
        {
            get { return recognizedText; }
            set
            {
                recognizedText = value;
                OnPropertyChanged();
            }
        }
        public ICommand RecognizeSpeechFromMicCommand { get; }
        public ICommand OpenScanCartIDViewCommand { get; }
        public ICommand OpenPickItemViewCommand { get; }
        public ICommand SynthesizeSpeechCommand { get; }
        public ICommand TranslateCommand => translatorViewModel.TranslateCommand;

        #endregion

        private readonly TextToSpeechViewModel textToSpeechViewModel;
        private readonly SpeechToTextViewModel speechToTextViewModel;
        private readonly TranslatorViewModel translatorViewModel;

        public ViewModel()
        {
            textToSpeechViewModel = new TextToSpeechViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            translatorViewModel = new TranslatorViewModel();
            SynthesizeSpeechCommand = new RelayCommand(async () => await textToSpeechViewModel.SynthesizeSpeech(TextBoxValue));
            RecognizeSpeechFromMicCommand = new RelayCommand(async () =>
            {
                IsRecording = true;
                await speechToTextViewModel.RecognizeSpeechFromMic();
                RecognizedText = speechToTextViewModel.RecognizedText;
                isRecording = false;
            });
            OpenScanCartIDViewCommand = new RelayCommand(async () => await OpenScanCartIDView(null, null));
            OpenPickItemViewCommand = new RelayCommand(async () => await OpenPickItemView(null, null));
        }

        public async Task OpenScanCartIDView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel();
            var cartIDView = new CartID { DataContext = cartIDViewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = cartIDView;
            currentWindow.Title = "Scan Cart ID";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }

        public async Task OpenPickItemView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel();
            var pickItemViewModel = new PickItemViewModel();
            var pickItemView = new PickItemUC { DataContext = pickItemViewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = pickItemView;
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
