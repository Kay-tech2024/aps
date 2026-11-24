using System;

namespace APS.Data
{
    public enum RequestAction : byte
    {
        Unspecified = 0,
        Submit = 1,
        Approve = 2,
        Reject = 3,
        ApproveWithComments = 4,
        Forward = 5,
        Resubmit = 6,
        SubmitCoresearcherData = 7,
        Cancel = 8,
        ReturnToResearcher = 9,
        ForwardToInitialReferee = 10,
        Publish = 11,
        ReturnToResearchUnitHead = 12,
        ReturnToViceDeanOfAcademicResearch = 13,
        SelectReferees = 14,
        ForwardToPressOficer = 15,
        ForwardToProofreader = 16,
        ForwardToResearchUnitSecretary = 17,
        ForwardToDeanOfAcademicResearch = 18,
        ForwardToSpareReferee = 19,
        ApproveWithNoReferees = 20,
        ForwardToResearchUnitHead = 21,
        ApproveAndForwardToResearchUnitHead = 22,
        Accept = 23,
        SendToCommiteeMembers = 24,
        ApproveAndForwardToDeanOfAcademicResearch = 25,
        RejectAndForwardToDeanOfAcademicResearch = 26,
        ForwardToDeanshipOfAcademicResearchCommiteeMembers = 27,
        ApproveAndForwardToUnversityRector = 28,
        RejectAndForwardToUniversityRector = 29,
        ReturnToResearchUnitSecretary = 30,
        ReturnToDeanshipOfAcademicResearch = 31,
        ApproveApproval = 32,
        ForwardToPAECommitteeHead = 33,
        ForwardToAcademicCouncilHead = 34,

        ApproveAndReturnToPAECommitee = 35,
        RejectAndReturnToPAECommitee = 36,
        ForwardToMainReferees = 37,
        ReturnToAcademdicCouncil = 38,
        ReturnToPAECommittee = 39,
        ApproveRejection = 40,
        ResendToReferees = 41,

        ForwardToAcademicCouncil = 42,
        ApproveAndForwardToViceDeanOfAcademicResearch = 43,
        RejectAndForwardToViceDeanOfAcademicResearch = 44,

        ApproveAndForwardToAcademicCouncil = 45,
        RejectAndForwardToAcademicCouncil = 46,
        ApproveWithCommentsAndForwardToAcademicCouncil = 47,
        ProvideFeedback = 48,
        ReturnToPAECommitteeSecretary = 49,
        ForwardToReferees = 50,
        ApproveModifications = 51,
        RejectModifications = 52,
        ApproveRejectionAndForwardToUniversityRector = 53,
        ApproveApprovalAndForwardToUniversityRector = 54,
        RejectAndReturnToResearcher = 55,
        ResubmitAndReturnToCoresearchers = 56,
        ReturnToAcademdicCouncilSecretary = 57,
        ResubmitAndForwardToPAECommitee = 58,
        ForwardToViceDeanOfAcademicResearch = 59,
        ReturnToOfficerOfPrintingUnit = 60,
        ApproveAndForwardToUniversityPressDirector = 61,
        RejectAndReturnToOfficerOfPrintingUnit = 62,
        CertifyAndForwardToUniversityPressOfficer = 63,
        OperationCommandExecutedAndForwardToPUAP = 64,
        ReturnToUniversityPressDirector = 65,
        ReturnToUniversityPressOfficer = 66,
        ForwardToResearcher = 67,
        ReturnToProofreader = 68,
        SubmitOperationCommandToViceDeanOfAcademicResearch = 69,
        ReturnToPUAP = 70,
        AcceptWithComments = 71,
        CloseRequest = 72,
        SubmitISBNRequestToKFNL = 73,
        SubmitISBNToImamUniversity = 74,
        ReturnToKFNLOfficer = 75,
        RejectAndReturnToPUAP = 76,
        RejectAndReturnToViceDeanOfAcademicResearch = 77,
        SubmitToPressOfficer = 78,
        PrintingCompleted = 79,
        ApproveAndForwardToFiancialDepartmentDirector = 80,
        ApproveAndForwardToAccountant = 81,
        RejectAndReturnToDeanOfAcademicResearch = 82,
        PaymentCompleted = 83,
        PrepareDistributionList = 84,
        ConfirmReceivalOfCopies = 85,
        ConfirmDeliveringCopiesToResaercher = 86,
        ForwardToKFNL = 87,
        ConfirmDeliveringCopiesToKFNL = 88,
        ConfirmDeliveringCopiesToAgencies = 89,
        ForwardToUniversityPressDirector = 90,
        ApproveAndForwardToDeanshipOfLibraryAffairs = 91,
        ApproveAndAndForardToDeanOfAcademicResearch = 92,
        ReturnToDeanshipOfLibraryAffairs = 93,
        AcceptedByCoResearcher = 94,
        Canceled = 100,
        SkipToSpareReferee = 101,
        AcceptToReview = 102,
        RejectToReview = 103,
        RejectToReviewBySystem = 104,
        AcceptToReviewButNotResponded = 105,
        SentForRefereeSelectionDueToRejectedByAllReferees = 106
    }

