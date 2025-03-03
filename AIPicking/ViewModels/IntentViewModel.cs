using Azure.AI.Language.Conversations;
using Azure.Core;
using Azure;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using AIPicking;
using Microsoft.CognitiveServices.Speech;
using System.Linq;
using AIPicking.ViewModels;
using static AIPicking.ViewModel;

public class IntentViewModel : INotifyPropertyChanged
{
    #region Properties
    private Uri endpoint = new Uri("https://seniordesignlanguage.cognitiveservices.azure.com/");
    private AzureKeyCredential credential = new AzureKeyCredential("59O1KpOwwOQwFTliUh931fyiATPPXJES1T5CJNg6dGAga7odm5G2JQQJ99BBACYeBjFXJ3w3AAAaACOGyRH3");
    static string speechKey = "8xzDB1l9OOZGb5CLKHjS82qhnAPeVV31yKZqDAyTmde0A98lbYcRJQQJ99BBACYeBjFXJ3w3AAAYACOGlizV";
    static string speechRegion = "eastus";

    private ConversationAnalysisClient client;

    private string inputText;
    public string InputText
    {
        get => inputText;
        set
        {
            inputText = value;
            OnPropertyChanged(nameof(InputText));
        }
    }

    private string registeredLang;
    public string RegisteredLang
    {
        get => registeredLang;
        set
        {
            registeredLang = value;
            OnPropertyChanged(nameof(RegisteredLang));
        }
    }

    private string intent;
    public string Intent
    {
        get => intent;
        set
        {
            intent = value;
            OnPropertyChanged(nameof(Intent));
        }
    }

    private float confidenceScore;
    public float ConfidenceScore
    {
        get => confidenceScore;
        set
        {
            confidenceScore = value;
            OnPropertyChanged(nameof(ConfidenceScore));
        }
    }

   

    public ICommand AnalyzeCommand { get; }
    public ICommand SynthesizeSpeechCommand { get; }
    // Add a private field for PickItemViewModel
    private PickItemViewModel pickItemViewModel;
    #endregion
    public IntentViewModel()
    {
        client = new ConversationAnalysisClient(endpoint, credential);
        AnalyzeCommand = new RelayCommand(async () => await AnalyzeConversationAsync());
        //pickItemViewModel = new PickItemViewModel();
    }

    public async Task AnalyzeConversationAsync()
    {
        string projectName = "ConversationalUnderstanding";
        string deploymentName = "PickingAI";

        var data = new
        {
            analysisInput = new
            {
                conversationItem = new
                {
                    text = InputText,
                    id = "1",
                    language = RegisteredLang,
                    participantId = "1",
                }
            },
            parameters = new
            {
                projectName,
                deploymentName,

                // Use Utf16CodeUnit for strings in .NET.
                stringIndexType = "Utf16CodeUnit",
            },
            kind = "Conversation",
        };

        Response response = await client.AnalyzeConversationAsync(RequestContent.Create(data));

        JsonDocument result = JsonDocument.Parse(response.ContentStream);
        JsonElement conversationalTaskResult = result.RootElement;
        JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

        Intent = conversationPrediction.GetProperty("topIntent").GetString();
        ConfidenceScore = conversationPrediction.GetProperty("intents").EnumerateArray()
            .FirstOrDefault(intent => intent.GetProperty("category").GetString() == Intent)
            .GetProperty("confidenceScore").GetSingle();

        Console.WriteLine($"Top intent: {Intent}");
        Console.WriteLine($"Confidence: {ConfidenceScore}");
        Console.WriteLine(RegisteredLang);

        Console.WriteLine("Intents:");
        foreach (JsonElement intent in conversationPrediction.GetProperty("intents").EnumerateArray())
        {
            string category = intent.GetProperty("category").GetString();
            float confidence = intent.GetProperty("confidenceScore").GetSingle();

            Console.WriteLine($"Category: {category}");
            Console.WriteLine($"Confidence: {confidence}");
            Console.WriteLine();

            if (category == "Arrived at shelf" && confidence > 0.75f)
            {
                await SynthesizeSpeech("you made it to the shelf");
                await SynthesizeSpeech("are you ready for the next item?");
                //await pickItemViewModel.RecognizeSpeechFromMic();
                
            }
        }

        Console.WriteLine("Entities:");
        foreach (JsonElement entity in conversationPrediction.GetProperty("entities").EnumerateArray())
        {
            Console.WriteLine($"Category: {entity.GetProperty("category").GetString()}");
            Console.WriteLine($"Text: {entity.GetProperty("text").GetString()}");
            Console.WriteLine($"Offset: {entity.GetProperty("offset").GetInt32()}");
            Console.WriteLine($"Length: {entity.GetProperty("length").GetInt32()}");
            Console.WriteLine($"Confidence: {entity.GetProperty("confidenceScore").GetSingle()}");
            Console.WriteLine();

            if (entity.TryGetProperty("resolutions", out JsonElement resolutions))
            {
                foreach (JsonElement resolution in resolutions.EnumerateArray())
                {
                    if (resolution.GetProperty("resolutionKind").GetString() == "DateTimeResolution")
                    {
                        Console.WriteLine($"Datetime Sub Kind: {resolution.GetProperty("dateTimeSubKind").GetString()}");
                        Console.WriteLine($"Timex: {resolution.GetProperty("timex").GetString()}");
                        Console.WriteLine($"Value: {resolution.GetProperty("value").GetString()}");
                        Console.WriteLine();
                    }
                }
            }
        }
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
