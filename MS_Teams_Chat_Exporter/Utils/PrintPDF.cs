using iText.Html2pdf;
using iText.Kernel.Pdf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_Teams_Chat_Exporter.Data;


namespace MS_Teams_Chat_Exporter.Utils
{
    public static class PrintPDF
    {
       public static void GeneratePDFFromJSON(string jsonFilePath, string pdfFilePath)
        {
            try
            {

                // Ensure the directory exists
                if (!Directory.Exists(pdfFilePath))
                {
                    Directory.CreateDirectory(pdfFilePath);
                }

                var fileName = jsonFilePath.Split("\\")[jsonFilePath.Split("\\").Length - 1].Replace(".json", "").Trim();

                // Load JSON data
                var jsonData = File.ReadAllText(jsonFilePath);
                var chatEntries = JArray.Parse(jsonData);


                // Create PDF file path
                var pdfFileExportPath = Path.Combine(pdfFilePath, $"{fileName}.pdf");

                using (var pdfWriter = new PdfWriter(pdfFileExportPath))
                using (var pdfDocument = new PdfDocument(pdfWriter))
                {
                    string outputHtml = string.Empty;
                    foreach (var entry in chatEntries)
                    {
						var htmlContent = "";
						
						// handle different types of entries
						string mt = entry["messagetype"]?.ToString();
						
						if ( string.IsNullOrEmpty( mt ) ) {
							continue;
						} else if ( mt == "ThreadActivity/DeleteMember" ) {
							continue;
						} else if ( mt == "ThreadActivity/AddMember" ) {
							htmlContent = "<div style='padding: 10px 5px 5px 10px;text-wrap: pretty;'>";
							var time = Convert.ToDateTime(entry["originalarrivaltime"]?.ToString());
							htmlContent += $"<p><strong>Person added</strong> - {time.ToString("g")}</p>";
							// Add a separator line
							htmlContent += "<hr style='margin-bottom: 10px;'/>";
							htmlContent += "</div>";

						} else {

							// Prepare HTML content
							htmlContent = "<div style='padding: 10px 5px 5px 10px;text-wrap: pretty;'>";

							// Name
							var name = entry["fromDisplayNameInToken"]?.ToString() ?? entry["imdisplayname"]?.ToString() ?? "Unknown";
							var time = Convert.ToDateTime(entry["originalarrivaltime"]?.ToString());
							htmlContent += $"<p><strong>Name:</strong> {name} - {time.ToString("g")}</p>";

							// Chat content
							var content = entry["content"]?.ToString() ?? "No content";
	//                        htmlContent += $"<p><strong>Chat content:</strong> {content}</p>";
							htmlContent += $"<p>{content}</p>";

							// Chat files links
							var files = entry["properties"]?["files"]?.ToString();
							if ( ( ! string.IsNullOrEmpty( files ) ) && ( ( files != "[]" ) ) )  {
								htmlContent += $"<p><strong>++ Chat files links:</strong> {files}</p>";

							}
							// Add a separator line
							htmlContent += "<hr style='margin-bottom: 10px;'/>";

							htmlContent += "</div>";
						}
                        outputHtml += htmlContent;
                    }

                    // Convert HTML content to PDF and add it to the existing PDF document
                    using (var tempHtmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(outputHtml)))
                    {
                        HtmlConverter.ConvertToPdf(tempHtmlStream, pdfDocument);
                    }
                    DataProcessor.logWriteLine("PDF generated successfully at: " + Path.GetFullPath(pdfFileExportPath));
                }

            }
            catch (Exception ex)
            {
                DataProcessor.logWriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
