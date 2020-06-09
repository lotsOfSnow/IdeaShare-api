# IdeaShare API

This is an API for a simple publishing platform. It was built with .NET Core 3 and EF Core. Its features include:
* Creating and managing user account
* Authentication
* Creating, editing, deleting and rating articles
* Posting and deleting comments
* Basic Swagger documentation

## Getting Started
Use these instructions to get the project up and running.

### Prerequisites
You will need the following tools:

* [Visual Studio Code or Visual Studio 2019](https://visualstudio.microsoft.com/vs/) (version 16.3 or later)
* [.NET Core SDK 3](https://dotnet.microsoft.com/download/dotnet-core/3.1)
### Setup
Follow these steps to get your development environment set up:

  1. Install EF Core tools:
     ```
     dotnet tool install --global dotnet-ef
     ```
  2. Clone the repository:
     ```
     git clone https://github.com/lotsOfSnow/IdeaShare-api.git
     ```
  3. At the root directory, restore required packages by running:
     ```
     cd IdeaShare-api
     dotnet restore
     ```
  4. Next, build the solution by running:
     ```
     dotnet build
     ```
  5. Launch the API by running:
     ```
	 dotnet run --project .\src\Api\
	 ```
  6. Go to [https://localhost:5001/swagger](https://localhost:5001/swagger/index.html) in your browser to view the API.