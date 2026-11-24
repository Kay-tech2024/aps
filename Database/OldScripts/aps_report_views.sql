IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ViewRequestsDetails]'))
BEGIN
	DROP VIEW ViewRequestsDetails
END

GO

Create VIEW ViewRequestsDetails as 
select r.Id, r.Type, r.Title, rs.FirstName, rs.SecondName, rs.ThirdName, rs.LastName, r.Status, r.PublicationType, r.Created, r.CreatedBy, r.Updated, r.UpdatedBy, 
(case when r2.Id IS NULL then 0
	else
		case r2.Status
			when 63 then 1
			else 0
		end
end)
IsPrinted
from requests r 
join users u on u.id = r.CreatedBy
join Researchers rs on rs.UserId = u.Id
left join requests r2 on r2.id = (select top 1 id from requests r3 where r3.PublicationRequestId = r.Id and r3.type = 6 order by id desc)

GO



IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ViewRequestsByTypeReport]'))
BEGIN
	DROP VIEW ViewRequestsByTypeReport
END

GO

Create VIEW ViewRequestsByTypeReport as select * from 
	(
		select 
				1,
				'Book',  
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 1 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 1 and r1.status = 63)	
			
			UNION SELECT
				2,
				'Handbook',
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 2 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 2 and r1.status = 63)	
			UNION SELECT
				3,
				'Thesis',
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 3 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 3 and r1.status = 63)	

			UNION SELECT
				4,
				'Dictionary',
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 4 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 4 and r1.status = 63)	

			UNION SELECT
				5,
				'Encyclopedia',
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 5 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 5 and r1.status = 63)	

			UNION SELECT
				6,
				'Probe',
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6 and status = 63),
				(select count(*) from Requests where (type = 1 or type= 2) and PublicationType = 6 and status = 51),
				(select count(*) from Requests r1 left join Requests r2 on r1.PublicationRequestId = r2.Id where (r1.type = 6) and r2.PublicationType = 6 and r1.status = 63)	

	) E(PublicationTypeId, PublicationType, SubmittedRequests, ApprovedRequests, RejectedRequests, PrintedRequests)
	
	
go

IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[FuncRequestsByType]'))
BEGIN
	DROP FUNCTION [FuncRequestsByType]
END

GO

CREATE FUNCTION [dbo].[FuncRequestsByType] (@PublicationType smallint, @RequestStatus smallint)
RETURNS TABLE
AS
RETURN
	select * from viewrequestsdetails vr
	where 
		(@PublicationType = 0 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 0 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 0 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 0 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.IsPrinted = 1) OR
		
		
		
		(@PublicationType = 1 and vr.PublicationType = 1 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 1 and vr.PublicationType = 1 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 1 and vr.PublicationType = 1 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 1 and vr.PublicationType = 1 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1) OR
		(@PublicationType = 2 and vr.PublicationType = 2 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 2 and vr.PublicationType = 2 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 2 and vr.PublicationType = 2 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 2 and vr.PublicationType = 2 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1) OR
		(@PublicationType = 3 and vr.PublicationType = 3 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 3 and vr.PublicationType = 3 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 3 and vr.PublicationType = 3 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 3 and vr.PublicationType = 3 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1) OR
		(@PublicationType = 4 and vr.PublicationType = 4 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 4 and vr.PublicationType = 4 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 4 and vr.PublicationType = 4 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 4 and vr.PublicationType = 4 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1) OR
		(@PublicationType = 5 and vr.PublicationType = 5 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 5 and vr.PublicationType = 5 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 5 and vr.PublicationType = 5 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 5 and vr.PublicationType = 5 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1) OR
		(@PublicationType = 6 and vr.PublicationType = 6 and @RequestStatus = 1 and (vr.Type = 1 or vr.Type = 2)) OR
		(@PublicationType = 6 and vr.PublicationType = 6 and @RequestStatus = 2 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63) OR
		(@PublicationType = 6 and vr.PublicationType = 6 and @RequestStatus = 3 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 51) OR
		(@PublicationType = 6 and vr.PublicationType = 6 and @RequestStatus = 4 and (vr.Type = 1 or vr.Type = 2) and vr.Status = 63 and vr.IsPrinted = 1)

