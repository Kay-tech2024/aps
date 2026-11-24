update requests set LastActionId=null
delete from RequestHistoryLogs
delete from requestreferees
delete from RequestFiles
delete from requestdata
delete from BookRequests
delete from ThesisRequests
delete from ReviewRequests
delete from EncyclopediaRequests
delete from DictionaryRequests
delete from RequestCoresearchers
delete from HandbookRequests
delete from RewardRequests
delete from requestusers
delete from RequestDistributionListAgencies
delete from requests

delete from UserSessionData
delete from UserSessionFiles
delete from UserSessions
delete from userlogs
update settings set CreatedBy = '00000000-0000-0000-0000-000000000000', UpdatedBy = '00000000-0000-0000-0000-000000000000'
delete from Referees
delete from Researchers
delete from UserProfiles where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from userroles where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from userfiles where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from UserProfiles where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from UserTemporaryUrls where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from UserExtraRoles where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from UserPreferences where userid in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
update DistributionEntities set CreatedBy =  '00000000-0000-0000-0000-000000000000', UpdatedBy = '00000000-0000-0000-0000-000000000000'
update templates set CreatedBy =  '00000000-0000-0000-0000-000000000000', UpdatedBy = '00000000-0000-0000-0000-000000000000'
update UserProfiles set CreatedBy =  '00000000-0000-0000-0000-000000000000'where createdby in ( (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher'))
update UserProfiles set UpdatedBy =  '00000000-0000-0000-0000-000000000000'where UpdatedBy in ( (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher'))
update users set CreatedBy =  '00000000-0000-0000-0000-000000000000'where createdby in ( (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher'))
update users set UpdatedBy =  '00000000-0000-0000-0000-000000000000'where UpdatedBy in ( (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher'))
delete from UserDelegation where UserDelegation.DelegateeId in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher') or UserDelegation.DelegatorId in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')
delete from users where id in (select id from viewusers where RoleName is null or RoleName  = 'Referee' or rolename = 'Researcher')





--- remove kay users

delete from userroles where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from UserProfiles where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from UserSessionData where SessionId in(select id from UserSessions where userid in (select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system'))
delete from UserSessionFiles where SessionId in(select id from UserSessions where userid in (select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system'))
delete from UserSessions where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from UserFiles where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from referees where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from Researchers where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from UserTemporaryUrls where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from UserExtraRoles where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
delete from userlogs where userid in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')
update requests set lastactionid = null, PublicationRequestId = null where id in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from RequestHistoryLogs where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from RequestDistributionListAgencies where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from RequestUsers where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from BookRequests where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from RequestCoresearchers where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from RequestFiles where requestid in (Select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))

delete from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')) or PublicationRequestId in (select id from requests where  CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system') or PublicationRequestId in (select id from requests where CreatedBy in(select id from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system')))
delete from users where (lower(email) like N'%@kay-ict.com' or lower(email) like N'%@kay.sa' ) and username != 'system'

