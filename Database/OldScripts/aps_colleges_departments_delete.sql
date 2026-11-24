
UPDATE Majors
SET CollegeId = NULL

UPDATE Referees
SET CollegeId = NULL

UPDATE Requests
SET CollegeId = NULL

UPDATE Researchers
SET CollegeId = NULL

UPDATE ThesisRequests
SET CollegeId = NULL

UPDATE UserExtraRoles
SET CollegeId = NULL

UPDATE Users
SET CollegeId = NULL

UPDATE Researchers
SET DepartmentID = NULL

UPDATE Referees
SET DepartmentID = NULL

UPDATE Users
SET DepartmentID = NULL

DELETE FROM Departments
DBCC CHECKIDENT ('dbo.Departments',RESEED, 0)

ALTER TABLE Requests NOCHECK CONSTRAINT all
DELETE FROM Colleges
DBCC CHECKIDENT ('dbo.Colleges',RESEED, 0)
ALTER TABLE Requests  WITH CHECK CHECK CONSTRAINT all


