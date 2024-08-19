
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using System.Net.Http;

namespace MS_Teams_Chat_Exporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string initialApiUrl = "https://teams.microsoft.com/api/chatsvc/emea/v1/users/ME/conversations";
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
            string outputFilePath = Path.Combine(directoryPath, "conversations.json");

            Console.WriteLine("Please enter your Bearer token: ");
            string bearerToken = Console.ReadLine();

            Console.WriteLine("Fetching Conversations ... ");
            await FetchAPIData(initialApiUrl, outputFilePath, bearerToken, directoryPath, "conversations");
            Console.WriteLine("Conversations Json exported");

            string filePath = outputFilePath;

            Console.WriteLine("Fetching One to One Personal Chats ... ");
            await GenerateMessageJson(filePath, bearerToken);
            Console.WriteLine("One to One Personal Chats Json exported ... ");

            Console.WriteLine("Do you want to Generate Chat PDF - Say : Yes or No");
            string choice = Console.ReadLine()?.ToLower();

            if (choice == "yes")
            {
                string messageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "messages");
                if (Directory.Exists(messageDirectoryPath))
                {
                    string[] files = Directory.GetFiles(messageDirectoryPath);

                    // Output the paths of the files
                    Console.WriteLine("Files in directory: " + messageDirectoryPath);

                    foreach (var file in files)
                    {
                        if(file.EndsWith("1723991179562.json"))
                        GeneratePDFFromJSON(file, Path.Combine(Directory.GetCurrentDirectory(), "exports"));
                    }
                }
                else
                {
                    Console.WriteLine("Thanks Existing ....");
                    await Task.Delay(1000);
                }
            }
        }

        static async Task FetchAPIData(string initialApiUrl, string outputFilePath, string bearerToken, string directoryPath, string key)
        {
            try
            {
                // Create the directory if it does not exist
                Directory.CreateDirectory(directoryPath);

                var allConversations = new JArray();
                string syncStateUrl = null;
                int retryCount = 0;
                const int maxRetries = 5;

                using (HttpClient client = new HttpClient())
                {
                    // Add the Bearer token to the request headers
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    string currentApiUrl = initialApiUrl;

                    while (!string.IsNullOrEmpty(currentApiUrl))
                    {
                        try
                        {
                            // Send GET request to the API
                            HttpResponseMessage response = await client.GetAsync(currentApiUrl);

                            if (response.IsSuccessStatusCode)
                            {
                                // Read response content
                                string data = await response.Content.ReadAsStringAsync();
                                JObject jsonResponse = JObject.Parse(data);

                                // Extract and accumulate conversations
                                var conversations = jsonResponse[key] as JArray;
                                if (conversations != null)
                                {
                                    allConversations.Merge(conversations);
                                }

                                // Save the syncState URL if it's the first request
                                if (syncStateUrl == null)
                                {
                                    syncStateUrl = jsonResponse["_metadata"]?["syncState"]?.ToString();
                                }

                                // Get the backwardLink to determine the next API call
                                currentApiUrl = jsonResponse["_metadata"]?["backwardLink"]?.ToString();

                                // If backwardLink is empty and syncStateUrl is available, use syncStateUrl
                                if (string.IsNullOrEmpty(currentApiUrl) && !string.IsNullOrEmpty(syncStateUrl))
                                {
                                    currentApiUrl = syncStateUrl;
                                    syncStateUrl = jsonResponse["_metadata"]?["syncState"]?.ToString();
                                    break;
                                }
                            }
                            else if (response.StatusCode == (System.Net.HttpStatusCode)429) // TooManyRequests
                            {
                                retryCount++;
                                if (retryCount > maxRetries)
                                {
                                    Console.WriteLine("Max retry attempts reached. Exiting...");
                                    break;
                                }
                                int delay = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff
                                Console.WriteLine($"Rate limit exceeded. Waiting for {delay / 1000} seconds before retrying...");
                                await Task.Delay(delay);
                            }
                            else
                            {
                                Console.WriteLine($"Error: {response.StatusCode}");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                            break;
                        }

                        // Small delay to prevent hammering the API too quickly
                        await Task.Delay(200);
                    }

                    // Write all accumulated conversations to a JSON file
                    string jsonFormatted = JsonConvert.SerializeObject(allConversations, Formatting.Indented);
                    await File.WriteAllTextAsync(outputFilePath, jsonFormatted);

                    Console.WriteLine($"Data has been written to {outputFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        static async Task GenerateMessageJson(string filePath, string bearerToken)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Read the JSON file into a string
                    string jsonData = File.ReadAllText(filePath);

                    // Parse the JSON data into a JArray
                    JArray conversations = JArray.Parse(jsonData);

                    // Loop through each conversation item
                    foreach (JObject conversation in conversations)
                    {
                        // Extract and print various properties
                        string execute = conversation["id"]?.ToString();

                        // Export only one to one Chat
                        if (execute.EndsWith("@unq.gbl.spaces"))
                        {
                            string id = conversation["threadProperties"]?["spaceThreadTopic"]?.ToString() + " " + conversation["version"]?.ToString();
                            string type = conversation["type"]?.ToString();
                            string messageAPIURL = conversation["messages"]?.ToString();
                            Console.WriteLine($"Conversation ID: {id} - Type: {type}");

                            string messageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"messages");
                            string messageOutputFilePath = Path.Combine(messageDirectoryPath, $"{id}.json");

                            await FetchAPIData(messageAPIURL, messageOutputFilePath, bearerToken, messageDirectoryPath, "messages");
                            await Task.Delay(1000);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"File not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        static void GeneratePDFFromJSON(string jsonFilePath, string pdfFilePath)
        {
            try
            {

                // Ensure the directory exists
                if (!Directory.Exists(pdfFilePath))
                {
                    Directory.CreateDirectory(pdfFilePath);
                }

                var fileName = jsonFilePath.Split("\\")[jsonFilePath.Split("\\").Length - 1].Replace(".json","");

                // Load JSON data
                var jsonData = File.ReadAllText(jsonFilePath);
                var chatEntries = JArray.Parse(jsonData);


                // Create PDF file path
                var pdfFileExportPath = Path.Combine(pdfFilePath, $"{fileName}.pdf");

                using (var pdfWriter = new PdfWriter(pdfFileExportPath))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    string outputHtml = string.Empty;
                    foreach (var entry in chatEntries)
                    {
                        // Prepare HTML content
                        var htmlContent = "<div style='padding: 10px 5px 5px 10px;text-wrap: pretty;'>";

                        // Name
                        var name = entry["fromDisplayNameInToken"]?.ToString() ?? entry["imdisplayname"]?.ToString() ?? "Unknown";
                        var time = Convert.ToDateTime(entry["originalarrivaltime"]?.ToString());
                        htmlContent += $"<p><strong>Name:</strong> {name} - {time.ToString("g")}</p>";

                        // Chat content
                        var content = entry["content"]?.ToString() ?? "No content";
                        htmlContent += $"<p><strong>Chat content:</strong> {content}</p>";

                        // Chat files links
                        var files = entry["properties"]?["files"]?.ToString() ?? "No files";
                        htmlContent += $"<p><strong>Chat files links:</strong> {files}</p>";

                        // Add a separator line
                        htmlContent += "<hr style='margin-bottom: 10px;'/>";

                        htmlContent += "</div>";

                        outputHtml += htmlContent;
                    }

                    // Convert HTML content to PDF and add it to the existing PDF document
                    using (var tempHtmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(outputHtml)))
                    {
                        HtmlConverter.ConvertToPdf(tempHtmlStream, pdfDocument);
                    }
                    Console.WriteLine("PDF generated successfully at: " + Path.GetFullPath(pdfFileExportPath));
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
