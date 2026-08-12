# DateTimeTable
<img width="780" height="632" alt="image" src="https://github.com/user-attachments/assets/9d0009c8-344d-42cd-af46-00210a144341" />

<img width="1901" height="390" alt="image" src="https://github.com/user-attachments/assets/e6d92c58-67af-4d52-a925-804002fdf9ea" />

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


<div dir="rtl">

این مخزن کدی برای تولید «جدول تاریخ» (Date table) فراهم می‌کند که در هر ردیف مقادیر مربوط به چند تقویم را کنار هم می‌گذارد: میلادی (Gregorian)، شمسی/جلالی (Persian/Jalali) و قمری (Hijri). این خروجی مناسب درج در پایگاه داده، گزارش‌سازی یا محاسبات تقویمی است.

ویژگی‌های اصلی:
- تولید ردیف‌های تاریخ با مقادیر هر سه تقویم، نام ماه و نام روز، شماره هفته و رشته‌های قالب‌بندی‌شده.
- ابزارهای کمکی برای نام‌های ماه/روز در تقویم شمسی و قمری و بررسی‌های ساده‌ی تعطیلات.
- امکان انتخاب یا تنظیم تقویم قمری برای دقت بهتر (اطلاعات بیشتر در بخش پایین).

نحوهٔ شروع سریع:
1. پیش‌نیاز: .NET 10 SDK
2. کد را کلون کنید و بسته‌ها را بازیابی کنید:

   dotnet restore

3. پروژه را بسازید:

   dotnet build

4. منطق اصلی در فایل Logic/DateCalculator.cs قرار دارد. متد CalculateDates یک DataTable تولید می‌کند که می‌توانید به‌طور مستقیم در دیتابیس یا در واسط‌های گزارش‌دهی استفاده کنید.

دربارهٔ تقویم قمری (هلالی/قمری):
- پیاده‌سازی پیش‌فرض System.Globalization.HijriCalendar در دات‌نت از روش جدولی (tabular) استفاده می‌کند که صرفاً الگوریتمی است و ممکن است با تاریخ‌های مشاهده‌شده‌ی محلی (با رویت هلال) تفاوت داشته باشد.
- برای تاریخ‌های مدنی دقیق‌تر معمولاً از System.Globalization.UmAlQuraCalendar استفاده می‌شود که یک جدول پیش‌محاسبه‌شده (برای تقویم رسمی عربستان سعودی) است. این روش برای بسیاری از کاربردهای دولتی/اداری مناسب‌تر است اما لزوماً مطابق تقویم‌های محلی ایران نیست.
- اگر نیاز دارید تاریخ‌های قمری مطابق رویت محلی یا جداول رسمی ایران تولید شود، باید از یک منبع معتبر منطقه‌ای (API یا جدول رسمی) استفاده کنید یا الگوریتم‌های نجومی/رؤیت هلال را پیاده‌سازی کنید. تنظیم HijriAdjustment در HijriCalendar می‌تواند اختلاف را یک یا چند روز جبران کند ولی راه‌حل دائمی و دقیقی نیست.
</div>
