using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Teams_Chat_Exporter.Constants
{
    public struct AppSettingsConstants
    {
      public const string InitialApiUrl = "https://teams.microsoft.com/api/chatsvc/emea/v1/users/ME/conversations";

      public const string DataFolderName = "data";

      public const string MessageFolderName = "messages";

      public const string ExportPdfFolderName = "exports";

      public const string ConversationJsonFileName = "conversations.json";

      public const string OneToOnePersonelChatsKey = "@unq.gbl.spaces";
    }
}
