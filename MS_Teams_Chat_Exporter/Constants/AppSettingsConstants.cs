/*
TODO:
- check attachments/etc --> image from HTML?
- TEST: ignore conversations with threadProperties":       "isdeleted": "True",
- adapt pdf creator for other types
- PDF-Creation reactions from chats 

DONE 31.10.
- Remove space for conversations file namespace
- check for other thread title
- export also multiperson chats
- export other teams channels

DONE 1.11.
- create PDF for all
- Added final Trim for filenames --> avoid spaces on filenames end/beginning
- allow pdfs to be done separately
- add log --> exportlog.txt and move old logs to -x
- remove no links if empty
- PDF-Creation: handled ThreadActivity/AddMember as short text
- PDF-Creation:  ThreadActivity/DeleteMember ignore
- add name of person guess to oneToOne chats + identify yourself (from id and name)
- add exit option if files are there and no parameter given
*/

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

      public const string TestApiUrl = "https://teams.microsoft.com/api/chatsvc/emea/v1/users/ME/conversations";

      public const string DataFolderName = "export";		// base folder

      public const string TestFolderName = "test";

      public const string MessageFolderName = "messages";
      public const string ChannelFolderName = "channel";
      public const string MultiFolderName = "chats";

      public const string ExportPdfFolderName = "pdf";

      public const string ConversationJsonFileName = "conversations";

      public const string BearerTokenFileName = "bt.txt";

      public const string OneToOnePersonelChatsKey = "@unq.gbl.spaces";
      public const string TeamsaChannelChatsKey = "@thread.skype";
      public const string TeamsStdChannelChatsKey = "@thread.tacv2";
      public const string MultiChatsKey = "@thread.v2";
      public const string AnnotationsChatsKey = ":annotations";
      public const string MeChatKey = "48:notes";
	  
	 
	  // Also: @thread.tacv2
    }
}
