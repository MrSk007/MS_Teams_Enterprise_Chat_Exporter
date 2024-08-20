using MS_Teams_Chat_Exporter.Constants;
using MS_Teams_Chat_Exporter.Data;
using MS_Teams_Chat_Exporter.Utils;

namespace MS_Teams_Chat_Exporter.Core
{
    public interface ICoreProcessor
    {
        Task Start();
    }
    public sealed class CoreProcessor: ICoreProcessor
    {
        public CoreProcessor() {
            PrintAppTilte.PrintColorFullTitle();
        }

        public async Task Start()
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), AppSettingsConstants.DataFolderName);
            string outputFilePath = Path.Combine(directoryPath, AppSettingsConstants.ConversationJsonFileName);

            Console.WriteLine("Please enter your Bearer token to fetch your teams conversations : ");

            string bearerToken = Console.ReadLine();

            Console.WriteLine("Fetching Conversations ... ");

            await DataProcessor.FetchAPIData(AppSettingsConstants.InitialApiUrl, outputFilePath, bearerToken, directoryPath, "conversations");

            Console.WriteLine("----------- Conversations Json exported -------------");

            Console.WriteLine("Fetching One to One Personal Chats ... ");

            await DataProcessor.GenerateMessageJson(outputFilePath, bearerToken);

            Console.WriteLine("--------------- One to One Personal Chats Json exported ... -------------------");

            Console.WriteLine("Do you want to Generate Chat PDF - Say : Yes or No");

            string choice = Console.ReadLine()?.ToLower();

            if (choice == "yes")
            {
                string messageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), AppSettingsConstants.MessageFolderName);
                if (Directory.Exists(messageDirectoryPath))
                {
                    string[] files = Directory.GetFiles(messageDirectoryPath);

                    // Output the paths of the files
                    Console.WriteLine("Files in directory: " + messageDirectoryPath);

                    foreach (var file in files)
                    {
                        PrintPDF.GeneratePDFFromJSON(file, Path.Combine(Directory.GetCurrentDirectory(), AppSettingsConstants.ExportPdfFolderName));
                    }
                }
                else
                {
                    Console.WriteLine("Your Chats Exported Successfully....");
                    await Task.Delay(1000);
                }
            }
        }
    }
}
