using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APS.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //WriteTerms(@"C:\users\omars\desktop\terms.txt");

            //return;
            CreateReferees(@"C:\Users\GOUSE\Desktop\referees.txt");
            Console.Read();
            return;

            //InsertFakeReferees(1000);
            //return;
            //InsertFakeData();

            //GenerateProgram.GenerateMain();

            //GenerateStateTransitionMachine(@"C:\users\omars\desktop\state_transiition_machine.txt");

            //var roles = new ApsUserRole[]
            //{
            //};
        }

        //public static void InsertFakeReferees(int max)
        //{
        //    var db = Utils.db;

        //    var role = db.Roles.FirstOrDefault(r => r.Name == ApsUserRole.Referee.ToString());
        //    var rand = new Random();
        //    var majors = db.Majors.ToList();

        //    for (var i = 1; i <= max; i++)
        //    {
        //        char gender = ' ';
        //        var name = "";
        //        while (string.IsNullOrWhiteSpace(name) || gender != 'M')
        //        {
        //            name = FakeGenerator.GenerateName(out gender);
        //        }

        //        var comps = Utils.GetNameComponents(name);

        //        var user = new User
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Roles = new List<Role> { role },
        //            UserName = "rf" + i,
        //            Email = "user" + i + "@gmail.com",
        //            SecurityStamp = "a14ee95e-c73e-466e-a786-aaf4378237d3",
        //            PasswordHash = "AFDn7WXfSXvvQ9X29S0t/tqKGG8lS7To/E7zVj7mbaxOoBmbN2xE6JwwPQTz/Td8Fg==",
        //            IsActivated = true,
        //            Created = DateTime.Now,
        //            CreatedBy = "00000000-0000-0000-0000-000000000000",
        //            Updated = DateTime.Now,
        //            UpdatedBy = "00000000-0000-0000-0000-000000000000",
        //        };

        //        db.Users.Add(user);
        //        db.SaveChanges();

        //        var userP = new UserProfile
        //        {
        //            UserId = user.Id,
        //            FirstName = comps.FirstName,
        //            MiddleName = comps.SecondName,
        //            ThirdName = comps.ThirdName,
        //            LastName = comps.LastName,

        //            Created = DateTime.Now,
        //            CreatedBy = "00000000-0000-0000-0000-000000000000",
        //            Updated = DateTime.Now,
        //            UpdatedBy = "00000000-0000-0000-0000-000000000000",
        //        };

        //        db.UserProfiles.Add(userP);
        //        db.SaveChanges();

        //        var @ref = new Referee
        //        {
        //            UserId = user.Id,
        //            FirstName = comps.FirstName,
        //            SecondName = comps.SecondName,
        //            ThirdName = comps.ThirdName,
        //            LastName = comps.LastName,
        //            Email = user.Email,
        //            MajorId = majors[rand.Next(0, majors.Count)].Id,
        //            Created = DateTime.Now,
        //            CreatedBy = "00000000-0000-0000-0000-000000000000",
        //            Updated = DateTime.Now,
        //            UpdatedBy = "00000000-0000-0000-0000-000000000000",
        //        };

        //        db.Referees.Add(@ref);
        //        db.SaveChanges();
        //    }
        //}

        //public static void InsertFakeData()
        //{
        //    var db = Utils.db;

        //    db.Configuration.AutoDetectChangesEnabled = true;
        //    db.Configuration.LazyLoadingEnabled = true;

        //    var x = new List<dynamic>();
        //    x.Add(new { Title = "أحسن التقاسيم في معرفة الأقاليم (مختارات)", Author = "محمد بن أحمد المقدسي" });
        //    x.Add(new { Title = "أخبار المصحفين", Author = "الحسن بن عبد الله العسكري أبو أحمد" });
        //    x.Add(new { Title = "أخبار النحويين", Author = "عبد الواحد بن عمر بن محمد بن أبي هاشم" });
        //    x.Add(new { Title = "أخبار ملوك بني عبيد وسيرتهم", Author = "محمد بن علي بن حماد أبو عبد الله" });
        //    x.Add(new { Title = "أخبار مكة في قديم الدهر وحديثه", Author = "محمد بن إسحاق بن العباس الفاكهي أبو عبد الله" });
        //    x.Add(new { Title = "الآحاد والمثاني", Author = "أحمد بن عمرو بن الضحاك أبو بكر الشيباني" });
        //    x.Add(new { Title = "الألفاظ المختلفة في المعاني المؤتلفة", Author = " محمد بن عبد الملك بن مالك الطائي الجياني أبو عبد الله" });
        //    x.Add(new { Title = " الأوائل", Author = "أحمد بن عمرو بن أبي عاصم الشيباني " });
        //    x.Add(new { Title = "الإعجاز في المدائن القديمة ", Author = "محمد شكور بن محمود الحاجي أمرير" });
        //    x.Add(new { Title = "الإرشاد في معرفة علماء الحديث", Author = "الخليل بن عبد الله بن أحمد الخليلي القزويني أبو يعلى" });
        //    x.Add(new { Title = "الإصابة في تمييز الصحابة", Author = "أحمد بن علي بن حجر أبو الفضل العسقلاني الشافعي" });
        //    x.Add(new { Title = "البدء والتاريخ", Author = " مطهر بن طاهر المقدسي" });

        //    var role = db.Roles.FirstOrDefault(r => r.Name == ApsUserRole.Researcher.ToString());

        //    var i = 0;
        //    foreach (var a in x)
        //    {
        //        i++;
        //        var user = new User
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Roles = new List<Role> { role },
        //            UserName = "r" + i,
        //            Email = "user" + i + "@gmail.com",
        //            SecurityStamp = "a14ee95e-c73e-466e-a786-aaf4378237d3",
        //            PasswordHash = "AFDn7WXfSXvvQ9X29S0t/tqKGG8lS7To/E7zVj7mbaxOoBmbN2xE6JwwPQTz/Td8Fg==",
        //            IsActivated = true,
        //            Created = DateTime.Now,
        //            CreatedBy = "91698DCE-BAA8-469E-906D-EF55D528321B",
        //            Updated = DateTime.Now,
        //            UpdatedBy = "91698DCE-BAA8-469E-906D-EF55D528321B",
        //        };

        //        db.Users.Add(user);
        //        db.SaveChanges();
        //        var comps = Utils.GetNameComponents((string)a.Author);

        //        user.Researchers.Add(new Data.DbModels.Researcher()
        //        {
        //            FirstName = comps.FirstName,
        //            SecondName = comps.SecondName,
        //            ThirdName = comps.ThirdName,
        //            LastName = comps.LastName,
        //            Email = "abdulrahman@gmail.com",
        //            MobileNo = "0595657226",
        //            Address = "الرياض- حي التعاون",
        //            //Bank = new Data.DbModels.Bank
        //            //{
        //            //    NameArabic = "بنك الراجحي"
        //            //},
        //            City = new Data.DbModels.City
        //            {
        //                NameArabic = "الرياض",
        //                Country = new Data.DbModels.Country
        //                {
        //                    NameArabic = "المملكة العربية السعودية",

        //                },
        //            },
        //            College = new Data.DbModels.College
        //            {
        //                NameArabic = "كلية التربية",

        //                Created = DateTime.Now,
        //                CreatedBy = user.Id,
        //                Updated = DateTime.Now,
        //                UpdatedBy = user.Id,
        //            },

        //            DateOfBirth = new DateTime(1985, 1, 1),

        //            Created = DateTime.Now,
        //            CreatedBy = user.Id,
        //            Updated = DateTime.Now,
        //            UpdatedBy = user.Id,
        //        });

        //        db.SaveChanges();

        //        db.Requests.Add(new Data.DbModels.Request
        //        {
        //            UniqueId = "1",
        //            Type = (byte)RequestType.PublishRequest,
        //            PublicationType = (byte)PublicationType.Book,
        //            Status = (byte)RequestStatus.PublishRequestPendingCoresearchers,
        //            BookRequest = new Data.DbModels.BookRequest
        //            {
        //                BookTitle = a.Title,
        //                IsTranslated = false,
        //                PublishingDate = new DateTime(2010, 1, 1),
        //                //Major = new Data.DbModels.Major
        //                //{
        //                //    NameArabic = "علوم ثقافية",

        //                //    Created = DateTime.Now,
        //                //    CreatedBy = user.Id,
        //                //    Updated = DateTime.Now,
        //                //    UpdatedBy = user.Id,
        //                //},
        //                NoOfRecommendations = 3,
        //                OriginalTitleBook = a.Title,
        //                IsArabic = true,
        //                RecommendationDate = new DateTime(2016, 1, 1),
        //                BookType = (byte)BookType.Cultural,
        //                Created = DateTime.Now,
        //                CreatedBy = user.Id,
        //                Updated = DateTime.Now,
        //                UpdatedBy = user.Id,
        //            },
        //            Created = DateTime.Now,
        //            CreatedBy = user.Id,
        //            Updated = DateTime.Now,
        //            UpdatedBy = user.Id,

        //            RequestHistoryLogs = new List<RequestHistoryLog>
        //        {
        //            new RequestHistoryLog()
        //            {
        //                Action = (byte) RequestAction.Submit,
        //                IpAddress= "192.168.1.1",
        //                CurrentlyWith =  "91698DCE-BAA8-469E-906D-EF55D528321B",
        //                Comments = "لا يوجد ملاحظات",
        //                Created = DateTime.Now,
        //                CreatedBy = user.Id,

        //            }

        //        }

        //        });

        //        db.SaveChanges();
        //    }
        //}

        //public static void GenerateStateTransitionMachine(string outputFilePath)
        //{
        //    var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);

        //    var roles = Enum.GetValues(typeof(ApsUserRole));
        //    var actions = Enum.GetValues(typeof(RequestAction));
        //    var states = Enum.GetValues(typeof(RequestStatus));

        //    writer.WriteLine("{0}\t{1}\t{2}", "State", "Role", "Action");

        //    foreach (var s in states)
        //    {
        //        foreach (var a in actions)
        //        {
        //            foreach (var r in roles)
        //            {
        //                writer.WriteLine("{0}\t{1}\t{2}", s.ToString(), r.ToString(), a.ToString());
        //            }
        //        }
        //    }

        //    writer.Close();
        //}

        public static void CreateReferees(string inputFile)
        {
            var reader = new StreamReader(inputFile);
            var lines = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();

            var db = Utils.db;
            var emails = new List<string>();
            var names = new List<NameComponents>();
            var role = db.Roles.FirstOrDefault(r => r.Name == ApsUserRole.Referee.ToString());

            var countries = db.Countries.ToList();
            var referees = db.Referees.ToList();
            var majors = db.Majors.ToList().Select(x => new { Id = x.Id, Name = Utils.NormalizeString(x.NameArabic) }).ToList();

            var lastRefId = db.Users.FirstOrDefault(r => r.UserName.StartsWith("REF_"));
            var userIdStart = lastRefId == null ? 1 : int.Parse(lastRefId.UserName.Split('_')[1]) + 1;
            Console.WriteLine("Reading and Writing..");
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var tabs = line.Split('\t');

                if (tabs.Length < 10) continue;

                var no = tabs[0];
                var name = tabs[1];
                var nationality = tabs[2];
                var universityOther = tabs[3];
                var qualification = tabs[4];
                var major = tabs[5];
                var mobile = tabs[6];
                var email = tabs[7];
                var iban = tabs[8];
                var comments = tabs[9];

                if (string.IsNullOrWhiteSpace(email))
                    email = null;
                else
                {
                    email = email.ToLower().Trim();

                    if (!Utils.ValidateEmail(email))
                    {
                        email = null;
                    }
                }

                var comps = Utils.GetNameComponents(name);

                if (comps == null || comps.FirstName == null || comps.LastName == null) continue;
                if (emails.Contains(email)) continue;

                if (referees.Count(r => r.FirstName == comps.FirstName && r.LastName == comps.LastName && (email == null || email == r.Email)) > 0) continue;
                if (names.Count(n => n.FirstName == comps.FirstName && n.LastName == comps.LastName && n.ThirdName == comps.ThirdName && n.SecondName == comps.SecondName) > 0) continue;

                emails.Add(email);
                names.Add(comps);

                var majorId = (int?)null;

                if (!string.IsNullOrWhiteSpace(major))
                {
                    var m = majors.FirstOrDefault(x => x.Name == Utils.NormalizeString(major));
                    if (m != null)
                    {
                        majorId = m.Id;
                    }
                }

                if (string.IsNullOrWhiteSpace(universityOther))
                {
                    universityOther = null;
                }
                else
                {
                    universityOther = universityOther.Trim();
                }

                if (string.IsNullOrWhiteSpace(major))
                {
                    major = null;
                }
                else
                {
                    major = major.Trim();
                }

                var qualificationType = (QualificationType?)null;
                if (string.IsNullOrWhiteSpace(qualification))
                {
                    qualification = null;
                }
                else
                {
                    qualification = qualification.Trim();

                    if (qualification.StartsWith("أستاذ مشارك") || qualification.StartsWith("استاذ مشارك"))
                    {
                        qualificationType = QualificationType.AssociateProfessor;
                    }
                    else if (qualification.StartsWith("أستاذ مساعد") || qualification.StartsWith("استاذ مساعد"))
                    {
                        qualificationType = QualificationType.AssistantProfessor;
                    }
                    else if (qualification.StartsWith("أستاذ") || qualification.StartsWith("استاذ"))
                    {
                        qualificationType = QualificationType.Professor;
                    }
                }

                if (string.IsNullOrWhiteSpace(iban))
                {
                    iban = null;
                }
                else
                {
                    iban = iban.Trim().ToUpper();

                    if (!Utils.ValidateIBAN(iban))
                    {
                        iban = null;
                    }
                }

                var nationalityId = (int?)null;
                if (string.IsNullOrWhiteSpace(nationality))
                {
                    nationality = null;
                }
                else
                {
                    nationality = nationality.Trim().ToUpper();

                    nationalityId = countries.Where(c => c.NameArabic == nationality || c.NameEnglish == nationality).Select(c => c.Id).FirstOrDefault();

                    if (nationalityId == 0)
                        nationalityId = null;
                }

                var username = "REF_" + (userIdStart++) + "";
                var password = Helpers.GeneratePassword(12);

                var validPhone = (string)null;

                if (!string.IsNullOrWhiteSpace(mobile))
                {
                    if (!Utils.ValidateMobile(mobile, out validPhone))
                    {
                        var tabs2 = mobile.Split('/', '-');
                        foreach (var t in tabs2)
                        {
                            if (Utils.ValidateMobile(t, out validPhone))
                            {
                                mobile = validPhone;
                                break;
                            }
                        }
                    }
                }

                var fullName = comps.FirstName;

                if (comps.SecondName != null)
                    fullName += " " + comps.SecondName;

                if (comps.ThirdName != null)
                    fullName += " " + comps.ThirdName;

                if (comps.LastName != null)
                    fullName += " " + comps.LastName;

                var userId = (string)null;
                Console.WriteLine("Writing Referee " + fullName);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Roles = new List<Role> { role },
                        UserName = username,
                        Email = email,
                        PhoneNumber = validPhone,
                        MajorId = majorId,
                        MajorOther = majorId > 0 ? null : major,
                        UniversityOther = universityOther,
                        FullName = fullName,
                        SecurityStamp = "a14ee95e-c73e-466e-a786-aaf4378237d3",
                        PasswordHash = "AFDn7WXfSXvvQ9X29S0t/tqKGG8lS7To/E7zVj7mbaxOoBmbN2xE6JwwPQTz/Td8Fg==",
                        IsActivated = true,
                        Created = DateTime.Now,
                        CreatedBy = "00000000-0000-0000-0000-000000000000",
                        Updated = DateTime.Now,
                        UpdatedBy = "00000000-0000-0000-0000-000000000000",
                    };

                    db.Users.Add(user);
                    db.SaveChanges();
                    userId = user.Id;
                    var userP = new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = comps.FirstName,
                        MiddleName = comps.SecondName,
                        ThirdName = comps.ThirdName,
                        LastName = comps.LastName,
                        MobileNo = validPhone,
                        MobileNumber = validPhone,
                        BankIban = iban,
                        Created = DateTime.Now,
                        CreatedBy = "00000000-0000-0000-0000-000000000000",
                        Updated = DateTime.Now,
                        UpdatedBy = "00000000-0000-0000-0000-000000000000",
                    };

                    db.UserProfiles.Add(userP);
                    db.SaveChanges();
                }

                var @ref = new Referee
                {
                    UserId = userId,
                    FirstName = comps.FirstName,
                    SecondName = comps.SecondName,
                    ThirdName = comps.ThirdName,
                    LastName = comps.LastName,
                    Email = email,
                    MobileNo = validPhone,
                    MajorId = majorId,
                    NationalityId = nationalityId,
                    Qualification = (byte?)qualificationType,
                    Created = DateTime.Now,
                    CreatedBy = "00000000-0000-0000-0000-000000000000",
                    Updated = DateTime.Now,
                    UpdatedBy = "00000000-0000-0000-0000-000000000000",
                };

                db.Referees.Add(@ref);
                db.SaveChanges();
            }
            Console.WriteLine("Db is updated...");
        }

        //        public static void WriteTerms(string outputFilePath)
        //        {
        //            var dict = new List<string>()
        //            {
        //               "ActionSubmit	إرسال",
        //"ActionApprove	موافقة",
        //"ActionReject	رفض",
        //"ActionApproveWithComments	موافقة مع ملاحظات ",
        //"ActionForward	إعادة توجيه ",
        //"ActionResubmit	إعادة إرسال",
        //"ActionSubmitCoresearcherData	إرسال بيانات الباحث",
        //"ActionCancel	إلغاء",
        //"ActionReturnToResearcher	إعادة للباحث",
        //"ActionForwardToInitialReferee	إرسال للمحكم المبدئي",
        //"ActionPublish	نشر",
        //"ActionReturnToResearchUnitHead	إعادة إلى رئيس وحدة البحوث",
        //"ActionReturnToViceDeanOfAcademicResearch	إعادة إلى وكيل العمادة للنشر العلمي  ",
        //"ActionSelectReferees	اختيار محكمين",
        //"ActionForwardToPressOficer	إرسال إلى موظف مطابع الجامعة",
        //"ActionForwardToProofreader	إرسال إلى المصحح",
        //"ActionForwardToResearchUnitSecretary	إعادة توجيه إلى أمين وحدة البحوث ",
        //"ActionForwardToDeanOfAcademicResearch	إرسال إلى عميد البحث العلمي ",
        //"ActionForwardToSpareReferee	إرسال إلى المحكم الإحتياطي",
        //"ActionApproveWithNoReferees	موافقة من غير محكمين ",
        //"ActionForwardToResearchUnitHead	إرسال إلى رئيس وحدة البحوث ",
        //"ActionApproveAndForwardToResearchUnitHead	موافقة وإرسال إلى رئيس وحدة البحوث",
        //"ActionAccept	موافق ",
        //"ActionSendToCommiteeMembers	إرسال إلى أعضاء اللجنة ",
        //"ActionApproveAndForwardToDeanOfAcademicResearch	موافقة وإرسال إلى عميد البحث العلمي",
        //"ActionRejectAndForwardToDeanOfAcademicResearch	رفض وإرسال إلى عميد البحث العلمي ",
        //"ActionForwardToDeanshipOfAcademicResearchCommiteeMembers	إرسال إلى أعضاء لجنة  عمادة البحث العلمي ",
        //"ActionApproveAndForwardToUnversityRector	موافقة وإعادة توجيه لمدير الجامعة ",
        //"ActionRejectAndForwardToUniversityRector	رفض وإعادة توجيه إلى مدير الجامعة ",
        //"ActionReturnToResearchUnitSecretary	إعادة إلى أمين وحدة البحوث",
        //"ActionReturnToDeanshipOfAcademicResearch	إعادة إلى عميد البحث العلمي ",
        //"ActionApproveApproval	اعتماد الموافقة ",
        //"ActionForwardToPAECommitteeHead	إرسال إلى رئيس لجنة النشر والتحكيم والمعادلات ",
        //"ActionForwardToAcademicCouncilHead	إرسال إلى رئيس المجلس العلمي ",
        //"ActionApproveAndReturnToPAECommitee	موافقة وإعادة للجنة النشر والتحكيم والمعادلات",
        //"ActionRejectAndReturnToPAECommitee	رفض وإعادة إلى أعضاء لجنة النشر والتحكيم و المعادلات ",
        //"ActionForwardToMainReferees	إرسال إلى المحكمان  الأساسيان ",
        //"ActionReturnToAcademdicCouncil	إعادة إلى المجلس العلمي ",
        //"ActionReturnToPAECommittee	إعادة إلى لجنة النشر والتحكيم والمعادلات",
        //"ActionApproveRejection	اعتماد الرفض ",
        //"ActionResendToReferees	إرسال إلى المحكمين",
        //"ActionForwardToAcademicCouncil	إرسال إلى المجلس العلمي ",
        //"ActionApproveAndForwardToViceDeanOfAcademicResearch	موافقة وإرسال إلى وكيل عمادة للبحث العلمي ",
        //"ActionRejectAndForwardToViceDeanOfAcademicResearch	رفض وإرسال وكيل عمادة البحث العلمي",
        //"Action		",
        //"ActionApproveAndForwardToAcademicCouncil	موافقة وإرسال إلى المجلس العلمي ",
        //"ActionRejectAndForwardToAcademicCouncil	رفض وإرسال إلى المجلس العلمي ",
        //"ActionApproveWithCommentsAndForwardToAcademicCouncil	موافقة مع ملاحظات وإرسال إلى المجلس العلمي ",
        //"ActionProvideFeedback	إبداء الرأي",
        //"ActionReturnToPAECommitteeSecretary	إرجاع إلى أمين لجنة النشر والتحكيم والمعادلات ",
        //"ActionForwardToReferees	إرسال للمحكمين ",
        //"ActionApproveModifications	الموافقة على التعديلات ",
        //"ActionRejectModifications	رفض التعديلات ",
        //"ActionApproveRejectionAndForwardToUniversityRector	موافقة وإعادة توجيه لمدير الجامعة  ",
        //"ActionApproveApprovalAndForwardToUniversityRector	اعتماد الموافقة وإرسال إلى مدير الجامعة ",
        //"ActionRejectAndReturnToResearcher	رفض وإرجاع للباحث ",
        //"ActionResubmitAndReturnToCoresearchers	إعادة إرسال للمشاركين في البحث",
        //"ActionReturnToAcademdicCouncilSecretary	إعادة إلى أمين المجلس العلمي ",
        //"ActionResubmitAndForwardToPAECommitee	إعادة إرسال وإرسال إلى لجنة النشر النشر والتحكيم والمعادلات ",
        //"ActionForwardToViceDeanOfAcademicResearch	إرسال إلى وكيل عمادة البحث العلمي ",
        //"ActionReturnToOfficerOfPrintingUnit	إر جاع إلى موظف وحدة التجهيز الطباعي ",
        //"ActionApproveAndForwardToUniversityPressDirector	موافقة وإرسال إلى مدير مطابع الجامعة ",
        //"ActionRejectAndReturnToOfficerOfPrintingUnit	رفض وإرجاع لموظف وحدة التجهيز الطباعي ",
        //"ActionCertifyAndForwardToUniversityPressOfficer	تصديق وإعادة إرسال إلى موظف مطابع الجامعة ",
        //"ActionOperationCommandExecutedAndForwardToPUAP	تنفيذ أمر التشغيل وإرسال إلى أمين لجنة التجهيز الطباعي ",
        //"ActionReturnToUniversityPressDirector	إرجاع لموظف مطابع الجامعة ",
        //"ActionReturnToUniversityPressOfficer	إعادة إلى موظف مطابع الجامعة ",
        //"ActionForwardToResearcher	إرسال إلى الباحث ",
        //"ActionReturnToProofreader	إعادة للمصحح ",
        //"ActionSubmitOperationCommandToViceDeanOfAcademicResearch	إرسال طلب أمر التشغيل إلى وكيل عمادة البحث العلمي ",
        //"ActionReturnToPUAP	إعادة إلى أمين لجنة التجهيز الطباعي ",
        //"ActionCloseRequest	إقفال الطلب",
        //"Action		",
        //"ActionSubmitISBNRequestToKFNL	رفع طلب ردمك إلى مكتبة الملك فهد الوطنية ",
        //"ActionSubmitISBNToImamUniversity	إرسال رقم الردمك إلى جامعة الإمام محمد بن سعود الإسلامية ",
        //"ActionReturnToKFNLOfficer	إرجاع إلى موظف مكتبة الملك فهد ",
        //"ActionRejectAndReturnToPUAP	إعادة إلى أمين لجنة التجهيز الطباعي ",
        //"ActionRejectAndReturnToViceDeanOfAcademicResearch	رفض وإعادة إلى وكيل عمادة البحث العلمي ",
        //"ActionSubmitToPressOfficer	إرسال إلى موظف المطابع ",
        //"ActionPrintingCompleted	تمت الطباعة ",
        //"ActionApproveAndForwardToFiancialDepartmentDirector	موافقة وإرسال إلى مدير الإدارة المالية ",
        //"ActionApproveAndForwardToAccountant	موافقة وإرسال إلى المحاسب ",
        //"ActionRejectAndReturnToDeanOfAcademicResearch	رفض وإرجاع إلى عميد البحث العلمي ",
        //"ActionPaymentCompleted	صرف المكافأة ",
        //"ActionPrepareDistributionList	تجهيز قوائم التوزيع",
        //"ActionConfirmReceivalOfCopies	تأكيد إستلام النسخ ",
        //"ActionConfirmDeliveringCopiesToResaercher	تاكيد إستلام الباحث للنسخ ",
        //"ActionForwardToKFNL	إرسال إلى موظف مكتبة الملك فهد ",
        //"ActionConfirmDeliveringCopiesToKFNL	تأكيد إستلام مكتبة الملك فهد النسخ ",
        //"ActionConfirmDeliveringCopiesToAgencies	تأكيد إستلام نسخ الجهات ",
        //"ActionForwardToUniversityPressDirector	إرسال إلى مدير مطابع الجامعة ",
        //"ActionApproveAndForwardToDeanshipOfLibraryAffairs	موافقة وإرسال إلى عميد شؤون المكتبات ",
        //"ActionApproveAndAndForardToDeanOfAcademicResearch	موافقة وإرسال إلى عميد البحث العلمي ",
        //"ActionReturnToDeanshipOfLibraryAffairs	إعادة إلى عميد شؤون المكتبات ",
        //"ActionCanceled	إلغاء",
        //"HistorySubmit	تم الإرسال ",
        //"HistoryApprove	تمت الموافقة",
        //"HistoryReject	رُفضت",
        //"HistoryApproveWithComments	تمت الموافقة مع تعديلات",
        //"HistoryForward	تم الإرسال ",
        //"HistoryResubmit	تمت إعادة الإرسال",
        //"HistorySubmitCoresearcherData	طلب النشر بانتظار ",
        //"HistoryCancel	تم الإلغاء",
        //"HistoryReturnToResearcher	إعادة للباحث",
        //"HistoryForwardToInitialReferee	إرسال إلى المحكم المبدئي",
        //"HistoryPublish	نشر",
        //"HistoryReturnToResearchUnitHead	إعادة إلى رئيس وحدة البحوث",
        //"HistoryReturnToViceDeanOfAcademicResearch	إعادة إلى وكيل العمادة للنشر العلمي",
        //"HistorySelectReferees	اختيار محكمين",
        //"HistoryForwardToPressOficer	تم الإرسال إلى موظف مطابع الجامعة",
        //"HistoryForwardToProofreader	تم الإرسال إلى المصحح",
        //"HistoryForwardToResearchUnitSecretary	تم الإرسال إلى أمين وحدة البحوث",
        //"HistoryForwardToDeanOfAcademicResearch	تم الإرسال إلى عميد البحث العلمي",
        //"HistoryForwardToSpareReferee	تم الإرسال إلى المحكم الاحتياطي",
        //"HistoryApproveWithNoReferees	تمت الموافقة بدون تحكيم",
        //"HistoryForwardToResearchUnitHead	تم الإرسال إلى رئيس وحدة البحوث",
        //"HistoryApproveAndForwardToResearchUnitHead	تمت الموافقة والإرسال إلى رئيس وحدة البحوث",
        //"HistoryAccept	تمت الموافقة",
        //"HistorySendToCommiteeMembers	تم الإرسال إلى أعضاء اللجنة",
        //"HistoryApproveAndForwardToDeanOfAcademicResearch	تمت الموافقة والإرسال إلى عميد  البحث العلمي",
        //"HistoryRejectAndForwardToDeanOfAcademicResearch	تم الرفض والإرسال إلى عميد البحث العلمي",
        //"HistoryForwardToDeanshipOfAcademicResearchCommiteeMembers	تمت الإرسال إلى مجلس عمادة البحث العلمي",
        //"HistoryApproveAndForwardToUnversityRector	تمت الموافقة والإرسال إلى مدير الجامعة",
        //"HistoryRejectAndForwardToUniversityRector	تم الرفض والإرسال إلى مدير الجامعة",
        //"HistoryReturnToResearchUnitSecretary	تمت الإعادة إلى أمين وحدة البحوث",
        //"HistoryReturnToDeanshipOfAcademicResearch	تمت الإعادة إلى عميد البحث العلمي",
        //"HistoryApproveApproval	تم الاعتماد",
        //"HistoryForwardToPAECommitteeHead	تم الإرسال إلى رئيس لجنة النشر والتحكيم والمعادلات",
        //"HistoryForwardToAcademicCouncilHead	تم الإرسال إلى رئيس المجلس العلمي",
        //"HistoryApproveAndReturnToPAECommitee	تمت الموافقة والإعادة إلى لجنة النشر والتحكيم والمعادلات",
        //"HistoryRejectAndReturnToPAECommitee	تم الرفض والإعادة إلى لجنة النشر والتحكيم والمعادلات",
        //"HistoryForwardToMainReferees	تم الإرسال إلى المحكمين الأساسيين",
        //"HistoryReturnToAcademdicCouncil	تم الإعادة إلى المجلس العلمي",
        //"HistoryReturnToPAECommittee	تم الإعادة إلى لجنة النشر والتحكيم والمعادلات",
        //"HistoryApproveRejection	تم اعتماد الرفض",
        //"HistoryResendToReferees	تم إعادة الإرسال للمحكمين",
        //"HistoryForwardToAcademicCouncil	تم الإرسال إلى المجلس العلمي",
        //"HistoryApproveAndForwardToViceDeanOfAcademicResearch	تمت الموافقة والإرسال إلى وكيل العمادة للنشر العلمي",
        //"HistoryRejectAndForwardToViceDeanOfAcademicResearch	تم الرفض والإرسال إلى وكيل العمادة للنشر العلمي",
        //"HistoryApproveAndForwardToAcademicCouncil	تمت الموافقة والإرسال إلى المجلس العلمي",
        //"HistoryRejectAndForwardToAcademicCouncil	تم الرفض والإرسال إلى المجلس العلمي",
        //"HistoryApproveWithCommentsAndForwardToAcademicCouncil	تمت الموافقة مع تعديلات، والإرسال إلى المجلس العلمي",
        //"HistoryProvideFeedback	تم إبداء الرأي",
        //"HistoryReturnToPAECommitteeSecretary	تم الإعادة إلى أمين لجنة النشر والتحكيم والمعادلات",
        //"HistoryForwardToReferees	تم الإرسال إلى المحكمين ",
        //"HistoryApproveModifications	تمت الموافقة على تعديلات الباحث",
        //"HistoryRejectModifications	تم رفض تعديلات الباحث",
        //"HistoryApproveRejectionAndForwardToUniversityRector	تم اعتماد الرفض والإرسال إلى مدير الجامعة",
        //"HistoryApproveApprovalAndForwardToUniversityRector	تم اعتماد الموافقة والإرسال إلى مدير الجامعة",
        //"HistoryRejectAndReturnToResearcher	تم الرفض والإعادة إلى الباحث",
        //"HistoryResubmitAndReturnToCoresearchers	تم إعادة الإرسال للمشاركين في المادة العلمية",
        //"HistoryReturnToAcademdicCouncilSecretary	تم الإعادة إلى أمين المجلس العلمي",
        //"HistoryResubmitAndForwardToPAECommitee	تم إعادة الإرسال إلى لجنة النشر والتحكيم والمعادلات",
        //"HistoryForwardToViceDeanOfAcademicResearch	تم الإرسال إلى وكيل العمادة للنشر العلمي",
        //"HistoryReturnToOfficerOfPrintingUnit	تم الإعادة إلى موظف وحدة التجهيز الطباعي",
        //"HistoryApproveAndForwardToUniversityPressDirector	تمت الموافقة والإرسال إلى مدير مطابع الجامعة",
        //"HistoryRejectAndReturnToOfficerOfPrintingUnit	تم الرفض والإعادة إلى موظف وحدة التجهيز الطباعي",
        //"HistoryCertifyAndForwardToUniversityPressOfficer	تمت المصادقة، والإرسال إلى موظف مطابع الجامعة",
        //"HistoryOperationCommandExecutedAndForwardToPUAP	تم تنفيذ أمر التشغيل، والإرسال إلى موظف وحدة التجهيز الطباعي",
        //"HistoryReturnToUniversityPressDirector	تم الإعادة إلى مدير مطابع الجامعة",
        //"HistoryReturnToUniversityPressOfficer	تم الإعادة إلى موظف مطابع الجامعة",
        //"HistoryForwardToResearcher	تم الإرسال إلى الباحث",
        //"HistoryReturnToProofreader	تم الإعادة إلى المصحح",
        //"HistorySubmitOperationCommandToViceDeanOfAcademicResearch	تم إرسال أمر التشغيل إلى وكيل العمادة للنشر العلمي",
        //"HistoryReturnToPUAP	تم الإعادة إلى موظف وحدة التجهيز الطباعي",
        //"HistoryAcceptWithComments	تمت الموافقة مع تعديلات",
        //"HistoryCloseRequest	تم إغلاق الطلب",
        //"HistorySubmitISBNRequestToKFNL	تم إرسال طلب الردمك إلى مكتبة الملك الفهد الوطنية",
        //"HistorySubmitISBNToImamUniversity	تم إرسال الردمك إلى جامعة الإمام محمد بن سعود الإسلامية",
        //"HistoryReturnToKFNLOfficer	تمت الإعادة إلى موظف مكتبة الملك فهد الوطنية",
        //"HistoryRejectAndReturnToPUAP	تم الرفض والإعادة إلى موظف وحدة التجهيز الطباعي",
        //"HistoryRejectAndReturnToViceDeanOfAcademicResearch	تم الرفض والإعادة إلى وكيل العمادة للنشر العلمي",
        //"HistorySubmitToPressOfficer	تم الإرسال إلى موظف مطابع الجامعة",
        //"HistoryPrintingCompleted	تمت الطباعة",
        //"HistoryApproveAndForwardToFiancialDepartmentDirector	تمت الموافقة والإرسال إلى مدير الإدارة المالية",
        //"HistoryApproveAndForwardToAccountant	تمت الموافقة والإرسال إلى المحاسب",
        //"HistoryRejectAndReturnToDeanOfAcademicResearch	تم الرفض والإعادة إلى عميد البحث العلمي",
        //"HistoryPaymentCompleted	تم صرف المكافأة",
        //"HistoryPrepareDistributionList	تم إعداد قائمة التوزيع للكتاب",
        //"HistoryConfirmReceivalOfCopies	تم تأكيد استلام نسخ الكتاب",
        //"HistoryConfirmDeliveringCopiesToResaercher	تم تأكيد استلام نسخ الكتاب من قِبل الباحث",
        //"HistoryForwardToKFNL	تم الإرسال إلى مكتبة الملك فهد الوطنية",
        //"HistoryConfirmDeliveringCopiesToKFNL	تم تأكيد استلام نسخ الكتاب من قِبل مكتبة الملك فهد الوطنية",
        //"HistoryConfirmDeliveringCopiesToAgencies	تم تأكيد استلام النسخ لجهات التوزيع",
        //"HistoryForwardToUniversityPressDirector	تم الإرسال إلى مدير مطابع الجامعة",
        //"HistoryApproveAndForwardToDeanshipOfLibraryAffairs	تمت الموافقة والإرسال إلى عميد شؤون المكتبات",
        //"HistoryApproveAndAndForardToDeanOfAcademicResearch	تمت الموافقة والإرسال إلى عميد البحث العلمي",
        //"HistoryReturnToDeanshipOfLibraryAffairs	تمت الإعادة إلى عميد شؤون المكتبات",
        //"HistoryCanceled	تم الإلغاء",
        //"StatePublishRequestNotSubmitted	لم يتم إرسال الطلب ",
        //"StatePublishRequestPendingModificationsAfterRUS	في انتظار تعديلات الباحث ",
        //"StatePublishRequestPendingModificationsAfterReferees	في انتظار تعديلات الباحث بعد التحكيم ",
        //"StatePublishRequestPendingCoresearchers	في انتظار المشاركين في المادة العلمية ",
        //"StatePublishRequestRejectedByCoresearchers	طلب النشر رفض بواسطة المشاركون بالبحث",
        //"StatePublishRequestPendingResearchUnitSecretary	في انتظار أمين وحدة البحوث ",
        //"StatePublishRequestPendingResearchUnitHead	في انتظار  رئيس وحدة البحوث ",
        //"StatePublishRequestPendingResearchUnitCommittee	في انتظار  لجنة النشر",
        //"StatePublishRequestPendingInitialReferee	في انتظار  المحكم المبدئي ",
        //"StatePublishRequestPendingViceDeanOfAcademicResearchPublishApproval	في انتظار  وكيل العمادة للنشر العلمي ",
        //"StatePublishRequestPendingDeanOfAcademicResearchPublishApproval	في انتظار  موافقة عميد البحث العلمي ",
        //"StatePublishRequestPendingDeanshipOfAcademicResearchCommitee	في انتظار لجنة عمادة البحث العلمي ",
        //"StatePublishRequestPendingUniversityRectorPublishApproval	في انتظار موافقة اعتماد مدير الجامعة ",
        //"StatePublishRequestPendingPAERefereeSelection	في انتظار اختيار المحكمين من قبل لجنة النشر والتحكيم والمعادلات ",
        //"StatePublishRequestPendingPAERefereeSelectionViceDean	في انتظار اختيار المحكمين من قبل عميد عمادة النشر العلمي",
        //"StatePublishRequestPendingPAERefereeSelectionHead	في انتظار اختيار رئيس لجنة النشر والتحكيم والمعادلات للمحكمين ",
        //"StatePublishRequestPendingPAERefereeSelectionMembers	في انتظار اختيار أعضاء لجنة النشر والتحكيم والمعادلات للمحكمين ",
        //"StatePublishRequestPendingAcademicCouncilRefereeSelection	في انتظار اختيار المحكمين من قبل المجلس العلمي ",
        //"StatePublishRequestPendingAcademicCouncilRefereeSelectionMembers	في انتظار اختيار المحكمين من قبل أعضاء المجلس العلمي ",
        //"StatePublishRequestPendingAcademicCouncilRefereeSelectionHead	في انتظار اختيار المحكمين من قبل رئيس المجلس العلمي ",
        //"StatePublishRequestPendingUniversityRectorAFterRefereeSelection	في انتظار موافقة مدير الجامعة على اختيار المحكمين  ",
        //"StatePublishRequestPendingPAEAfterRefereeSelectionViceDean	في انتظار اختيار المحكمين من قبل وكيل عمادة البحث العلمي ",
        //"StatePublishRequestPendingPAEAfterRefereeSelection	في انتظار لجنة النشر والتحكيم والمعادلات بعد التحكيم ",
        //"StatePublishRequestPendingReferees	في انتظار المحكمين",
        //"StatePublishRequestPendingPAEAfterReferees	في انتظار لجنة النشر والتحكيم والمعادلات بعد التحكيم ",
        //"StatePublishRequestPendingPAEAfterRefereesViceDean	في انتظار وكيل عمادة النشر العلمي",
        //"StatePublishRequestPendingPAEAfterRefereesHead	في انتظار رئيس لجنة النشر والتحكيم والمعادلات",
        //"StatePublishRequestPendingPAEAfterRefereesMembers	في انتظار أعضاء لجنة النشر والتحكيم والمعادلات ",
        //"StatePublishRequestPendingSpareReferee	في انتظار المحكم الإحتياطي",
        //"StatePublishRequestPendingAcademicCouncilAfterReferees	في انتظار المجلس العلمي بعد التحكيم ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesHead	في انتظار رئيس المجلس العلمي  ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesMembers	في انتظار  أعضاء المجلس العلمي ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApprove	في انتظار موافقة المجلس العلمي ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApproveHead	في انتظار موافقة رئيس المجلس العلمي  ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApproveMembers	في انتظار موافقة أعضاء المجلس العلمي ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesReject	تم الرفض من قبل المجلس العلمي",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesRejectHead	تم الرفض من قبل  رئيس المجلس العلمي",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesRejectMembers	تم الرفض من قبل أعضاء المجلس ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApproveWithComments	موافقة المجلس العلمي مع وجود ملاحظات ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsHead	موافقة رئيس المجلس العلمي مع وجود ملاحظات ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsMembers	موافقة أعضاء المجلس العلمي مع وجود ملاحظات ",
        //"StatePublishRequestPendingPAECouncilAfterRefereesApproveWithComments	موافقة لجنة النشر والتحكيم والمعادلات ",
        //"StatePublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsViceDean	موافقة وكيل عمادة النشر العلمي ",
        //"StatePublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsHead	موافقة رئيس لجنة النشر والتحكيم والمعادلات مع وجود ملاحظات ",
        //"StatePublishRequestPendingPAEAfterRefereesAndResearcherModifications	في انتظار تعديلات الباحث ",
        //"StatePublishRequestPendingPAEAfterRefereesAndResearcherModificationsViceDean	في انتظار وكيل عمادة  البحث العلمي ",
        //"State	PublishRequestPendingPAEAfterRefereesAndResearcherModificationsHead	",
        //"State	PublishRequestPendingRefereesAfterRefereesAndResearcherModifications	",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesRectorApproved	تمت الموافقة من قبل مدير الجامعة ",
        //"StatePublishRequestPendingAcademicCouncilAfterRefereesRectorRejected	تم الرفض من قبل مدير الجامعة ",
        //"StateOperationCommandPendingOperationCommandInitiation	في انتظار اعطاء امر بدء التشغيل من قبل وكيل العمادة للنشر العلمي",
        //"StateOperationCommandPendingOperationCommandInitiationAfterProofReading	في انتظار اعطاء امر بدء التشغيل من قبل وكيل العمادة للنشر العلمي",
        //"State	OperationCommandPendingOperationCommandPreparation	",
        //"State	OperationCommandPendingOperationCommandApproval	",
        //"State	OperationCommandPendingOperationCommandExecution	",
        //"State	OperationCommandPendingOperationCommandValidation	",
        //"StateOperationCommandPendingResearcherAcceptance	في موافقة الباحث على طلب التشغيل",
        //"StateOperationCommandPendingISBNRequestSubmission	في انتظار إرسال طلب رقم  الردمك ",
        //"StateOperationCommandPendingProofreading	في انتظار المصحح ",
        //"StateOperationCommandPendingOperationCommandFinalApproval	اعتماد طلب التشغيل",
        //"StateOperationCommandPendingPrintingManager	في انتظار مدير المطابع ",
        //"StateOperationCommandPendingOperationCommandForward	إعادة توجيه أمر التشغيل ",
        //"StateOperationCommandPendingPendingCorrection	في انتظار التصحيح",
        //"StateOperationCommandPendingPrinting	في انتظار الطباعة ",
        //"StateOperationCommandPendingResearcherProofreadingAcceptance	في انتظار موافقة المصحح ",
        //"StateOperationCommandPendingOperationCommandForwardResearcherRejection	في انتظار إعادة توجيه رفض الباحث ",
        //"StateOperationCommandPendingResearcherComments	في انتظار تعديلات الباحث ",
        //"State	OperationCommandPendingOperationCommandForward1AndHalf	",
        //"State	OperationCommandPendingResearcherComments2	",
        //"State	OperationCommandPendingOperationCommandExecution2	",
        //"State	OperationCommandPendingOperationCommandForward2	",
        //"State	OperationCommandPendingOperationCommandForward2AndHalf	",
        //"State	OperationCommandPendingOperationCommandExecution3	",
        //"State	OperationCommandPendingOperationCommandForward3	",
        //"State	OperationCommandPendingOperationCommandResearcherRejected	",
        //"State	OperationCommandPendingISBN	",
        //"StateOperationCommandAfterISBN	بدء عملية التسعير",
        //"StateOperationCommandPendingOperationCommandFinalApprovalViceDean	الموافقة النهائية من قبل وكيل عمادة البحث العلمي ",
        //"StatePricingPendingPricingPrepartion	بدء عملية التسعير",
        //"StatePricingPendingPricingApprovalByUniversityPress	في انتظار موافقة مدير مطابع الجامعة ",
        //"StatePricingPendingPricingApprovalByLibraryAffairs	في انتظار موافقة عميد شؤون المكتبات ",
        //"StatePricingPendingPricingApprovalByDean	في انتظار موافقة العميد ",
        //"StateDistributionPendingDistributionListPreparation	إعداد قوائم التوزيع",
        //"StateDistributionPendingDistributionListApproval	اعتماد قوائم التوزيع",
        //"StateDistributionPendingDistributionListApprovalViceDean	في انتظار موافقة وكيل عمادة النشر العلمي ",
        //"StateDistributionPendingAuthorCopiesDistributionByPR	معالجة طلب تسليم نسخ للمؤلف من قبل مدير العلاقات العامة",
        //"State	DistributionPendingKFNLCopiesDistributionByPrintingUnit	",
        //"StateDistributionPendingCopiesDistributionByLibraryAffairs	معالجة طلب التوزيع على الجهات",
        //"StateDistributionPendingDistributionClosure	في انتظار إغلاق طلب التوزيع",
        //"StateDistributionPendingResearcherConfirmation	في انتظار تأكيد استلام النسخ من قِبل الباحث",
        //"StateDistributionPendingCopyDistribution	في انتظار توزيع النسخ",
        //"StateRewardPendingApprovalByViceDean	في انتظار اعتماد المكافأة من قبل وكيل عمادة النشر العلمي ",
        //"StateRewardPendingApprovalByDean	في انتظار اعتماد المكافأة من قبل العميد",
        //"StateRewardPendingAccountant	في انتظار صرف الموافقة من قبل المحاسب",
        //"StateRewardPendingFinancialDepartmentDirector	في انتظار صرف الموافقة من قبل مدير الإدارة المالية",
        //"StatePendingAcademicCouncilApproval	في انتظار موافقة المجلس العلمي",
        //"StatePendingAdvertisement	في انتظار الإعلان",
        //"StatePending	قيد الانتظار",
        //"StateRejected	تم رفض الطلب",
        //"StateConditionalApproval	موافقة مشروطة",
        //"StateApproved	تم الاعتماد ",
        //"StateRejectedByResearcher	تم رفض الطلب بواسطة الباحث",
        //"StateCanceled	تم إلغاء الطلب",
        //"StateCompleted	تم اكتمال الطلب",

        //            };

        //            var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8);

        //            foreach (var k in dict)
        //            {
        //                writer.WriteLine(Utils.PascalCaseToUnderscore(k));
        //            }

        //            writer.Close();
        //        }
    }
}