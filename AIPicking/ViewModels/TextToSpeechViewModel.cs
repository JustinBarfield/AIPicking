using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static AIPicking.ViewModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace AIPicking.ViewModels
{
    class TextToSpeechViewModel
    {
        private static string speechKey = "8xzDB1l9OOZGb5CLKHjS82qhnAPeVV31yKZqDAyTmde0A98lbYcRJQQJ99BBACYeBjFXJ3w3AAAYACOGlizV";
        private static string speechRegion = "eastus";

        public ICommand SynthesizeSpeechCommand { get; }

        public TextToSpeechViewModel()
        {

        }

        public async Task SynthesizeSpeech(string text)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }

        static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Console.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
        }

        public async Task SynthesizeAllInfo(string CartID, string Title, string Quantity, string Location, string Description, string ItemsLeft, string SerialNumber)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = $"Item: {Title}, Cart ID: {CartID}, Quantity: {Quantity}, Location: {Location}, Description: {Description}, Items Left: {ItemsLeft}, Serial Number: {SerialNumber}";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }

            // Ask if the user is at the shelf
            await AskIfAtShelf();
        }

        private async Task AskIfAtShelf()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = "Are you at the shelf?";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }
    }
}
