using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

// https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/

string aoaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!;
string aoaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!;
string aoaiModel = "gpt-35-turbo";

// Initialize the kernel
IKernel kernel = Kernel.Builder
    .WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .WithAzureChatCompletionService(aoaiModel, aoaiEndpoint, aoaiApiKey)
    .Build();

// Create a new chat
IChatCompletion ai = kernel.GetService<IChatCompletion>();
ChatHistory chat = ai.CreateNewChat("you are trying to read a CSV");
StringBuilder builder = new();

string settingsString = File.ReadAllText("ressources/settings.json");
IEnumerable<String> contentExample = File.ReadLines("ressources/example1.csv").Take(5);

JsonElement settings = (JsonElement) JsonSerializer.Deserialize<object>(settingsString);

StringBuilder data = new();
foreach (string line in contentExample)
{
    data.AppendLine(line);
}
chat.AddUserMessage("you have these data : " + data.ToString());

foreach(object info in settings.GetProperty("helpInfo").EnumerateArray())
{
    chat.AddUserMessage(info.ToString());
}

chat.AddUserMessage("match these column name with the of the CSV : " + settings.GetProperty("possibleColumns").ToString());
chat.AddUserMessage("i want your answer in this format : " + settings.GetProperty("outputFormat").ToString());

builder.Clear();
await foreach (string message in ai.GenerateMessageStreamAsync(chat))
{
    builder.Append(message);
}
chat.AddAssistantMessage(builder.ToString());

try
{
    JsonElement answer = (JsonElement)JsonSerializer.Deserialize<object>(builder.ToString());
    /*Console.WriteLine(answer.ToString());*/
    List<JsonObject> csv = readCSV("ressources/example1.csv", answer);
    foreach(JsonObject line in csv)
    {
        Console.WriteLine(line.ToString());
    }
}
catch(Exception e)
{
    Console.WriteLine(builder.ToString());
}

List<JsonObject> readCSV(string path, JsonElement output)
{

    IEnumerable<String> fileContent = File.ReadLines(path);

    string separtator = output.GetProperty("separator").ToString();
    string[] columns = output.GetProperty("columns").EnumerateArray().Select(x => x.ToString()).ToArray();
    
    if(output.GetProperty("hasHeader").GetBoolean())
        fileContent = fileContent.Skip(1);

    List<JsonObject> employees = new List<JsonObject>();
    foreach (string line in fileContent)
    {
        string[] values = line.Split(separtator);
        JsonObject lineResult = new JsonObject();
        for (int i = 0; i < columns.Length; i++)
        {
            lineResult.Add(columns[i], values[i]);
        }
        employees.Add(lineResult);
    }

    return employees;
}

builder.Clear();