    public enum TempUrlType : byte
    {
        Unspecified = 0,
        SmsActivation = 1,
        EmailActivation = 2,
        CoresearcherApproval = 3,
        RefereeApproval = 4,
        ResetPassword = 5,
        RegisterReferee = 6,
        RefereeAccountActivation = 7,
        RefereeEmailActivation = 8,
        SmsActivationNoUser = 9,
        EmailActivationNoUser = 10,
        WelcomeMessageResearcher = 11,
        CoresearcherNotification = 12,
    }

    public enum RequestType : byte
    {
        Unspecificed = 0,
        PublishRequest = 1,
        RepublishRequest = 2,
        ResearcherReward = 3,
        RefereeReward = 4,
        ProofreaderReward = 5,
        OperationCommand = 6,
        Pricing = 7,
        Distribution = 8
    };

    public enum PublicationType : byte
    {
        Unspecificed = 0,
        Book = 1,
        Handbook = 2,
        Thesis = 3,
        Dictionary = 4,
        Encyclopedia = 5,
        Probe = 6
    };

    [Flags]
    public enum OldRequestStatus : long
    {
        Unspecified = ((long)1) << 0,

        // Request type: publish request
        PublishRequestNotSubmitted = ((long)1) << 1,

        PublishRequestPendingModificationsAfterRUS = ((long)1) << 46,
        PublishRequestPendingModificationsAfterReferees = ((long)1) << 2,
        PublishRequestPendingCoresearchers = ((long)1) << 3,
        PublishRequestRejectedByCoresearchers = ((long)1) << 4,

        PublishRequestPendingResearchUnitSecretary = ((long)1) << 6,
        PublishRequestPendingResearchUnitHead = ((long)1) << 7,

        PublishRequestPendingInitialReferee = ((long)1) << 8,

        PublishRequestPendingDeanOfAcademicResearchPublishApproval = ((long)1) << 10,
        PublishRequestPendingUniversityRectorPublishApproval = ((long)1) << 11,

        PublishRequestPendingPAERefereeSelection = ((long)1) << 12,

        PublishRequestPendingAcademicCouncilRefereeSelection = ((long)1) << 13,
        //PendingUniversityRectorRefereeApproval = ((long)1) << 14,

        PublishRequestPendingReferees = ((long)1) << 15,
        PublishRequestPendingPAEAfterReferees = ((long)1) << 16,
        PublishRequestPendingSpareReferee = ((long)1) << 14,

        PublishRequestPendingAcademicCouncilAfterReferees = ((long)1) << 17,

        // Request type: Operation command
        OperationCommandPendingOperationCommandInitiation = ((long)1) << 20,

        OperationCommandPendingOperationCommandPreparation = ((long)1) << 18,
        OperationCommandPendingOperationCommandApproval = ((long)1) << 19,
        OperationCommandPendingOperationCommandExecution = ((long)1) << 21,
        OperationCommandPendingOperationCommandValidation = ((long)1) << 42,
        OperationCommandPendingResearcherAcceptance = ((long)1) << 22,
        OperationCommandPendingISBNRequestSubmission = ((long)1) << 23,
        OperationCommandPendingProofreading = ((long)1) << 43,
        OperationCommandPendingOperationCommandFinalApproval = ((long)1) << 24,
        OperationCommandPendingPrintingManager = ((long)1) << 25,
        OperationCommandPendingOperationCommandForward = ((long)1) << 44,
        OperationCommandPendingPendingCorrection = ((long)1) << 45,
        OperationCommandPendingPrinting = ((long)1) << 47,

