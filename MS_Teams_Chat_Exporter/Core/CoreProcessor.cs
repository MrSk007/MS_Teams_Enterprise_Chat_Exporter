using MS_Teams_Chat_Exporter.Constants;
using MS_Teams_Chat_Exporter.Data;
using MS_Teams_Chat_Exporter.Utils;

namespace MS_Teams_Chat_Exporter.Core
{
    public interface ICoreProcessor
    {
        Task Start(string argcmd, bool doPdf,  bool doConv, bool doChats,  bool doCont);
    }
    public sealed class CoreProcessor: ICoreProcessor
    {
        public CoreProcessor() {
            PrintAppTilte.PrintColorFullTitle();
        }

        public async Task Start(string argcmd, bool doPdf,  bool doConv, bool doChats,  bool doCont)
        {
			bool freshRun = true;
			
			// init log
			string logfile = Path.Combine(Directory.GetCurrentDirectory(), "exportlog.txt");

			if ( File.Exists( logfile ) ) {
				int i = 0;
				string oldfile = logfile;
				while ( File.Exists( oldfile ) ) {
					i = i + 1;
					oldfile = logfile + "-" + i.ToString().Trim();
				}
				File.Move( logfile, oldfile );
			}
			if ( File.Exists( logfile ) ) {
				Console.WriteLine(">>> ERROR: Could not move old log files");
				return;
			}
			DataProcessor.logwriter = File.AppendText(logfile);


			// get dirs & files
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), AppSettingsConstants.DataFolderName);


			string outputFilePath = DataProcessor.createFileName( directoryPath, AppSettingsConstants.ConversationJsonFileName, ".json", false );

            string bearerTokenFile = Path.Combine(directoryPath, AppSettingsConstants.BearerTokenFileName);


			
			
			
			if ( doConv ) {

				DataProcessor.logWriteLine("Fetching Conversations ... ");

				string choice = "";
				if ( File.Exists( outputFilePath ) ) {
					if ( doCont ) {
						freshRun = false;
					} else {
						Console.WriteLine("Do you want to overwrite conversations??? - Say : Yes or No");

						choice = "";
						choice = Console.ReadLine()?.ToLower();
						if ( choice != "yes" ) {
							return;
						}
					}
				}
				
				if ( freshRun ) {
					DataProcessor.logWriteLine("Fetching full response ... ");
					await DataProcessor.FetchAPIData(AppSettingsConstants.InitialApiUrl, outputFilePath, bearerTokenFile, directoryPath, "conversations" );


					DataProcessor.logWriteLine("----------- Conversations Json exported -------------");
				} else {
					DataProcessor.logWriteLine("----------- use existing conversations -------------");
				}
					
			}

			if ( doChats ) {
				DataProcessor.logWriteLine("Fetching Recent Chats ... ");

				await DataProcessor.GenerateMessageJson(outputFilePath, directoryPath, bearerTokenFile);

				DataProcessor.logWriteLine("--------------- Chats Json exported ... -------------------");
			}
			
			if ( doPdf ) {
					await DataProcessor.doPdf( directoryPath );
				// Console.WriteLine("Do you want to Generate Chat PDF - Say : Yes or No");

				// choice = "";
				// choice = Console.ReadLine()?.ToLower();

				// if (choice == "yes")

			}
			
			DataProcessor.logWriteLine("Your Chats Exported Successfully....");
			DataProcessor.logWriteLine("");
			DataProcessor.logWriteLine(">>>DONE");
			await Task.Delay(1000);
        }	
    }
}
