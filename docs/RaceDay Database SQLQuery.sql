CREATE DATABASE RaceDay;
Go
USE RaceDay;
Go
-- Table Creation
CREATE TABLE Participant 
(
ParticipantID INT IDENTITY(1,1) PRIMARY KEY,
FirstName VARCHAR(30) NOT NULL,
LastName VARCHAR(30) NOT NULL,
CellNumber VARCHAR(15) NOT NULL,
Address VARCHAR(200) NOT NULL
);
CREATE TABLE Organiser
(
OrganiserID INT IDENTITY(1,1) PRIMARY KEY,
FirstName VARCHAR(30) NOT NULL,
LastName VARCHAR(30) NOT NULL,
Address VARCHAR(200) NOT NULL
);
CREATE TABLE Account
(
AccountID INT IDENTITY(1,1) PRIMARY KEY,
OrganiserID INT FOREIGN KEY REFERENCES Organiser(OrganiserID) NULL,
ParticipantID INT FOREIGN KEY REFERENCES Participant(ParticipantID) NULL,
AuthenticationRole VARCHAR(30) NOT NULL,
Email VARCHAR(100) UNIQUE,
PasswordHash VARCHAR(230) NOT NULL
);

CREATE TABLE Event 
(
EventID INT IDENTITY(100,1) PRIMARY KEY,
OrganiserID int FOREIGN KEY REFERENCES Organiser(OrganiserID),
EventName VARCHAR(50) NOT NULL,
Description VARCHAR(100) NOT NULL,
EventType VARCHAR(50) NOT NULL,
EventDate DATETIME2 NOT NULL,
EventDuration VARCHAR(50) NOT NULL,
Location VARCHAR(100) NOT NULL,
Distance VARCHAR(50) NOT NULL
);
CREATE TABLE Category
(
CategoryID INT IDENTITY(1010,1) PRIMARY KEY,
EventID INT FOREIGN KEY REFERENCES Event(EventID),
CategoryName VARCHAR(30) NOT NULL,
MinAge INT NOT NULL,
MaxAge INT NOT NULL
);
CREATE TABLE Enrolment
(
EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
AccountID INT FOREIGN KEY REFERENCES Account(AccountID),
CategoryID INT FOREIGN KEY REFERENCES Category(CategoryID),
EventID INT FOREIGN KEY REFERENCES Event(EventID),
EnrolmentDate DATETIME2 NOT NULL
);
CREATE TABLE RaceResults
(
RaceResultsID INT IDENTITY(1101,1) PRIMARY KEY,
EnrolmentID INT UNIQUE FOREIGN KEY REFERENCES Enrolment(EnrolmentID),
FinishTime VARCHAR(30) NOT NULL,
FinishPosition INT NOT NULL,
PerformanceDetails VARCHAR(255) NULL
);
CREATE TABLE Awards
(
AwardsID INT IDENTITY(1,1) PRIMARY KEY,
RaceResultsID INT FOREIGN KEY REFERENCES RaceResults(RaceResultsID),
AwardDetails VARCHAR(255) NULL
);
-- Table data Insertion
INSERT INTO Participant
VALUES
('Elizabeth', 'Reddington','0758457478', '15 Diepkloof Street'),
('Nkuli', 'September','0768455468', '10 Diepkloof Street'),
('John', 'Van Wester','0778455463', '10 Watertrapper Cresent');
INSERT INTO Organiser
VALUES
('Philip', 'Smith', '10 Stroms Road'),
('Maria', 'Johnson', '10 Lydia Street');
INSERT INTO Account
VALUES
-- Participant Account
(NULL, 1, 'Participant', 'reddingtonlizzy@gmail.com', 'Lizzy20#20'),
(NULL, 2, 'Participant', 'nuseptember@gmail.com', 'SeptNkuli&1010'), 
(NULL, 3, 'Participant', 'westerjohn22@gmail.com', '@WesterJ1995');
INSERT INTO Account
VALUES
-- Organisers Account
(1, NULL, 'Organiser', 'Philsmith22@gmail.com', 'Philly_1999'),
(2, NULL, 'Organiser', 'marryjohnny@gmail.com', 'MariaJ@1090');
INSERT INTO Event 
VALUES
(1, 'City Run', 'Old Mutal Marathon event', 'Marathon', '2026-09-10 08:00:00', '4 Hours', 'FNB Stadium', '10 KM'), 
(2, 'Sprint Run', 'Addidas Sprint event', 'Sprints', '2026-09-10 08:30:00', '1 Hours', 'FNB Stadium', '200 M'), 
(2, 'Sprint Run', 'Addidas Sprint event', 'Sprints', '2026-09-10 09:00:00', '1 Hours', 'FNB Stadium', '100 M');
INSERT INTO Category
VALUES
(100, 'Senior''s Marathon', 18, 39), 
(100, 'Youth Marathon', 16, 25), 
(101, 'High Performance', 25, 39);
INSERT INTO Enrolment
VALUES

(1, 1010, 100, '2026-07-10 08:00:00'), 
(2, 1011, 100, '2026-08-10 08:30:00'),
(3, 1012, 101, '2026-08-10 10:30:00');
INSERT INTO RaceResults 
VALUES 
(1, '00:52:30', 1, 'Well endurance performance'), 
(3, '00:09:30', 2, 'Excellent Technique'), 
(2, '00:10:30', 3, 'Excellent Start');
INSERT INTO Awards 
VALUES
(1101, '1st Place - Senior Marathon'), 
(1102, '2nd Place - High Performance'),
(1103, '3rd Place - Youth Marathon');
SELECT * FROM Participant;
SELECT * FROM Organiser;
SELECT * FROM Account;
SELECT * FROM Event;
SELECT * FROM Category;
SELECT * FROM Enrolment;
SELECT * FROM RaceResults;
SELECT * FROM Awards;






