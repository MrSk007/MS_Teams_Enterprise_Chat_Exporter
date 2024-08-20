using System;
using Colorful;
using Console = Colorful.Console;

namespace MS_Teams_Chat_Exporter.Utils
{
    public static class PrintAppTilte
    {
        public static void PrintColorFullTitle()
        {
            Figlet figlet = new Figlet();
            Console.WriteLine(figlet.ToAscii("Author : MrSk007"));
        }
    }
}