        // Requst type: Pricing
        PricingPendingPricingPrepartion = ((long)1) << 26,

        PricingPendingPricingApprovalByUniversityPress = ((long)1) << 27,
        PricingPendingPricingApprovalByLibraryAffairs = ((long)1) << 28,
        PricingPendingPricingApprovalByDean = ((long)1) << 29,

        // Request type: distribution
        DistributionPendingDistributionListPreparation = ((long)1) << 30,

        DistributionPendingDistributionListApproval = ((long)1) << 31,
        DistributionPendingAuthorCopiesDistributionByPR = ((long)1) << 32,
        DistributionPendingKFNLCopiesDistributionByPrintingUnit = ((long)1) << 33,
        DistributionPendingCopiesDistributionByLibraryAffairs = ((long)1) << 34,
        DistributionPendingDistributionClosure = ((long)1) << 35,

        // Request type: reward
        RewardPendingApprovalByViceDean = ((long)1) << 37,

        RewardPendingApprovalByDean = ((long)1) << 38,
        RewardPendingAccountant = ((long)1) << 39,

        PendingAcademicCouncilApproval = ((long)1) << 41,

        PendingAdvertisement = ((long)1) << 40,

        Pending = ((long)1) << 50,
        Rejected = 51,
        ConditionalApproval = ((long)1) << 52,
        Approved = ((long)1) << 53,

        RejectedByResearcher = ((long)1) << 54,

        Canceled = 62,
        Completed = 63,
    }

    [Flags]
    public enum RequestStatus : long
    {
        Unspecified = 0,

        // Request type: publish request
        PublishRequestNotSubmitted = 1,

        PublishRequestPendingModificationsAfterRUS = 46,
        PublishRequestPendingModificationsAfterReferees = 2,
        PublishRequestPendingCoresearchers = 3,
        PublishRequestRejectedByCoresearchers = 4,
        PublishRequestPendingModifiedAfterReferees = 384,
        PublishRequestPendingCoresearchersAfterRefereesModified = 385,
        PublishRequestRejectedByCoresearchersAfterRefereeAndModified = 386,
        PublishRequestPendingCoresearchersAfterModified = 387,
        PublishRequestPendingResearchUnitSecretary = 6,
        PublishRequestPendingResearchUnitHead = 7,

        PublishRequestPendingResearchUnitCommittee = 5,

        //PublishRequestPendingInitialReferee = 8,
        PublishRequestPendingInitialRefereeForRUH = 8,

        PublishRequestPendingInitialRefereeForDAR = 9,

        PublishRequestPendingViceDeanOfAcademicResearchPublishApproval = 48,
        PublishRequestPendingDeanOfAcademicResearchPublishApproval = 10,

        PublishRequestPendingDeanshipOfAcademicResearchCommitee = 36,
        PublishRequestPendingUniversityRectorPublishApproval = 11,

        PublishRequestPendingPAERefereeSelection = 12,
        PublishRequestPendingPAERefereeSelectionViceDean = 360,
        PublishRequestPendingPAERefereeSelectionHead = 333,
        PublishRequestPendingPAERefereeSelectionMembers = 336,

        PublishRequestPendingAcademicCouncilRefereeSelection = 13,
        PublishRequestPendingAcademicCouncilRefereeSelectionMembers = 334,
        PublishRequestPendingAcademicCouncilRefereeSelectionHead = 335,
        PublishRequestPendingUniversityRectorAFterRefereeSelection = 338,
        //PendingUniversityRectorRefereeApproval = 14,

        PublishRequestPendingPAEAfterRefereeSelectionViceDean = 361,
        PublishRequestPendingPAEAfterRefereeSelection = 337,
        PublishRequestPendingReferees = 15,
        PublishRequestPendingPAEAfterReferees = 16,
        PublishRequestPendingPAEAfterRefereesViceDean = 362,
        PublishRequestPendingPAEAfterRefereesHead = 345,
        PublishRequestPendingPAEAfterRefereesMembers = 346,
        PublishRequestPendingSpareReferee = 14,

