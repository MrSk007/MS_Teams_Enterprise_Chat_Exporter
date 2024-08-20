using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MS_Teams_Chat_Exporter.Constants;

namespace MS_Teams_Chat_Exporter.Data
{
    public static class DataProcessor
    {
        public static async Task FetchAPIData(string initialApiUrl, string outputFilePath, string bearerToken, string directoryPath, string key)
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

        public static async Task GenerateMessageJson(string filePath, string bearerToken)
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
                        if (execute.EndsWith(AppSettingsConstants.OneToOnePersonelChatsKey))
                        {
                            string id = conversation["threadProperties"]?["spaceThreadTopic"]?.ToString() + " " + conversation["version"]?.ToString();
                            string type = conversation["type"]?.ToString();
                            string messageAPIURL = conversation["messages"]?.ToString();
                            Console.WriteLine($"Conversation ID: {id} - Type: {type}");

                            string messageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), AppSettingsConstants.MessageFolderName);
                            string messageOutputFilePath = Path.Combine(messageDirectoryPath, $"{id}.json");

                            await FetchAPIData(messageAPIURL, messageOutputFilePath, bearerToken, messageDirectoryPath, AppSettingsConstants.MessageFolderName);
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
    }
}
