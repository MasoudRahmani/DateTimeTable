# DateTimeTable

DateTimeTable is a small .NET library and utility for generating calendar date tables covering multiple calendar systems. It is intended to produce rows of calendar data (for example for a database calendar table) that include Gregorian (Miladi), Persian (Shamsi/Jalali) and Hijri (Islamic) values and related metadata.

Supported calendars
- Gregorian (Miladi)
- Persian / Jalali (Shamsi)
- Hijri (Islamic — tabular and civil)

Key features
- Generate date rows with multiple calendar systems side-by-side
- Week numbers, month/day names, padded formatted values
- Helpers for Persian and Hijri names and common holiday checks

Getting started
1. Prerequisites: .NET 10 SDK
2. Clone the repository and restore packages:

   dotnet restore

3. Build the solution:

   dotnet build

4. The core logic is in Logic/DateCalculator.cs. Use DateCalculator.CalculateDates to generate a DataTable of calendar rows for a date range.

Notes about Hijri accuracy
- System.Globalization.HijriCalendar implements a tabular arithmetic algorithm that can differ from civil/observed dates. For more accurate civil Hijri dates prefer System.Globalization.UmAlQuraCalendar (precomputed Saudi civil table) or an authoritative region-specific calendar table or API if you need Iran-specific religious/observed dates. Adjustments (HijriAdjustment) are a short-term workaround but not a robust solution for local moon-sighting rules.

Contributing
- Bug reports and pull requests are welcome. Please open issues describing the problem and include sample dates that reproduce the issue.

License
- This project does not include a license file. Add a LICENSE to clarify terms if you intend to publish or share the code.

English / فارسی (خلاصه)

- This repository provides tools to build a multi-calendar date table (Gregorian, Persian, Hijri). For Persian (Jalali) it uses the .NET PersianCalendar; for Hijri consider UmAlQuraCalendar or external authoritative sources when accuracy is required for a region.

این مخزن ابزاری برای تولید جدول تاریخ‌ها با چند تقویم (میلادی، شمسی، قمری) فراهم می‌کند. تقویم شمسی از PersianCalendar دات‌نت استفاده می‌کند؛ برای دقت بهتر در تقویم قمری در شرایط محلی از UmAlQuraCalendar یا منابع معتبر منطقه‌ای استفاده کنید.