        PublishRequestPendingAcademicCouncilAfterReferees = 344,
        PublishRequestPendingAcademicCouncilAfterRefereesHead = 347,
        PublishRequestPendingAcademicCouncilAfterRefereesMembers = 348,
        PublishRequestPendingAcademicCouncilAfterRefereesApprove = 17,
        PublishRequestPendingAcademicCouncilAfterRefereesApproveHead = 349,
        PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers = 350,
        PublishRequestPendingAcademicCouncilAfterRefereesReject = 339,
        PublishRequestPendingAcademicCouncilAfterRefereesRejectHead = 351,
        PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers = 352,
        PublishRequestPendingAcademicCouncilAfterRefereesApproveWithComments = 340,
        PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsHead = 356,
        PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsMembers = 354,
        PublishRequestPendingPAECouncilAfterRefereesApproveWithComments = 341,
        PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsViceDean = 363,
        PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsHead = 357,
        PublishRequestPendingPAEAfterRefereesAndResearcherModifications = 342,
        PublishRequestPendingPAEAfterRefereesAndResearcherModificationsViceDean = 364,
        PublishRequestPendingPAEAfterRefereesAndResearcherModificationsHead = 353,
        PublishRequestPendingRefereesAfterRefereesAndResearcherModifications = 343,
        PublishRequestPendingAcademicCouncilAfterRefereesRectorApproved = 358,
        PublishRequestPendingAcademicCouncilAfterRefereesRectorRejected = 359,
        PublishRequestPendingAfterRefereesResponseAndResearcherModifications = 384,
        PublishRequestPendingUniversityRectorPublishRejected = 385,

        // Request type: Operation command
        OperationCommandPendingOperationCommandInitiation = 20,

        OperationCommandPendingOperationCommandInitiationAfterProofReading = 365,
        OperationCommandPendingOperationCommandPreparation = 18,
        OperationCommandPendingOperationCommandApproval = 19,
        OperationCommandPendingOperationCommandExecution = 21,
        OperationCommandPendingOperationCommandValidation = 42,
        OperationCommandPendingResearcherAcceptance = 22,
        OperationCommandPendingISBNRequestSubmission = 23,
        OperationCommandPendingProofreading = 43,
        OperationCommandPendingOperationCommandFinalApproval = 24,
        OperationCommandPendingPrintingManager = 25,
        OperationCommandPendingOperationCommandForward = 44,
        OperationCommandPendingPendingCorrection = 45,
        OperationCommandPendingPrinting = 47,
        OperationCommandPendingResearcherProofreadingAcceptance = 366,
        OperationCommandPendingOperationCommandForwardResearcherRejection = 367,
        OperationCommandPendingResearcherComments = 368,
        OperationCommandPendingOperationCommandForward1AndHalf = 369,
        OperationCommandPendingResearcherComments2 = 370,
        OperationCommandPendingOperationCommandExecution2 = 371,
        OperationCommandPendingOperationCommandForward2 = 372,
        OperationCommandPendingOperationCommandForward2AndHalf = 373,
        OperationCommandPendingOperationCommandExecution3 = 374,
        OperationCommandPendingOperationCommandForward3 = 375,
        OperationCommandPendingOperationCommandResearcherRejected = 376,
        OperationCommandPendingISBN = 377,
        OperationCommandAfterISBN = 378,
        OperationCommandPendingOperationCommandFinalApprovalViceDean = 379,

        // Requst type: Pricing
        PricingPendingPricingPrepartion = 26,

        PricingPendingPricingApprovalByUniversityPress = 27,
        PricingPendingPricingApprovalByLibraryAffairs = 28,
        PricingPendingPricingApprovalByDean = 29,

        // Request type: distribution
        DistributionPendingDistributionListPreparation = 30,

        DistributionPendingDistributionListApproval = 31,
        DistributionPendingDistributionListApprovalViceDean = 381,
        DistributionPendingAuthorCopiesDistributionByPR = 32,
        DistributionPendingKFNLCopiesDistributionByPrintingUnit = 33,
        DistributionPendingCopiesDistributionByLibraryAffairs = 34,
        DistributionPendingDistributionClosure = 35,
        DistributionPendingResearcherConfirmation = 382,
        DistributionPendingCopyDistribution = 383,

