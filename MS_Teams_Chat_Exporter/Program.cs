
using MS_Teams_Chat_Exporter.Core;

namespace MS_Teams_Chat_Exporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ICoreProcessor _coreProcessor = new CoreProcessor();
            await _coreProcessor.Start();
        }
    }
}
