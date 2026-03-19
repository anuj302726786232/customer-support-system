SuuportDeskAPI

Project Overview:

This project is a backend application built using .NET V8 Core (C#) with MySQL(MariaDb) as the database. It is designed to handle scalable and efficient data operations while providing a robust API/service layer.

The application follows a structured architecture to ensure maintainability, separation of concerns, and ease of future enhancements.

Tech Stack Used:

Backend Framework: .NET Core (C#)

Database: MySQL(MariaDb)

ORM: Entity Framework Core V6

API Type: RESTful APIs

Development Tools: Visual Studio

Version Control: Git

Steps to Run the Project Locally
Prerequisites:

Make sure you have the following installed:

.NET SDK (version 8.0 or above)

MySQL Server OR MariaDb V12.3

Git (optional)

1. Clone the Repository
git clone https://github.com/anuj302726786232/customer-support-system.git

cd customer-support-system

3. Configure Database:
   
Update the connection string in appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=your_database_name;user=root;password=your_password;"
}

4. Apply Migrations (if using Entity Framework)
Add-Migration MigrationName
Update-Database

5. Run the Application:
   
dotnet run

5. Access the API:

The application will typically run at:

https://localhost:44305/swagger/index.html(Documentation)

http://localhost:44306/swagger/index.html
