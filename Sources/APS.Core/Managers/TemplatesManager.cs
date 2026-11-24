using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace APS.Core.Managers
{
    public class TemplatesManager : AbstractManager
    {
        private Dictionary<TemplateKey, Template> _templatesDict;

        private static List<Template> _defaults = new List<Template>()
        {
              new Template
                {
                    TemplateKey = (short)TemplateKey.NewUserSmsActivation,
                    SubjectEnglish = "",
                    BodyEnglish = "Your activation code in APS is {code}",
                    SubjectArabic = "",
                    BodyArabic = "رمز التحقق الخاص بك في نظام النشر العلمي هو {code}",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.NewUserEmailActivation,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "تفعيل مستخدم جديد",
                    BodyArabic = "<p>السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "تم إنشاء حساب مستخدم جديد باسم {username}. يرجى الضغط <a href=\"{link}\">هنا</a> لتفعيل الحساب"+
                    "</p>",
                },
                   new Template
                {
                    TemplateKey = (short)TemplateKey.ResetPassword,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "إعادة تفعيل كلمة المرور",
                    BodyArabic = "<p>السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "بناء على طلبكم؛ يرجى الضغط <a href=\"{link}\">هنا</a> لإعادة تعيين كلمة المرور للحساب {username}."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.ThesisAgreeement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر بأنني أمتلك جميع حقوق الملكية الفكرية للرسالة، وأتحمل بمفردي نتيجة ما قد يظهر خلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر الرسالة وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر، وأتعهد بمتابعة إجراءات النشر مع العمادة، كما أقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر الرسالة إذا تأخرت عن متابعة إعدادها للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلك . حرر بتاريخ: &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق .&nbsp;<b class=\"rtl\">{date_greg}</b>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.BookAgreement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر  بأنني امتلك جميع حقوق الملكية الفكرية للكتاب دون مشاركة من أحد، أو بمشاركة:&nbsp;<b>{participants}</b>وقد حصلت منه/منهم على الموافقة المطلوبة وفق الصيغة المرافقة، وأتحمل بمفردي نتيجة ما قد يظهر خلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر الكتاب وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر. وأتعهد بمتابعة إجراءات النشر مع العمادة، وأقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر الكتاب إذا تأخرت عن متابعة إعداده للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلك. حرر بتاريخ:  &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق &nbsp;<b class=\"rtl\">{date_greg}</b>"
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.ProbeAgreement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر  بأنني أمتلك جميع حقوق الملكية الفكرية للتحقيق دون مشاركة من أحد، أو  بمشاركة:............&nbsp;<b>{participants}</b>وقد حصلت منه/ منهم على الموافقة المطلوبة وفق الصيغة المرافقة، وأتحمل بمفردي نتيجة ما قد يظهر بخلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر التحقيق وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية ، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر. وأتعهد بمتابعة إجراءات النشر مع العمادة، وأقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر التحقيق إذا تأخرت عن متابعة إعداده  للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلك . حرر بتاريخ:  &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق  &nbsp;<b class=\"rtl\">{date_greg}</b>"
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.HandbookAgreement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر  بأنني أمتلك جميع حقوق الملكية الفكرية للكتيب دون مشاركة من أحد، أو بمشاركة: &nbsp;<b>{participants}</b> ، وقد حصلت منه/ منهم على الموافقة المطلوبة وفق الصيغة المرافقة، وأتحمل بمفردي نتيجة ما قد يظهر خلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر الكتيب وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية ، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر. وأتعهد بمتابعة إجراءات النشر مع العمادة، وأقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر الكتيب إذا تأخرت عن متابعة إعداده للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلك. حرر بتاريخ: &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق &nbsp;<b class=\"rtl\">{date_greg}</b>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.DicationaryAgreeement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر  بأنني أمتلك جميع حقوق الملكية الفكرية للمعجم دون مشاركة من أحد، أو  بمشاركة:&nbsp;<b>{participants}</b> وقد حصلت منه/منهم على الموافقة المطلوبة وفق الصيغة المرافقة، وأتحمل بمفردي نتيجة ما قد يظهر خلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر المعجم وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية ، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر. وأتعهد بمتابعة إجراءات النشر مع العمادة، وأقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر المعجم إذا تأخرت عن متابعة إعداده للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلكحرر بتاريخ:  &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق &nbsp;<b class=\"rtl\">{date_greg}</b>."
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.EncyclopediaAgreement,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="",
                    BodyArabic = "أقر  بأنني أمتلك جميع حقوق الملكية الفكرية للموسوعة دون مشاركة من أحد، أو  بمشاركة:............&nbsp;<b>{participants}</b>وقد حصلت منه/ منهم على الموافقة المطلوبة وفق الصيغة المرافقة، وأتحمل بمفردي نتيجة ما قد يظهر خلاف ذلك، وقد أذنت لجامعة الإمام محمد بن سعود الإسلامية بنشر الموسوعة وفق ضوابط النشر لديها لمدة خمس سنوات من تاريخ التسجيل في مكتبة الملك فهد الوطنية ، ولا يحق لي المطالبة بأكثر من المكافأة التي يقرها المجلس العلمي مقابل النشر. وأتعهد بمتابعة إجراءات النشر مع العمادة، وأقر بأنني علمت أنه يحق لعمادة البحث العلمي صرف النظر عن نشر الموسوعة إذا تأخرت عن متابعة إعدادها للنشر مدة ستة أشهر من تسلمي الخطاب الخاص بذلك . حرر بتاريخ:  &nbsp;<b class=\"rtl\">{date_hijri}</b>، الموافق &nbsp;<b class=\"rtl\">{date_greg}</b>"
                },

                 new Template
                {
                    TemplateKey = (short)TemplateKey.RegisterReferee,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="دعوة لتحكيم مادة علمية",
                     BodyArabic = "<p>الأستاذ الفاضل <b>{name}</b>، السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "يسرنا إبلاغكم بأنه قد تم دعوتكم لتحكيم مادة علمية لجامعة الإمام محمد بن سعود الإسلامية، وهي بعنوان <b>\"{title}\"</b> ونوعها <b>{type}</b>. للتسحيل يرجى الضغط <a href=\"{link}\">هنا</a>."+
                    "</p>",
                },

                   new Template
                {
                    TemplateKey = (short)TemplateKey.RegisterRefereeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="دعوة لتحكيم مادة علمية",
                     BodyArabic = "<p>الأستاذ الفاضل <b>{name}</b>، السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "يسرنا إبلاغكم بأنه قد تم دعوتكم لتحكيم مادة علمية لجامعة الإمام محمد بن سعود الإسلامية، وهي بعنوان <b>\"{title}\"</b> ونوعها <b>{type}</b>. للتسحيل يرجى الضغط {link}."+
                    "</p>",
                },

                 new Template
                {
                    TemplateKey = (short)TemplateKey.InviteReferee,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="دعوة لتحكيم مادة علمية",
                     BodyArabic = "<p>الأستاذ الفاضل <b>{name}</b>، السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "يسرنا إبلاغكم بأنه قد تم دعوتكم لتحكيم مادة علمية لجامعة الإمام محمد بن سعود الإسلامية، وهي بعنوان <b>\"{title}\"</b> ونوعها <b>{type}</b>. لقبول الدعوة يرجى الدخول للموقع من <a href=\"{link}\">هنا</a>."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.InviteRefereeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic ="دعوة لتحكيم مادة علمية",
                     BodyArabic = "الأستاذ الفاضل <b>{name}</b>، السلام عليكم ورحمة الله وبركاته وبعد:    "+
                    "يسرنا إبلاغكم بأنه قد تم دعوتكم لتحكيم مادة علمية لجامعة الإمام محمد بن سعود الإسلامية، وهي بعنوان <b>\"{title}\"</b> ونوعها <b>{type}</b>. لقبول الدعوة يرجى الدخول للموقع من {link}."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NonOfficialEmailRequiresApproval,
                    SubjectEnglish = "Attension: Researcher wants to register with Non Official Email Address",
                    BodyEnglish = "Dear System Admin,"+
                                    "<br />This is to inform you that a researcher {name} with email Id {emailId} wants to register to the System.<br />Please validate him/her before Activating.<br /> Thank you.."+
                    "</p>",
                    SubjectArabic =": الباحث يريد التسجيل مع عنوان البريد الإلكتروني غير الرسمي",
                     BodyArabic = "<br /> هذا لإعلامك بأن الباحث {name} الذي لديه معرف البريد الإلكتروني {emailId} يريد التسجيل في النظام. <br /> يرجى التحقق منه قبل تفعيله. <br /             > شكرا لك .."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.ContactUsMessage,
                    SubjectEnglish = "Attension: {fullname} wants to contact us for something.",
                    BodyEnglish = "Dear System Admin,"+
                                    "<br />This is to inform you that a contact with name {fullname} and email Id {emailId} wants to contact us.<br/>Please look into his/her message and take action.<br />His/Her message is as below.<br /><br />Subject: {subject}<br />{message}<br />Thank You."+
                    "</p>",
                    SubjectArabic ="أتنسيون: {fullname} يريد الاتصال بنا للحصول على شيء ما.",
                     BodyArabic = "عزيزي مسؤول النظام، "+
                                    "<br /> هذا لإعلامك بأن جهة اتصال تحمل الاسم {fullname} ورقم البريد الإلكتروني {emailId} يريد الاتصال بنا. <br/> يرجى الاطلاع على رسالته واتخاذ إجراء. <br /> هيس / هر الرسالة على النحو التالي. <br /> <br /> الموضوع: {subject} <br /> {message} <br /> شكرا."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NotificationOnFinalApproval,
                    SubjectEnglish = "Attension: Final Approval is done for {Id}",
                    BodyEnglish = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got final Approval."+
                    "</p>",
                    SubjectArabic ="Attension: Final Approval is done for {Id}",
                     BodyArabic = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got final Approval."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NotificationOnFinalRejection,
                    SubjectEnglish = "Attension: Final Rejection is done for {Id}",
                    BodyEnglish = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got rejection at final Action."+
                    "</p>",
                    SubjectArabic ="أتنسيون: {fullname} يريد الاتصال بنا للحصول على شيء ما.",
                     BodyArabic = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got rejection at final Action."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NotificationOnGradeBasedRefereeApproval,
                    SubjectEnglish = "Attension: Request # {Id} is approved as Referees provided required Grade.",
                    BodyEnglish = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got approve as Referees provided the average Grade more than 80%."+
                    "</p>",
                    SubjectArabic ="Attension: Request # {Id} is approved as Referees provided required Grade.",
                     BodyArabic = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got approve as Referees provided the average Grade more than 80%."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NotificationOnGradeBasedRefereeRejection,
                    SubjectEnglish = "Attension: Request # {Id} is rejected as Referees did not provided required Grade.",
                    BodyEnglish = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got rejection as Referees did not provide the average Grade more than 80%."+
                    "</p>",
                    SubjectArabic ="Attension: Request # {Id} is rejected as Referees did not provided required Grade.",
                    BodyArabic = "Dear User,"+
                                    "<br />This is to inform you that The Request # {Id} is got rejection as Referees did not provide the average Grade more than 80%."+
                    "</p>",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.RefereesAgreement,
                    SubjectEnglish = "",
                    BodyEnglish = "This is the agreement for referees.",
                    SubjectArabic ="",
                    BodyArabic = "هذا هو اتفاق للحكام."
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.SmsActivationNoUser,
                    SubjectEnglish = "",
                    BodyEnglish = "Your activation code in APS is {code}",
                    SubjectArabic = "",
                    BodyArabic = "رمز التحقق الخاص بك في نظام النشر العلمي هو {code}",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.EmailActivationNoUser,
                    SubjectEnglish = "Your activation code in APS",
                    BodyEnglish = "Your activation code in APS is {code}",
                    SubjectArabic = "رمز التفعيل",
                    BodyArabic = "رمز التحقق الخاص بك في نظام النشر العلمي هو {code}",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NewCommitteForPAEMemberWelcomeEmail,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "<div style=\"direction: rtl\"><p>{FullName} </p> <p>السلام عليكم ورحمة الله وبركاته،، </p> <p>ترحب بكم 'عمادة البحث العلمي' في برنامجها 'النشر العلمي' وتتمنى لكم مزيداً من النجاح والتميز.. </p><p>نفيدكم بأنه  تم إضافتكم إلى مجلس مركز/وحدة البحوث في كلية /معهد الكلية او المعهد.</p><p>والبيانات الخاصة بكم للدخول على النظام هي:</p><p>اسم المستخدم : {Email}</p><p>كلمة المرور: {Password}</p><p>للتسجيل الدخول في نظام النشر العلمي <a href='{Link}'> إضغط هنا</a></p><p>والله ولي التوفيق. </p></div>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.NewCommitteForPAEMemberWelcomeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "سعادة الأستاذ الدكتور {FullName} عضو المجلس لقد تم إنشاء حساب لكم على نظام النشر العلمي الإلكتروني. نرجو الاطلاع على بريدكم الإلكتروني الجامعي الرسمي.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.DeanOfAcademicResearchMemberWelcomeEmail,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "<div style=\"direction: rtl\"><p>{FullName} </p> <p>السلام عليكم ورحمة الله وبركاته،، </p> <p>ترحب بكم 'عمادة البحث العلمي' في برنامجها 'النشر العلمي' وتتمنى لكم مزيداً من النجاح والتميز.. </p><p>نفيدكم بأنه  تم إضافتكم إلى مجلس مركز/وحدة البحوث في كلية /معهد الكلية او المعهد.</p><p>والبيانات الخاصة بكم للدخول على النظام هي:</p><p>اسم المستخدم : {Email}</p><p>كلمة المرور: {Password}</p><p>للتسجيل الدخول في نظام النشر العلمي  <a href='{Link}'> إضغط هنا</a></p><p>والله ولي التوفيق. </p></div>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.DeanOfAcademicResearchMemberWelcomeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "سعادة الأستاذ الدكتور {FullName} عضو المجلس لقد تم إنشاء حساب لكم على نظام النشر العلمي الإلكتروني. نرجو الاطلاع على بريدكم الإلكتروني الجامعي الرسمي.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.NewAcademicCouncilMemberWelcomeEmail,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "<div style=\"direction: rtl\"><p>{FullName} </p> <p>السلام عليكم ورحمة الله وبركاته،، </p> <p>ترحب بكم 'عمادة البحث العلمي' في برنامجها 'النشر العلمي' وتتمنى لكم مزيداً من النجاح والتميز.. </p><p>نفيدكم بأنه  تم إضافتكم إلى مجلس مركز/وحدة البحوث في كلية /معهد الكلية او المعهد.</p><p>والبيانات الخاصة بكم للدخول على النظام هي:</p><p>اسم المستخدم : {Email}</p><p>كلمة المرور: {Password}</p><p>للتسجيل الدخول في نظام النشر العلمي <a href='{Link}'> إضغط هنا</a></p><p>والله ولي التوفيق. </p></div>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.NewAcademicCouncilMemberWelcomeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "سعادة الأستاذ الدكتور {FullName} عضو المجلس لقد تم إنشاء حساب لكم على نظام النشر العلمي. نرجو الاطلاع على بريدكم الإلكتروني الجامعي الرسمي.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.ResearchUnitMemberWelcomeEmail,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "<div style=\"direction: rtl\"><p>{FullName} </p> <p>السلام عليكم ورحمة الله وبركاته،، </p> <p>ترحب بكم 'عمادة البحث العلمي' في برنامجها 'النشر العلمي' وتتمنى لكم مزيداً من النجاح والتميز.. </p><p>نفيدكم بأنه  تم إضافتكم إلى مجلس مركز/وحدة البحوث في كلية /معهد الكلية او المعهد.</p><p>والبيانات الخاصة بكم للدخول على النظام هي:</p><p>اسم المستخدم : {Email}</p><p>كلمة المرور: {Password}</p><p>للتسجيل الدخول في نظام النشر العلمي <a href='{Link}'> إضغط هنا</a></p><p>والله ولي التوفيق. </p></div>",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.ResearchUnitMemberWelcomeSMS,
                    SubjectEnglish = "",
                    BodyEnglish = "",
                    SubjectArabic = "بيانات عضو المجلس بنظام النشر العلمي",
                    BodyArabic = "سعادة الأستاذ الدكتور {FullName} عضو المجلس لقد تم إنشاء حساب لكم على نظام النشر العلمي. نرجو الاطلاع على بريدكم الإلكتروني الجامعي الرسمي.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.ExternalRefereeSMSNotification,
                    SubjectEnglish = "",
                    BodyEnglish = "سعادة الدكتور {FullName} نفيدكم بأنه تم ترشيحكم للانضمام لبرنامج النشر العلمي بجامعة الإمام محمد بن سعود الإسلامية؛ لتحكيم طلب نشر  بحثي، لطفاً اضغط على هذا الرابط للدخول على صفحة التحكيم. عمادة البحث العلمي.",
                    SubjectArabic = "",
                    BodyArabic = "سعادة الدكتور {FullName} نفيدكم بأنه تم ترشيحكم للانضمام لبرنامج النشر العلمي بجامعة الإمام محمد بن سعود الإسلامية؛ لتحكيم طلب نشر  بحثي لطفاً اضغط على هذا الرابط للدخول على صفحة التحكيم. عمادة البحث العلمي.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.InternalRefereeSMSNotification,
                    SubjectEnglish = "",
                    BodyEnglish = "سعادة عضو/عضوة هيئة التدريس نفيدكم بأنه تم ترشيحكم لتحكيم طلب نشر علمي وبانتظار موافقتكم الكريمة على التحكيم. ويسعدنا في عمادة البحث العلمي التعاون معكم.",
                    SubjectArabic = "",
                    BodyArabic = "سعادة عضو/عضوة هيئة التدريس نفيدكم بأنه تم ترشيحكم لتحكيم طلب نشر علمي وبانتظار موافقتكم الكريمة على التحكيم. ويسعدنا في عمادة البحث العلمي التعاون معكم.",
                },

                new Template
                {
                    TemplateKey = (short)TemplateKey.ExternalRefereeEmailNotification,
                    SubjectEnglish = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'",
                    BodyEnglish = "سعادة الدكتورسعادة الدكتور {FullName} وفقه الله<br/>السلام عليكم ورحمة الله وبركاته،،<br/>ترحب بكم عمادة البحث العلمي في برنامجها النشر العلمي وتتمنى لكم مزيداً من النجاح والتميز.<br/>وتدعوكم لتحكيم طلب 	نشر مشروعي بحثي،  لطفاً.. تفضل بالدخول على <a href='{REQUEST_DETAIL_PUBLIC_LINK}'>(هذا الرابط) </a> للاطلاع على المادة المطلوب تحكيمها.<br/><br/>والله ولي التوفيق",
                    SubjectArabic = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'",
                    BodyArabic = "سعادة الدكتورسعادة الدكتور {FullName} وفقه الله<br/>السلام عليكم ورحمة الله وبركاته،،<br/>ترحب بكم عمادة البحث العلمي في برنامجهاالنشر العلمي  وتتمنى لكم مزيداً من النجاح والتميز.<br/>وتدعوكم لتحكيم طلب 	نشر مشروعي بحثي،  لطفاً.. تفضل بالدخول على <a href='{REQUEST_DETAIL_PUBLIC_LINK}'>(هذا الرابط) </a> للاطلاع على المادة المطلوب تحكيمها.<br/><br/>والله ولي التوفيق",
                },
                new Template
                {
                    TemplateKey = (short)TemplateKey.InternalRefereeEmailNotification,
                    SubjectEnglish = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'",
                    BodyEnglish = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'السلام عليكم ورحمة الله وبركاته،،<br/>ترحب بكم عمادة البحث العلمي في برنامجها النشر العلمي وتتمنى لكم مزيداً من النجاح والتميز..<br/>وتدعوكم لتحكيم طلب نشر علمي،  لطفاً.. تفضل بالدخول على صفحتكم على <a href='{REQUEST_DETAIL_INTERNAL_LINK}'>نظام النشر العلمي</a> ومن ثم اختيار صفة ( محكم )؛ للاطلاع على المادة المطلوب تحكيمها.<br/>ملاحظة: إن كنت غير مسجلاً بـ  نظام النشر العلمي، لطفاً.. يرجى تسجيل مستخدم جديد.<br/><br/>والله ولي التوفيق.",
                    SubjectArabic = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'",
                    BodyArabic = "سعادة الدكتور عضو هيئة التدريس بجامعة 'الإمام محمد بن سعود الإسلامية'السلام عليكم ورحمة الله وبركاته،،<br/>ترحب بكم عمادة البحث العلمي في برنامجها النشر العلمي وتتمنى لكم مزيداً من النجاح والتميز..<br/>وتدعوكم لتحكيم طلب نشر علمي،  لطفاً.. تفضل بالدخول على صفحتكم على <a href='{REQUEST_DETAIL_INTERNAL_LINK}'>نظام النشر العلمي</a> ومن ثم اختيار صفة ( محكم )؛ للاطلاع على المادة المطلوب تحكيمها.<br/>ملاحظة: إن كنت غير مسجلاً بـ  نظام النشر العلمي، لطفاً.. يرجى تسجيل مستخدم جديد.<br/><br/>والله ولي التوفيق.",
                },
        };

        private Dictionary<TemplateKey, Template> TemplatesDict
        {
            get
            {
                if (_templatesDict != null) return _templatesDict;

                if (HasOwinContext)
                {
                    _templatesDict = HttpContextManager.Current.GetOwinContext().Get<Dictionary<TemplateKey, Template>>(typeof(Dictionary<TemplateKey, Template>).FullName);
                }

                if (_templatesDict == null)
                {
                    var max = (byte)Enum.GetValues(typeof(TemplateKey)).Cast<TemplateKey>().Last();

                    _templatesDict = new Dictionary<TemplateKey, Template>();

                    var templates = DbContext.Templates.ToList();

                    foreach (var tmpl in templates.Union(_defaults))
                    {
                        if (tmpl.TemplateKey > max || _templatesDict.ContainsKey((TemplateKey)tmpl.TemplateKey)) continue;

                        tmpl.Created = DateTime.Now;
                        tmpl.CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id;
                        tmpl.Updated = DateTime.Now;
                        tmpl.UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id;

                        _templatesDict.Add((TemplateKey)tmpl.TemplateKey, tmpl);

                        if (DbContext.Entry(tmpl).State == EntityState.Detached)
                        {
                            DbContext.Templates.Add(tmpl);
                        }
                    }

                    if (HasOwinContext)
                    {
                        HttpContextManager.Current.GetOwinContext().Set<Dictionary<TemplateKey, Template>>(typeof(Dictionary<TemplateKey, Template>).FullName, _templatesDict);
                    }

                    DbContext.SaveChanges();
                }

                return _templatesDict;
            }
        }

        public List<Template> Templates
        {
            get
            {
                return TemplatesDict.Select(d => d.Value).OrderBy(k => k.TemplateKey).ToList();
            }
        }

        public Template this[TemplateKey key]
        {
            get
            {
                if (TemplatesDict.ContainsKey(key))
                {
                    return TemplatesDict[key];
                }
                else
                {
                    var template = new Template
                    {
                        TemplateKey = (short)key,
                        SubjectEnglish = "New notification",
                        SubjectArabic = "إشعار جديد",
                        BodyEnglish = "<ul><li>Type: " + key.ToString() + "</li><li>Name: {Name}</li><li>Message: {Message}</li><li>Link: <a href=\"{Link}\">{Link}</a></li></ul>",
                        BodyArabic = "<ul><li>النوع: " + key.ToString() + "</li><li>الاسم: {Name}</li><li>الرسالة: {Message}</li><li>الرابط: <a href=\"{Link}\">{Link}</a></li></ul>"
                    };

                    TemplatesDict.Add(key, template);

                    return template;
                }
            }
            set
            {
                if (value == null || value.TemplateKey != (byte)key) return;

                if (TemplatesDict.ContainsKey(key))
                {
                    TemplatesDict[key] = value;
                }
                else
                {
                    TemplatesDict.Add(key, value);
                    DbContext.Templates.Add(value);
                }

                DbContext.SaveChanges();
            }
        }

        public void SendEmailAsync(Request request, TemplateKey key)
        {
            if (request == null) return;
            try
            {
                Hangfire.BackgroundJob.Enqueue(() => SendEmail(request.Id, key));
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, request);
            }
        }

        public static void SendEmail(long requestId, TemplateKey key)
        {
            if (requestId <= 0 || key == TemplateKey.Unspecified) return;

            var db = new ApsDbEntities();

            var request = db.Requests.FirstOrDefault(r => r.Id == requestId);
            if (request == null) return;
        }

        public static string UpdateRequestTemplate(Request request, User user, TemplateKey key, out string subject)
        {
            subject = null;
            if (request == null || key == TemplateKey.Unspecified || request.Id <= 0 || request.Type == 0) return "Empty or invalid request";
            if (user == null || user.Id == null || user.Email == null || !user.Email.Contains("@") || user.Email.EndsWith("@")) return "Empty or invalid user";

            var lang = user.UserPreferences.OrderByDescending(u => u.Id).FirstOrDefault(u => u.Key == "LANG" && u.Value == "AR") != null ? LanguagePreference.Arabic : LanguagePreference.English;
            var template = TemplateUtils[key];
            var type = (RequestType)request.Type;

            if (template == null) return "Invalid template";

            subject = (lang == LanguagePreference.Arabic ? template.SubjectArabic : template.SubjectEnglish).UpdateUserTemplateVariables(user).UpdateRequestTemplateVariables(request, lang);
            var body = (lang == LanguagePreference.Arabic ? template.BodyArabic : template.BodyEnglish).UpdateUserTemplateVariables(user).UpdateRequestTemplateVariables(request, lang);

            return body;
        }
    }
}