# RaceDay Event Management System
### Student Number
ST10478144
## Project Description
RaceDAy RaceDay Event Management System is a system that creates a race events. In these race events are organisers, participants (Users), event types, event categories, race results awards and enrolllments to participate in the race. 
## User Roles
### Organiser
- Creates an account and logs in.
- View and edit their account
- Account enables them to Create, edit and delete events.
- Is able to manage event categories.
- Can view event enrolments.
- Captures participant results.
- Is allowed to view information relating to events that they manage
### Participant
- Create an account and log in.
- View and edit their account
- Browse through the system to view available events.
- Enter an event and select a category they wish to participate in.
- View their own enrolments.
- Track their own race results and performance history.
- Track their own awards according to their finishing position
## Part 1- ERD- API Endpoint Plan- SQL Database Script
## Repository Structure
In the /docs folder there are two pdfs:
- ERD Diagram called: PROG6212_ ST10478144_ POE_ RaceDay ERD Diagram
- API Endpoint called: RaceDay_ API Endpoint Planning
And then there is an sql code called: RaceDay Database SQLQuery
## Database Setup
You need SQL Server Management Studio 2022 and later software. Clone the repository and you will be able to access the files including the sql code. Open the SQL code using SQL Server Management Studio 2022. Once the code opens and fill the query page, then click on the execute button and it will execute the code. 
## CI/CD
The Github CI checks if all files are firstly in their correct folder and there after it checks the file that are in the folder wether or not they fit format such as .pdf and .yml
The follownig is a screenshot of a working CI.yml
<img width="1915" height="872" alt="Screenshot 2026-09-04 230922" src="https://github.com/user-attachments/assets/6933fd4c-11fe-4a21-9070-f1cb2623f5a0" />
## Video Demonstration
Youtube Link: [Here is my demonstration video link](https://youtu.be/lg4PbJiuEHY)
## Part 2- API Application
### Technologies Used
- ASP.NET Core Web API
- Class Library
- xUnit Test Project
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Swashbuckle.AspNetCore
- Swashbuckle.AspNetCore.Annotations
- Microsoft.AspNetCore.Mvc.Testing
- Microsoft.EntityFrameworkCore.InMemory
- SQL Server Management Studio  SQLEXPRESS
- .NET 8.0.0
- Visual Studio 2022
### Project Structure
Under RaceDay.API there are the following folders:
- Controllers: This have all the controller classes are
- Data: This is where RaceDayDBCOntext is
- Mitigations: This is where all the migitigations are
- Models: This is where the entity model classes are
Then The Program.cs is find in this part of the solution
Under RaceDay.API.Tests are the following folders:
- Infrastucture: This is where you will find the classes that the rest of the test classes will refer back to
- Tests: In this file are all the program test classes
Under RaceDay.Contracts:
- DTOs: This is the folder that contains all project DATA TRANSFER OBJECTS(DTO)
### Database Setup
When Setting up the database you have to use: ""DefaultConnection": "Server=.\
\SQLEXPRESS;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;"
and be aware that if your normal database module uses another SQL Server instance, use the server name appropriate for your 
environment. The important requirement is that your API successfully connects to SQL Server"
### How to Run the API
Make sure the default startup item is RaceDay.Api and select "https" to run the project
### API Endpoints
The are the API following Endpoints
- Profile: GET /api/profile/me
PUT /api/profile/me
- Events: GET    
GET /api/events   
GET /api/events/{id}   
GET /api/events/mine
POST /api/events
PUT /api/events/{id}
DELETE /api/events/{id}
- Categories: GET /api/events/{eventId}/categories
  POST   /api/events/{eventId}/categories
  PUT    /api/categories/{id}
  DELETE /api/categories/{id}
- Event: POST /api/enrolments
  GET  /api/enrolments/mine
  GET  /api/enrolments/event/{eventId}
  PUT  /api/enrolments/{id}/status
- Results: POST /api/results
  GET  /api/results/mine
  GET  /api/results/event/{eventId}
### Swagger
Once you run the "https" you will enter the swagger interface. 
There is a 2-factor Authentication called "Authorise". This is a JWT Token feature which acts like "google authenticator"
Once you have registered into the system for an account, it will give you a token, called "AccessToken", then you copy whatever is in
the quotation marks and pasted it where it say "Value" at the "Authorise" Button.
Once you are authorised you can complete the different functions according to your Authorise Role
### Unit Testing
Under Tests in Visual Studio 2022, cless Run all tests and all 12 should pass successfully
### CI/CD
The Updated "dotnet.yml" is found under Actions tab and it is the build and tests CI/CD for Part 2. 
It is "Updated" because it was debugged until it became successful
### GitHub Actions Screenshot
<img width="1897" height="872" alt="Screenshot 2026-10-16 233438" src="https://github.com/user-attachments/assets/99007d81-64bf-4035-9379-4ee9b35d53a0" />

### Video Presentation
Youtube Link: [Here is my Part 2 pesentation video link](https://youtu.be/8tISr3vJ4MI)
