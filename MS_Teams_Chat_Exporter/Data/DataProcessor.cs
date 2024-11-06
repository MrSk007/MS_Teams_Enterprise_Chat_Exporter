using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;using System.Threading.Tasks;
using MS_Teams_Chat_Exporter.Constants;
using MS_Teams_Chat_Exporter.Utils;



namespace MS_Teams_Chat_Exporter.Data
{
    public static class DataProcessor
    {
        public static bool stopIt = false;
		
        public static StreamWriter logwriter;
		

		public static void logWriteLine( string logline ) 
		{
			Console.WriteLine( logline );
			 if (logwriter is not null) {
				 logwriter.WriteLine( $"{DateTime.Now.ToLongTimeString()} : {logline}" );
				 logwriter.Flush();
			 }
			
		}

		public static string createFileName( string basepath, string basename, string baseext, bool doIndex )
		{
			string basefilename = Path.Combine(basepath, basename).Trim();
			baseext = baseext.Trim();
			
			if ( doIndex ) {
				int i = 0;
				string nextfile = basefilename;
				while ( File.Exists( nextfile+baseext ) ) {
					i = i+1;
					nextfile = basefilename + "-" + i.ToString().Trim();
				}
				basefilename = nextfile;
			}
			return basefilename+baseext;
			
		}

		
		/******************************************************************************************/
		/******************************************************************************************/
		/*****                   FETCHAPIDATA                                                 *****/
		/******************************************************************************************/
		/******************************************************************************************/
		
