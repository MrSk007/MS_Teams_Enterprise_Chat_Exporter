
using MS_Teams_Chat_Exporter.Core;

namespace MS_Teams_Chat_Exporter
{
    class Program
    {
		// options cmd
		
		
        static async Task Main(string[] args)
        {
			String argcmd = "";
			
			bool doPdf = true;
			bool doConv = true;
			bool doChats = true;
			bool doCont = false;
			
			if (args.Length > 0) {
				argcmd = args[0];
			}
			
			if ( argcmd == "pdf" ) {
				doConv = false;
				doChats = false;
			} else if ( argcmd == "nopdf" ) {
				doPdf = false;
			} else if ( argcmd == "cont" ) {
				doCont = true;
				doPdf = false;
			} else if ( argcmd == "" ) {
			} else {
				Console.WriteLine("");
				Console.WriteLine("MS_Teams_Enterprise_Channel_Exporter - V3");
				Console.WriteLine("-> JVI & MrSk007");
				Console.WriteLine("");
				Console.WriteLine("dotnet run [ <cmd> ]");
				Console.WriteLine("");
				Console.WriteLine("without cmd all elements are executed: retrieve last conversations - get the corresponding chats - create pdfs");
				Console.WriteLine("");
				Console.WriteLine("following cmds are supported");
				Console.WriteLine(" cont  : continue last run use conversations file if existing");
				Console.WriteLine(" cont  : continue last run use conversations file if existing - without pdf");
				Console.WriteLine(" pdf   : only create pdf from existing chats");
				Console.WriteLine(" nopdf : run normally but no pdf creation");
				Console.WriteLine("");
				Console.WriteLine("console log copy can be found in exportlog.txt");
				Console.WriteLine("");
				return;
				
			}
			
            ICoreProcessor _coreProcessor = new CoreProcessor();
            await _coreProcessor.Start(argcmd, doPdf, doConv, doChats, doCont);
        }
    }
}
