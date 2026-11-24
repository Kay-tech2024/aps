
-- Date: 06/05/2019
-- Added new columns to Users
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Users]') 
         AND name = 'ForceToUpdateProfile'
)
ALTER TABLE Users ADD [ForceToUpdateProfile] bit default(0);
GO
-- Date: 06/05/2019
-- Added new columns to Referees
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Referees]') 
         AND name = 'ForceToUpdateProfile'
)
ALTER TABLE Referees ADD [ForceToUpdateProfile] bit default(0);
GO
-- Date: 06/05/2019
-- Added new columns to Researchers
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[Researchers]') 
         AND name = 'ForceToUpdateProfile'
)
ALTER TABLE Researchers ADD [ForceToUpdateProfile] bit default(0);
GO