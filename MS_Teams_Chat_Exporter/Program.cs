
using MS_Teams_Chat_Exporter.Core;

namespace MS_Teams_Chat_Exporter
{
    class Program
    {
        private readonly ICoreProcessor _coreProcessor;

        Program()
        {
            _coreProcessor = new CoreProcessor();
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program._coreProcessor.Start();
        }
    }
}