        // Request type: reward
        RewardPendingApprovalByViceDean = 37,

        RewardPendingApprovalByDean = 38,
        RewardPendingAccountant = 39,
        RewardPendingFinancialDepartmentDirector = 380,

        PendingAcademicCouncilApproval = 41,

        PendingAdvertisement = 40,

        Pending = 50,
        Rejected = 51,
        ConditionalApproval = 52,
        Approved = 53,

        RejectedByResearcher = 54,

        Canceled = 62,
        Completed = 63,
    }

    public enum RequestHistoryTypes
    {
        Unspecificed = 0,
        RequestCreated = 1,
        SubmittedByMainResearcher = 2,
        ApprovedByCoresearcher = 3,
        SendLinkToCoResearcher = 4,
        RejectByCoResearcher = 5,
        Approve = 14,
        Reject = 15,
        ApproveWithComments = 16,
        Forward = 17,
        Resubmit = 18,
        SubmitCoresearcherData = 19,
        Cancel = 20,
        ReturnToResearcher = 21,
        ForwardToInitialReferee = 22,
        Publish = 23,
        ReturnToResearchUnitHead = 24,
        ReturnToViceDeanOfAcademicResearch = 25
    }

    public enum Report1Status : byte
    {
        Unspecified = 0,
        Submitted = 1,
        Approved = 2,
        Rejected = 3,
        Printed = 4
    }

    public enum Report1UserRole : byte
    {
        Unspecified = 0,
        Researcher = 1,
        Inspector = 2,
        Proofreader = 3,
        Referee = 4
    }

    public enum RefereeType : byte
    {
        Unspecified = 0,
        Normal = 1,
        Initial = 2,
    }

    public enum QualificationType : byte
    {
        Unspecified = 0,
        Doctorate = 1,
        Masters = 2,
        Bachelors = 3,
        Diploma = 4,
        UndergraduateStudent = 5,
        MasterStudent = 6,
        PhdStudent = 7,
        Professor = 8,
        AssociateProfessor = 9,
        AssistantProfessor = 10,
        Technician = 11,
        Vocational = 12,
    }

    public enum ResearcherRole : byte
    {
        Unspecified = 0,
        MainResearcher = 1,
        AssociateResearcher = 2,
        AssistantResearcher = 3,
        Consultant = 4,
    }

    public enum CodeType : byte
    {
        Unspecified = 0,
        SMS = 1,
        Email = 2,
    };

    public enum RequestActions : byte
    {
        Unspecified = 0,
        Approve = 1,
        Reject = 2,
        Returned = 3,
        ConditionalApproval = 4,
        Cancel = 5,
        Submitted = 6,
        Resubmitted = 7
    }

    public enum RequestStep : byte
    {
        Unspecified = 0,
        New = 1,
        ScintificType = 2,
        Bank = 3,
        Participants = 4,
        Agreement = 5,
    }

    public enum ApsUserRole
    {
        None = 0,
        SystemAdmin = 100,
        Researcher = 1,
        ResearchUnitScretary = 2,
        ResearchUnitCouncilMember = 3,
        ResearchUnitHead = 4,
        Referee = 5,

        DeanOfAcademicResearch = 6,
        ViceDeanOfAcademicResearch = 7,
        DeanshipOfAcademicResearchCouncilMember = 8,

        //DearshipOfAcademicResearchCouncilSecretary = 9,
        UniversityRector = 12,

        CommitteeforPAESecretary = 13,
        CommitteeforPAEMember = 14,
        CommitteForPAEHead = 27,

        AcademicCouncilSecretary = 15,
        AcademicCouncilMember = 10,
        AcademicCouncilHead = 30,

        //UniversityRectorOfficeDirector = 11,

        UniversityPressOfficer = 25,
        UniversityPressDirector = 17,
        OfficerOfPrintingUnit = 16,
        Proofreader = 26,

        DeanOfLibraryAffairs = 20,
        PublicRelationsDirector = 21,
        KingFahadNationalLibraryOfficer = 18,
        //PricingCommitteeSecretary = 19,

        //EmployeeOfPortalSection = 22,
        Accountant = 23,

        Webmaster = 24,
        FinancialDepartmentDirecor = 31,
        Reviewer = 32,
        CoResearcher = 33
    };