go


IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ViewRewardRequestsDetails]'))
BEGIN
	DROP VIEW ViewRewardRequestsDetails
END

GO

Create VIEW ViewRewardRequestsDetails as 
select r.Id, r.Type, r.Title, rs.FirstName, rs.SecondName, rs.ThirdName, rs.LastName, r.Status, r.PublicationType, r.Created, r.CreatedBy, r.Updated, r.UpdatedBy, rr.Amount
from requests r 
join RewardRequests rr on rr.Requestid = r.Id
join users u on u.id = r.CreatedBy
join Researchers rs on rs.UserId = u.Id
where r.type in (3,4,5)

GO


IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ViewRequestsByFinancialTypesReport]'))
BEGIN
	DROP VIEW ViewRequestsByFinancialTypesReport
END

GO

Create VIEW ViewRequestsByFinancialTypesReport as select * from 
	(
		select 
				1,
				'Researcher',  
				(select count(*) from Requests where type = 3),
				(select count(*) from Requests where type = 3 and status = 63),
				(select count(*) from Requests where type = 3 and status = 51),
				(select sum(r1.amount) from RewardRequests r1 left join Requests r2 on r1.RequestId = r2.Id where r2.type = 3 and r2.status = 63)	
			UNION SELECT
				3,
				'Proofreader',
				(select count(*) from Requests where type = 5),
				(select count(*) from Requests where type = 5 and status = 63),
				(select count(*) from Requests where type = 5 and status = 51),
				(select sum(r1.amount) from RewardRequests r1 left join Requests r2 on r1.RequestId = r2.Id where r2.type = 5 and r2.status = 63)	

			UNION SELECT
				4,
				'Referee',
				(select count(*) from Requests where type = 4),
				(select count(*) from Requests where type = 4 and status = 63),
				(select count(*) from Requests where type = 4 and status = 51),
				(select sum(r1.amount) from RewardRequests r1 left join Requests r2 on r1.RequestId = r2.Id where r2.type = 4 and r2.status = 63)	

	) E(UserRoleId, UserRole, SubmittedRequests, ApprovedRequests, RejectedRequests, TotalAmount)
	
	
go

IF  EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[FuncRequestsByFinancialTypes]'))
BEGIN
	DROP FUNCTION [FuncRequestsByFinancialTypes]
END

GO

CREATE FUNCTION [dbo].[FuncRequestsByFinancialTypes] (@UserRole smallint, @RequestStatus smallint)
RETURNS TABLE
AS
RETURN
	select * from ViewRewardRequestsDetails vr
	where 
		(@UserRole = 0 and @RequestStatus = 1) OR
		(@UserRole = 0 and @RequestStatus = 2 and vr.Status = 63) OR
		(@UserRole = 0 and @RequestStatus = 3 and vr.Status = 51) OR
		
		(@UserRole = 1 and @RequestStatus = 1 and vr.Type = 3) OR
		(@UserRole = 1 and @RequestStatus = 2 and vr.Type = 3 and vr.Status = 63) OR
		(@UserRole = 1 and @RequestStatus = 3 and vr.Type = 3 and vr.Status = 51) OR
		
		(@UserRole = 3 and @RequestStatus = 1 and vr.Type = 4) OR
		(@UserRole = 3 and @RequestStatus = 2 and vr.Type = 4 and vr.Status = 63) OR
		(@UserRole = 3 and @RequestStatus = 3 and vr.Type = 4 and vr.Status = 51) OR
		
		(@UserRole = 4 and @RequestStatus = 1 and vr.Type = 5) OR
		(@UserRole = 4 and @RequestStatus = 2 and vr.Type = 5 and vr.Status = 63) OR
		(@UserRole = 4 and @RequestStatus = 3 and vr.Type = 5 and vr.Status = 51)

go