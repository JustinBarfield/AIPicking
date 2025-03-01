using Azure.AI.Language.Conversations;
using Azure.Core;
using Azure;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using AIPicking;

public class IntentViewModel : INotifyPropertyChanged
{
    private Uri endpoint = new Uri("https://seniordesignlanguage.cognitiveservices.azure.com/");
    private AzureKeyCredential credential = new AzureKeyCredential("59O1KpOwwOQwFTliUh931fyiATPPXJES1T5CJNg6dGAga7odm5G2JQQJ99BBACYeBjFXJ3w3AAAaACOGyRH3");

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

    public IntentViewModel()
    {
        client = new ConversationAnalysisClient(endpoint, credential);
        AnalyzeCommand = new RelayCommand(async () => await AnalyzeConversationAsync());
    }

    public ICommand AnalyzeCommand { get; }

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

        Console.WriteLine($"Top intent: {conversationPrediction.GetProperty("topIntent").GetString()}");

        Console.WriteLine("Intents:");
        foreach (JsonElement intent in conversationPrediction.GetProperty("intents").EnumerateArray())
        {
            Console.WriteLine($"Category: {intent.GetProperty("category").GetString()}");
            Console.WriteLine($"Confidence: {intent.GetProperty("confidenceScore").GetSingle()}");
            Console.WriteLine();
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

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