		public static async Task FetchAPIData(string initialApiUrl, string outputFilePath, string bearerTokenFile, string directoryPath, string key)
        {
            try
            {
                // Create the directory if it does not exist
                Directory.CreateDirectory(directoryPath);

                var allConversations = new JArray();
                var allResponses = new JArray();
                string syncStateUrl = null;
                int retryCount = 0;
                const int maxRetries = 8;
				
				string bearerToken = "";
				if (File.Exists(bearerTokenFile)) {
                    // Read the JSON file into a string
                    bearerToken = File.ReadAllText(bearerTokenFile);				
				}
				
                using (HttpClient client = new HttpClient())
                {
                    // Add the Bearer token to the request headers
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    string currentApiUrl = initialApiUrl;

                    while (!string.IsNullOrEmpty(currentApiUrl))
                    {
						if ( string.IsNullOrEmpty(bearerToken) ) {
							while ( string.IsNullOrEmpty(bearerToken) ) {
								logWriteLine("");
								logWriteLine(">>>>>>>>>>> !!!!! <<<<<<<<<<<<< ");
								logWriteLine("Please enter your Bearer token to fetch your teams conversations (or No): ");
								bearerToken = Console.ReadLine();
							}
							
							if (  bearerToken.ToLower() == "no" ) {
								logWriteLine("Exiting...");
								stopIt = true;
								break;
							}
							
							await File.WriteAllTextAsync(bearerTokenFile, bearerToken);
							client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
							logWriteLine($"... continuing ...");
						}

                        try
                        {
                            // Send GET request to the API
                            HttpResponseMessage response = await client.GetAsync(currentApiUrl);

                            if (response.IsSuccessStatusCode)
                            {
                                // Read response content
                                string data = await response.Content.ReadAsStringAsync();
                                JObject jsonResponse = JObject.Parse(data);

								if ( string.IsNullOrEmpty(key) ) 
								{
                                    logWriteLine("Add full json response");
										allConversations.Add(jsonResponse);
/* 									// Write all accumulated conversations to a JSON file
									string json2Formatted = JsonConvert.SerializeObject(jsonResponse, Formatting.Indented);
									await File.WriteAllTextAsync(outputFilePath, json2Formatted);
                                    logWriteLine("DONE Write full json response");
									currentApiUrl = null;
 */								}
								else
								{
									// Extract and accumulate conversations
									var conversations = jsonResponse[key] as JArray;
									if (conversations != null)
									{
										allConversations.Merge(conversations);
									}
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
                                    logWriteLine("Max retry attempts reached. Exiting...");
                                    break;
                                }
                                int delay = (int)Math.Pow(2, retryCount) * 1000; // Exponential backoff
                                logWriteLine($"Rate limit exceeded. Waiting for {delay / 1000} seconds before retrying...");
                                await Task.Delay(Math.Min(delay,64000));
                            }

                            else if (response.StatusCode == (System.Net.HttpStatusCode)401) // UNauthorized
                            {
								// get new bearer token
								bearerToken = "";
                                logWriteLine($"Bearer token expired getting a new one");
								await File.WriteAllTextAsync(bearerTokenFile, bearerToken);
                            }

                            else
                            {
                                logWriteLine($"Error: {response.StatusCode}");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logWriteLine($"An error occurred: {ex.Message}");
                            break;
                        }

                        // Small delay to prevent hammering the API too quickly
                        await Task.Delay(200);
                    }

                    // Write all accumulated conversations to a JSON file
                    string jsonFormatted = JsonConvert.SerializeObject(allConversations, Formatting.Indented);
					
                    await File.WriteAllTextAsync(outputFilePath, jsonFormatted);

                    logWriteLine($"Data has been written to {outputFilePath}");
                }
            }
            catch (Exception ex)
            {
                logWriteLine($"An error occurred: {ex.Message}");
            }
        }

		/******************************************************************************************/
		/******************************************************************************************/
		/*****                   GENERATEMESSAGEJSON                                          *****/
		/******************************************************************************************/
		/******************************************************************************************/

        public static async Task GenerateMessageJson(string inputFile, string outPath, string bearerTokenFile)
        {
            try
            {
                if (File.Exists(inputFile))
                {
                    // Read the JSON file into a string
                    string jsonData = File.ReadAllText(inputFile);

                    // Parse the JSON data into a JArray
                    JArray conversations = JArray.Parse(jsonData);

					string messageDirectoryPath = Path.Combine(outPath, AppSettingsConstants.MessageFolderName);
					Directory.CreateDirectory(messageDirectoryPath);

					string channelDirectoryPath = Path.Combine(outPath, AppSettingsConstants.ChannelFolderName);
					Directory.CreateDirectory(channelDirectoryPath);

					string multiDirectoryPath = Path.Combine(outPath, AppSettingsConstants.MultiFolderName);
					Directory.CreateDirectory(multiDirectoryPath);

					string regSearch = new string(Path.GetInvalidFileNameChars());
					Regex rg = new Regex(string.Format("[{0}]", Regex.Escape(regSearch)));


					String mename = "";
					String mefrom = "";
					
					// initial loop to find name and id of current user
                    foreach (JObject conversation in conversations)
                    {
						string deleted =  conversation["threadProperties"]?["isdeleted"]?.ToString().Trim();
						if ( ( ! string.IsNullOrEmpty( deleted ) ) && ( deleted == "True" ) ) {
							continue;
						}
						
                        // Extract and print various properties
                        string execute = conversation["id"]?.ToString();

						if ( execute == AppSettingsConstants.MeChatKey ) {
							if ( mename != "" ) {
								logWriteLine("");
								logWriteLine(">>>>>>>>>>> !!!!! <<<<<<<<<<<<< ");
								logWriteLine($"ERROR Duplicate identity found: {conversation["lastMessage"]?["imdisplayname"]?.ToString()} - From: {conversation["lastMessage"]?["from"]?.ToString()}");
								logWriteLine("");
								return;
							}
							mename = conversation["lastMessage"]?["imdisplayname"]?.ToString();
							mefrom = conversation["lastMessage"]?["from"]?.ToString();
							logWriteLine("");
							logWriteLine("Name and Identity found ");
                            logWriteLine($"Name          : {mename}");
                            logWriteLine($"ID URL (from) : {mefrom}");
							logWriteLine("");
						}

					}
					
                    // Loop through each conversation item
                    foreach (JObject conversation in conversations)
                    {
						string deleted =  conversation["threadProperties"]?["isdeleted"]?.ToString().Trim();
						if ( ( ! string.IsNullOrEmpty( deleted ) ) && ( deleted == "True" ) ) {
							continue;
						}
						
                        // Extract and print various properties
                        string execute = conversation["id"]?.ToString();

						string type =  conversation["threadProperties"]?["productThreadType"]?.ToString().Trim();
						if ( string.IsNullOrEmpty( type ) ) {
							type = conversation["type"]?.ToString().Trim();
						}

                        // Export one to one Chat and me chat
                        if ( (execute.EndsWith(AppSettingsConstants.OneToOnePersonelChatsKey)) || (execute.EndsWith(AppSettingsConstants.MeChatKey)) )
                        {
                            string top = conversation["threadProperties"]?["spaceThreadTopic"]?.ToString().Trim();
							if ( string.IsNullOrEmpty( top ) ) {
								top = conversation["threadProperties"]?["topicThreadTopic"]?.ToString().Trim();
							}

							// try to guess name of onetoone chat partner if last message is not from me - special case me chat
                            string lfrom = conversation["lastMessage"]?["from"]?.ToString().Trim();
							if ( string.IsNullOrEmpty( top ) ) {
								if ( ( ! string.IsNullOrEmpty( lfrom ) ) && ( ( lfrom != mefrom ) || (execute.EndsWith(AppSettingsConstants.MeChatKey)) ) )  {
									top = conversation["lastMessage"]?["imdisplayname"]?.ToString().Trim();
								}
							}
							
							if ( string.IsNullOrEmpty( top ) ) {
								top = " " + "ID: " + conversation["version"]?.ToString().Trim();
							} else {
								top = top + " (ID: " + conversation["version"]?.ToString().Trim() + ")";
							}
							top = rg.Replace(top, "").Trim();

                            string messageAPIURL = conversation["messages"]?.ToString();
                            logWriteLine($"Conversation O2O ID: {top} - Type: {type}");

                            string messageOutputFilePath = createFileName( messageDirectoryPath, top, ".json", true );

                            await FetchAPIData(messageAPIURL, messageOutputFilePath, bearerTokenFile, messageDirectoryPath, "messages" );
                            await Task.Delay(1000);
                        }
/* */
                        // Export also OLD Teams & Channels
                        else if ( (execute.EndsWith(AppSettingsConstants.TeamsaChannelChatsKey) || (execute.EndsWith(AppSettingsConstants.AnnotationsChatsKey)) || (execute.EndsWith(AppSettingsConstants.TeamsStdChannelChatsKey))) ) 
                        {
                            string top = conversation["threadProperties"]?["spaceThreadTopic"]?.ToString().Trim();
							if ( string.IsNullOrEmpty( top ) ) {
								top = conversation["threadProperties"]?["topicThreadTopic"]?.ToString().Trim();
							}

							if ( string.IsNullOrEmpty( top ) ) {
								top = " " + type + " ID: " + conversation["version"]?.ToString().Trim();
							} else {
								top = top + " (" + type + " ID: " + conversation["version"]?.ToString().Trim() + ")";
							}
							top = rg.Replace(top, "").Trim();

                            string messageAPIURL = conversation["messages"]?.ToString();
                            logWriteLine($"Conversation TCA ID: {top} - Type: {type}");

                            string channelOutputFilePath = createFileName( channelDirectoryPath, top, ".json", true );

                            await FetchAPIData(messageAPIURL, channelOutputFilePath, bearerTokenFile, channelDirectoryPath, "messages" );
// getting full JSON                            await FetchAPIData(messageAPIURL, channelOutputFilePath, bearerTokenFile, channelDirectoryPath, "");
                            await Task.Delay(1000);
                        }
                        // Export also OLD Teams & Channels
                        else if (execute.EndsWith(AppSettingsConstants.MultiChatsKey))
                        {
                            string top = conversation["threadProperties"]?["topic"]?.ToString().Trim();
							if ( string.IsNullOrEmpty( top ) ) {
								top = type + " ID: " + conversation["version"]?.ToString().Trim();
							} else {
								top = top + " (" + type + " ID: " + conversation["version"]?.ToString().Trim() + ")";
							}
							top = rg.Replace(top, "").Trim();

                            string messageAPIURL = conversation["messages"]?.ToString();
                            logWriteLine($"Conversation Mul ID: {top} - Type: {type}");

                            string multiOutputFilePath = createFileName( multiDirectoryPath, top, ".json", true );

                            await FetchAPIData(messageAPIURL, multiOutputFilePath, bearerTokenFile, multiDirectoryPath, "messages" );
                            await Task.Delay(1000);
                        } else {
                            string version = conversation["version"]?.ToString().Trim();
							logWriteLine("");
							logWriteLine(">>>>>>>>>>> !!!!! <<<<<<<<<<<<< ");
                            logWriteLine($"IGNORED UNKNOWN Conversation Version: {version} - Type: {execute}");
							logWriteLine("");
						}
						
						if ( stopIt ) {
							break;
						}
                    }
                }
                else
                {
                    logWriteLine($"File not found: {inputFile}");
                }
            }
            catch (Exception ex)
            {
                logWriteLine($"An error occurred: {ex.Message}");
            }
        }
		
		
		/******************************************************************************************/
		/******************************************************************************************/
		/*****                   PDF                                                          *****/
		/******************************************************************************************/
		/******************************************************************************************/

        public static async Task GeneratePdf(string outPath, string subdir)
        {
			if ( stopIt ) {
				return;
			}
            try
            {
				string destdir = subdir.Trim() + "-" + AppSettingsConstants.ExportPdfFolderName;
				string sourcePath = Path.Combine(outPath, subdir);
				 // Create the directory if it does not exist
			   if (Directory.Exists(sourcePath))
				{
					string[] files = Directory.GetFiles(sourcePath);

					// Output the paths of the files
					logWriteLine("Files in directory: " + sourcePath);

					foreach (var file in files)
					{
						PrintPDF.GeneratePDFFromJSON(file, Path.Combine(outPath, destdir));
					}
				}
				else
				{
					logWriteLine("Error -  Directory not found : " + sourcePath );
					await Task.Delay(1000);
				}
			}
			catch (Exception ex)
			{
				logWriteLine($"An error occurred: {ex.Message}");
			}

			// Small delay to prevent hammering the API too quickly
			await Task.Delay(200);
		}

        public static async Task doPdf(string directoryPath) {
			await DataProcessor.GeneratePdf( directoryPath, AppSettingsConstants.MessageFolderName );

			await DataProcessor.GeneratePdf( directoryPath, AppSettingsConstants.ChannelFolderName );

			await DataProcessor.GeneratePdf( directoryPath, AppSettingsConstants.MultiFolderName );

				
		}

    }
}