    public enum FileType : short
    {
        Unspecified = 0,
        Publication = 1,
        CoResearcherOpinion = 7,
        RefereeOpinion = 8,
        AcademicCouncilMemberOpinion = 9,
        DeanshipOfAcademicResearchCouncilMemberOpinion = 10,
        ResearchUnitMemberOpinion = 11,
        CorrectedPublication = 13,
        Resume = 14,
        IbanPhotocopy = 15,
        SwiftPhotocopy = 16,
        Other = 200
    };

    public enum RequestViewStatus : byte
    {
        Unspecified = 0,
        Actionable = 1,
        Returned = 2,
        Approved = 5,
        Rejected = 6,
        Canceled = 7,
        Inbox = 8,
        Outbox = 9,
        Overdue = 10,
    }

    public enum RequestTab : byte
    {
        Unspecified = 0,
        Pending = 1,
        Archive = 2,
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public enum Gender : byte
    {
        All = 2,
        Male = 0,
        Female = 1
    }

    public enum LanguagePreference
    {
        DontCare,
        Arabic,
        English
    }

    public enum MenuPreference
    {
        DontCare,
        Collapsed,
        Expanded
    }

    public enum RequestFields
    {
        Id,
        Date,
        Status,
        Month,
        Year,
        Amount,
        JobSeekerName,
        JobSeekerNin,
        Updated,
        DepartmentName,
        JpcName,
        Type
    }

    public enum TemplateKey : byte
    {
        Unspecified = 0,
        CreateNewUser = 1,
        NewUserEmailActivation = 2,
        NewUserSmsActivation = 3,
        UserActivated = 4,
        ResetPassword = 5,
        ThesisAgreeement = 6,
        BookAgreement = 7,
        ProbeAgreement = 8,
        HandbookAgreement = 9,
        EncyclopediaAgreement = 10,
        DicationaryAgreeement = 11,
        RegisterReferee = 12,
        InviteReferee = 13,
        RegisterRefereeSMS = 14,
        InviteRefereeSMS = 15,
        NonOfficialEmailRequiresApproval = 16,
        ContactUsMessage = 17,

        NotificationForRecearcher = 18,
        NotificationforRuh = 19,
        NotificationForVdar = 20,
        NotificationOnFinalApproval = 21,
        NotificationOnFinalRejection = 22,
        NotificationOnGradeBasedRefereeApproval = 23,
        NotificationOnGradeBasedRefereeRejection = 24,
        RefereesAgreement = 25,
        SmsActivationNoUser = 26,
        EmailActivationNoUser = 27,
        NewCommitteForPAEMemberWelcomeEmail = 31,
        NewCommitteForPAEMemberWelcomeSMS = 32,
        DeanOfAcademicResearchMemberWelcomeEmail = 33,
        DeanOfAcademicResearchMemberWelcomeSMS = 34,
        NewAcademicCouncilMemberWelcomeEmail = 35,
        NewAcademicCouncilMemberWelcomeSMS = 36,
        ResearchUnitMemberWelcomeEmail = 37,
        ResearchUnitMemberWelcomeSMS = 38,
        ExternalRefereeSMSNotification = 39,
        ExternalRefereeEmailNotification = 40,
        InternalRefereeSMSNotification = 41,
        InternalRefereeEmailNotification = 42,
    }

