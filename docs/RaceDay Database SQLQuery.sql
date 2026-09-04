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
