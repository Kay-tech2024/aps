# Public release notes

This repository is a history-free snapshot of the APS source code (branch `plan_b`, commit `6d0e7b0`), published on 2026-11-24.

Removed before publishing:
- `Database/Backups/*.bak`, `Database/db_init.sql` and the `Database/OldScripts` files with user data. They held real user records (names, e-mail addresses, phone numbers, password hashes). The schema and migration scripts (`delta_*.sql`, views, cleanup scripts) are kept.
- `Documents/referees.xlsx`, which held people's contact data.
- `Software/`: the SAP Crystal Reports installers. Get them from SAP.
- `.vs/`, a Visual Studio cache.

Values replaced with `__SET_ME__` (set your own before building or running):
- `Sources/APS.Core/Constants.cs`: `SendGridAPIKey`, `EncryptionSecurityKey`. A different `EncryptionSecurityKey` cannot read data encrypted with the original one.
- `Web.config` / `App.config` connection-string passwords, and `smtp:Password` in `Sources/APS.Web/Views/Web.config`.
- SMTP and SMS provider passwords in `Sources/APS.Services/SmsService*.cs`, `Sources/APS.Core/EmailService.cs`, `Sources/APS.Core/Utils.cs` and `Sources/APS.Web/Controllers/BaseController.cs`.