    public enum NotificationForUser : byte
    {
        AttachmentRequestApproval = 0,
        AttachmentRequestRejected = 1,
        TransferRequestApproval = 2,
        TransferRequestRejected = 3,
        PlacementRequestApproval = 4,
        PlacementRequestRejected = 5,
        ExitRequestApproval = 6,
        ExitRequestRejected = 7,
        ExtentionRequestApproval = 8,
        ExtentionRequestRejected = 9,
        SkipEligibilityCheckRequestApproval = 10,
        SkipEligibilityCheckRequestRejected = 11,
        SetupClaimRequestApproval = 12,
        SetupClaimRequestRejected = 13,
        FixedClaimRequestApproval = 14,
        FixedClaimRequestRejected = 15,
        AttachmentClaimRequestApproval = 16,
        AttachmentClaimRequestRejected = 17,
        PlacementClaimRequestApproval = 18,
        PlacementClaimRequestRejected = 19,
        SustainabilityClaimRequestApproval = 20,
        SustainabilityClaimRequestRejected = 21,
        CoachingClaimRequestAppoval = 22,
        CoachingClaimRequestRejected = 23,
        AttachmentNonJwDClaimRequestApproval = 24,
        AttachmentNonJwDClaimRequestRejected = 25,
        PlacementNonJwDClaimRequestApproval = 26,
        PlacementNonJwDClaimRequestRejected = 27,
        SustainabilityNonJwDRequestApproval = 28,
        SustainabilityNonJwDRequestRejected = 29,
        AttachmentJwDClaimRequestApproval = 30,
        AttachmentJwDClaimRequestRejected = 31,
        PlacementJwDClaimRequestApproval = 32,
        PlacementJwDClaimRequestRejected = 33,
        SustainabilityJwdClaimRequestApproval = 34,
        SustainabilityJwdClaimRequestRejected = 35,
        RefferalRequest = 36
    }

    public enum StatusType : byte
    {
        Basic = 0,
        Extended = 1
    }

    public enum ProviderType : byte
    {
        Unspecified = 0,
        JPC = 1,
        RO = 2,
        DR = 3,
        Branch = 4
    }

    public enum ExitingReason : byte
    {
        Unspecified = 0,
        JourneyEnded = 1,
        SustainabilityReached = 2,
        Penalization = 3,
        Transferred = 4,
        Other = 5
    }

    public enum NotificationTab : byte
    {
        Unspecified = 0,
        Requests = 1,
        Claims = 2,
        Contracts = 3
    }

    public enum CancelationReason : byte
    {
        Unspecified = 0,
        NewAttachmentRequest = 1,
        JobseekerAttached = 2,
        JobseekerExited = 3,
        RejectedByBM = 4,
        RejectedByPM = 5,
        RejectedByTaqat = 6,
        RejectedByJobseeker = 7
    }

    public enum JobseekerCitizenshipType : byte
    {
        Unspecified = 0,
        Saudi = 1,
        NonSaudiWithSaudiMother = 2,
        GccResidentWithSaudiMother = 3,
        MigrationTribe = 4
    }

    public enum FieldType : byte
    {
        Unspecified = 0,
        Numnber = 1,
        ShortText = 2,
        LongText = 3,
        Date = 4,
        DateRange = 5,
        DateTime = 6,
        DateTimeRange = 7,
        Email = 8,
        Phone = 9,
        File = 10,
        Array = 11,
        Name = 12,
        Country = 13
    }

    public enum PaymentMethod : byte
    {
        Unspecified = 0,
        Cash = 1,
        SalaryDeduction = 2
    }

    public enum VacationType : byte
    {
        Unspecified = 0,
        Ordinary = 1,
        Emergency = 2,
        Exceptional = 3,
        ChildCare = 4
    }

    public enum PublishTimeType : byte
    {
        NewPublish = 1,
        Republish = 2
    }

    public enum Degree : byte
    {
        Master = 1,
        Phd = 2
    }

    public enum PublishType : byte
    {
        Governmental = 1,
        Commercial = 2
    }

    public enum BankAccountType : byte
    {
        Local = 1,
        International = 2
    }

    public enum BookType : byte
    {
        PublicScience = 1,
        Systematic = 2,
        Cultural = 3
    }

    public enum BookLanguage : byte
    {
        Arabic = 1,
        TranslatedTo = 2
    }

    public enum HandBookLanguage : byte
    {
        Arabic = 1,
        TranslatedTo = 2
    }

    public enum HandBookType : byte
    {
        Cultural = 1,
        ChildrenStories = 2,
        Other = 3
    }

    public enum ProbeType : byte
    {
        ManuScript = 1,
        RareBook = 2
    }

    public enum EncyclopediaLanguage : byte
    {
        Arabic = 1,
        TranslatedTo = 2
    }

    public enum DictionaryLanguage : byte
    {
        Arabic = 1,
        TranslatedTo = 2
    }

    public enum ScientificMaterialType : byte
    {
        Commercial = 1,
        Governmental = 2
    }

    public enum BankAccountNumberType : byte
    {
        Iban = 1,
        Swift = 2
    }
}