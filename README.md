# MS Teams Enterprise Chat Exporter

**MS Teams Enterprise Chat Exporter** is a C# console application that allows users to export their Microsoft Teams chat conversations to JSON and PDF formats. The application fetches chat data using the Microsoft Teams API and provides an option to generate PDFs from the exported JSON data.

## Portable Exe for Windows x64
[Google Drive URL]([https://link-url-here.org](https://drive.google.com/file/d/1qWSJwoduNCV6uZtPJ4zDhBaWSk9ZNSw1/view?usp=sharing))

## Features

- **Export Conversations:** Fetch and export your Microsoft Teams conversations to a JSON file.
- **One-to-One Personal Chats:** Extract specific one-to-one chats from the exported conversations.
- **Generate PDF:** Convert chat data from JSON format to a well-formatted PDF.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download)
- [Bearer Token](#how-to-get-your-bearer-token) for Microsoft Teams API access.

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/ms_teams_enterprise_chat_exporter.git
   cd ms_teams_enterprise_chat_exporter

## Usage

Run the application:
```bash
dotnet run
```
Enter your Bearer token when prompted.

The application will fetch your conversations and save them in a conversations.json file.

You can choose to generate PDFs from the fetched chat data. If you opt for this, PDFs will be saved in the exports directory.

## Code Structure

- **Program.cs:** The main entry point of the application. Handles user input, API calls, and PDF generation.
- **FetchAPIData:** A method to fetch data from the Microsoft Teams API and save it to a JSON file.
- **GenerateMessageJson:** Extracts specific one-to-one personal chats from the fetched conversations.
- **GeneratePDFFromJSON:** Converts chat data in JSON format to a well-formatted PDF using the iText library.

## How to Get Your Bearer Token

To get your Bearer token, follow these steps:

1. Log in to Microsoft Teams.
2. Open the Developer Tools in your browser (usually by pressing `F12`).
3. Go to the `Network` tab.
4. Perform any action in Teams (like sending a message).
5. Look for a request with an `Authorization` header.
6. Copy the Bearer token from the request headers.

## Dependencies

- **iText 7:** Used for generating PDFs from HTML content.
- **Newtonsoft.Json:** Used for handling JSON data.

## Contributing

Contributions are always welcome! Whether it's bug fixes, feature suggestions, or improvements to the documentation, your help is greatly appreciated. Here are a few ways you can contribute:

- **Submit a Pull Request:** If you've made a change that you think could benefit others, feel free to submit a pull request. Make sure to follow the project's coding standards and include detailed descriptions of your changes.
- **Report Issues:** Found a bug? Have an idea for a new feature? Open an issue to let us know.
- **Improve Documentation:** If you notice any gaps or errors in the documentation, you can contribute by improving the documentation.

### Support the Project

If you find this project useful and want to support its continued development, consider sponsoring me on GitHub. Your support helps keep this project going and allows me to dedicate more time to improving it.

[![Sponsor Me on GitHub](https://img.shields.io/badge/Sponsor%20Me%20on%20GitHub-ffaa00?style=flat&logo=github)](https://github.com/sponsors/MrSk007)

Thank you for your support!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.








