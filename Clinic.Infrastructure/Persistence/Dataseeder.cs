using Clinic.Domain.Constants;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Clinic.Domain.Rules;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic.Infrastructure.Persistence.Seed;


public static class DataSeeder
{
    // Demo passwords (development only). Change these outside local/demo use.
    public const string DemoStaffPassword = "Clinic#2026pass";

    // ---------------------------------------------------------------- doctors
    private static readonly (string FullName, string Specialty, string QueueLetter, string Room, int AvgMinutes, bool IsActive)[] Doctors =
    [
        ("Dr. Hanna Bekele",   "General Practice", "G", "101", 10, true),
        ("Dr. Samuel Tadesse", "Pediatrics",       "P", "102", 15, true),
        ("Dr. Meron Alemu",    "Gynecology",       "Y", "103", 20, true),
        ("Dr. Dawit Haile",    "Cardiology",       "C", "104", 20, true),
        ("Dr. Selam Girma",    "Dermatology",      "D", "105", 10, true),
        ("Dr. Yonas Kebede",   "Orthopedics",      "O", "106", 15, false), // inactive: tests filtering
    ];

    private static readonly DayOfWeek[] WeekDays =
        new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

    // Weekly schedule per doctor: (days, open, close, slot minutes)
    private static readonly Dictionary<string, (DayOfWeek[] Days, TimeOnly Open, TimeOnly Close, int Slot)[]> Schedules = new()
    {
        ["G"] =
        [
            (WeekDays, new TimeOnly(8, 0), new TimeOnly(16, 0), 10),
            (new[] { DayOfWeek.Saturday }, new TimeOnly(8, 0), new TimeOnly(12, 0), 10),
        ],
        ["P"] = [(WeekDays, new TimeOnly(9, 0), new TimeOnly(15, 0), 15)],
        ["Y"] = [(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, new TimeOnly(8, 30), new TimeOnly(14, 30), 20)],
        ["C"] = [(new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, new TimeOnly(10, 0), new TimeOnly(16, 0), 20)],
        ["D"] = [(WeekDays, new TimeOnly(13, 0), new TimeOnly(17, 0), 10)],
        ["O"] = [(new[] { DayOfWeek.Monday, DayOfWeek.Thursday }, new TimeOnly(9, 0), new TimeOnly(13, 0), 15)],
    };

    // --------------------------------------------------------------- patients
    // Phone numbers are stored in normalized +251 form (see PhoneNumberNormalizer).
    private static readonly (string FullName, string Phone, DateOnly Dob, Sex Sex, string? Address)[] Patients =
    [
        ("Abebe Kebede",        "+251911000001", new DateOnly(1985, 3, 14),  Sex.Male,   "Bole, Addis Ababa"),
        ("Tigist Alemu",        "+251911000002", new DateOnly(1992, 7, 2),   Sex.Female, "Kirkos, Addis Ababa"),
        ("Dawit Mekonnen",      "+251911000003", new DateOnly(1978, 11, 23), Sex.Male,   "Yeka, Addis Ababa"),
        ("Hiwot Girma",         "+251911000004", new DateOnly(2001, 1, 30),  Sex.Female, "Arada, Addis Ababa"),
        ("Yohannes Tesfaye",    "+251911000005", new DateOnly(1969, 5, 9),   Sex.Male,   "Lideta, Addis Ababa"),
        ("Selamawit Bekele",    "+251911000006", new DateOnly(1995, 9, 17),  Sex.Female, "Nifas Silk-Lafto, Addis Ababa"),
        ("Bereket Haile",       "+251911000007", new DateOnly(2015, 4, 5),   Sex.Male,   "Gulele, Addis Ababa"),
        ("Meron Assefa",        "+251911000008", new DateOnly(1989, 12, 12), Sex.Female, "Bole, Addis Ababa"),
        ("Nahom Wolde",         "+251911000009", new DateOnly(2018, 8, 21),  Sex.Male,   "Kolfe Keranio, Addis Ababa"),
        ("Rahel Tadesse",       "+251911000010", new DateOnly(1998, 2, 28),  Sex.Female, "Akaky Kaliti, Addis Ababa"),
        ("Kaleb Fikru",         "+251911000011", new DateOnly(1974, 6, 6),   Sex.Male,   "Yeka, Addis Ababa"),
        ("Eden Solomon",        "+251911000012", new DateOnly(2003, 10, 10), Sex.Female, "Kirkos, Addis Ababa"),
        ("Mikiyas Getachew",    "+251911000013", new DateOnly(1982, 3, 3),   Sex.Male,   "Bole, Addis Ababa"),
        ("Bethlehem Yilma",     "+251911000014", new DateOnly(1990, 7, 19),  Sex.Female, "Arada, Addis Ababa"),
        ("Elias Demeke",        "+251911000015", new DateOnly(2012, 5, 25),  Sex.Male,   "Lideta, Addis Ababa"),
        ("Sara Abera",          "+251911000016", new DateOnly(1987, 9, 9),   Sex.Female, "Gulele, Addis Ababa"),
        ("Yared Negash",        "+251911000017", new DateOnly(1965, 1, 15),  Sex.Male,   "Kolfe Keranio, Addis Ababa"),
        ("Feven Teshome",       "+251911000018", new DateOnly(1999, 4, 4),   Sex.Female, "Bole, Addis Ababa"),
        ("Samuel Desta",        "+251911000019", new DateOnly(1993, 8, 8),   Sex.Male,   "Nifas Silk-Lafto, Addis Ababa"),
        ("Lidya Worku",         "+251911000020", new DateOnly(1980, 12, 1),  Sex.Female, "Yeka, Addis Ababa"),
        ("Henok Ayele",         "+251911000021", new DateOnly(2016, 6, 14),  Sex.Male,   "Kirkos, Addis Ababa"),
        ("Martha Zewdu",        "+251911000022", new DateOnly(1972, 2, 20),  Sex.Female, "Arada, Addis Ababa"),
        ("Yonas Berhanu",       "+251911000023", new DateOnly(1996, 10, 30), Sex.Male,   "Akaky Kaliti, Addis Ababa"),
        ("Ruth Melaku",         "+251911000024", new DateOnly(1991, 5, 18),  Sex.Female, "Bole, Addis Ababa"),
        ("Amanuel Sisay",       "+251911000025", new DateOnly(1958, 11, 11), Sex.Male,   "Lideta, Addis Ababa"),
        ("Betelhem Alemayehu",  "+251911000026", new DateOnly(2004, 3, 27),  Sex.Female, "Gulele, Addis Ababa"),
        ("Getachew Mamo",       "+251911000027", new DateOnly(1976, 7, 7),   Sex.Male,   null),
        ("Frehiwot Shiferaw",   "+251911000028", new DateOnly(1984, 9, 29),  Sex.Female, "Yeka, Addis Ababa"),
        ("Abenezer Tsegaye",    "+251911000029", new DateOnly(2010, 1, 9),   Sex.Male,   "Kolfe Keranio, Addis Ababa"),
        ("Hanna Fantahun",      "+251911000030", new DateOnly(1997, 6, 22),  Sex.Female, "Kirkos, Addis Ababa"),
    ];

    // ------------------------------------------------------------ staff users
    // Role, email, full name, index into Doctors (-1 = not linked to a doctor)
    private static readonly (string Role, string Email, string FullName, int DoctorIndex)[] StaffUsers =
    [
        (RoleNames.Administrator, "admin@clinic.local",        "Clinic Administrator", -1),
        (RoleNames.Receptionist,  "reception1@clinic.local",   "Almaz Tadesse",        -1),
        (RoleNames.Receptionist,  "reception2@clinic.local",   "Birtukan Solomon",     -1),
        (RoleNames.Clinician,     "dr.hanna@clinic.local",     "Dr. Hanna Bekele",      0),
        (RoleNames.Clinician,     "dr.samuel@clinic.local",    "Dr. Samuel Tadesse",    1),
        (RoleNames.Clinician,     "dr.meron@clinic.local",     "Dr. Meron Alemu",       2),
    ];

    // ------------------------------------------------------------------- main
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<string>>>();

        await SeedAsync(context, userManager, roleManager, ct);
    }

    public static async Task SeedAsync(
        ClinicDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<string>> roleManager,
        CancellationToken ct = default)
    {
        await SeedClinicDataAsync(context, ct);
        await SeedRolesAndUsersAsync(context, userManager, roleManager, ct);
        await SeedAppointmentsAsync(context, ct);
    }

    /// <summary>Doctors, schedules, closures and patients (no Identity needed).</summary>
    public static async Task SeedClinicDataAsync(ClinicDbContext context, CancellationToken ct = default)
    {
        if (context.Database.IsRelational() && context.Database.GetMigrations().Any())
            await context.Database.MigrateAsync(ct);
        else
            await context.Database.EnsureCreatedAsync(ct);

        if (!await context.Doctors.AnyAsync(ct))
        {
            foreach (var (fullName, specialty, letter, room, avg, isActive) in Doctors)
            {
                context.Doctors.Add(new Doctor
                {
                    FullName = fullName,
                    Specialty = specialty,
                    QueueLetter = letter,
                    RoomNumber = room,
                    AvgConsultMinutes = avg,
                    IsActive = isActive
                });
            }
            await context.SaveChangesAsync(ct);
        }

        // Give every doctor that has no schedule yet a weekly one. Known queue letters use the
        // templates above; any other doctor (e.g. created by hand) gets a Mon-Fri 08:00-16:00 default.
        var doctorsWithSchedule = await context.Schedules.Select(s => s.DoctorId).Distinct().ToListAsync(ct);
        var needSchedule = await context.Doctors
            .Where(d => !doctorsWithSchedule.Contains(d.Id))
            .ToListAsync(ct);

        foreach (var doctor in needSchedule)
        {
            var blocks = Schedules.TryGetValue(doctor.QueueLetter, out var known)
                ? known
                : new[] { (WeekDays, new TimeOnly(8, 0), new TimeOnly(16, 0), Math.Max(5, doctor.AvgConsultMinutes)) };

            foreach (var (days, open, close, slot) in blocks)
                foreach (var day in days)
                    context.Schedules.Add(new Schedule
                    {
                        DoctorId = doctor.Id,
                        DayOfWeek = day,
                        OpenTime = open,
                        CloseTime = close,
                        SlotMinutes = slot
                    });
        }
        if (needSchedule.Count > 0) await context.SaveChangesAsync(ct);

        if (!await context.ScheduleExceptions.AnyAsync(ct))
        {
            var today = ClinicTime.Today(DateTime.UtcNow);
            var active = await context.Doctors.Where(d => d.IsActive).OrderBy(d => d.Id).Take(2).ToListAsync(ct);

            if (active.Count > 0)
                context.ScheduleExceptions.Add(
                    new ScheduleException { DoctorId = active[0].Id, Date = today.AddDays(10), Reason = "Annual leave" });
            if (active.Count > 1)
                context.ScheduleExceptions.Add(
                    new ScheduleException { DoctorId = active[1].Id, Date = today.AddDays(5), Reason = "Medical conference" });

            await context.SaveChangesAsync(ct);
        }

        if (!await context.Patients.AnyAsync(ct))
        {
            foreach (var (fullName, phone, dob, sex, address) in Patients)
            {
                context.Patients.Add(new Patient
                {
                    FullName = fullName,
                    PhoneNumber = phone,
                    DateOfBirth = dob,
                    Sex = sex,
                    Address = address
                });
            }
            await context.SaveChangesAsync(ct);
        }
    }

    /// <summary>Roles plus demo staff accounts; clinician accounts are linked to their doctor.</summary>
    public static async Task SeedRolesAndUsersAsync(
        ClinicDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<string>> roleManager,
        CancellationToken ct = default)
    {
        foreach (var role in RoleNames.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<string> { Id = Guid.NewGuid().ToString(), Name = role });

        var doctors = await context.Doctors.OrderBy(d => d.Id).ToListAsync(ct);

        foreach (var (role, email, fullName, doctorIndex) in StaffUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            var doctor = doctorIndex >= 0 && doctorIndex < doctors.Count ? doctors[doctorIndex] : null;

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    IsActive = true,
                    DoctorId = doctor?.Id.ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                };

                var created = await userManager.CreateAsync(user, DemoStaffPassword);
                if (!created.Succeeded)
                    throw new InvalidOperationException(
                        $"Seeding user {email} failed: " +
                        string.Join("; ", created.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(user, role))
                await userManager.AddToRoleAsync(user, role);

            // Keep Doctor.UserId in sync with the clinician account.
            if (doctor is not null && doctor.UserId is null)
            {
                doctor.UserId = user.Id;
                await context.SaveChangesAsync(ct);
            }
        }
    }

 
    public static async Task SeedAppointmentsAsync(ClinicDbContext context, CancellationToken ct = default)
    {
        if (await context.Appointments.AnyAsync(ct)) return;

        var doctors = await context.Doctors.Where(d => d.IsActive).OrderBy(d => d.Id).ToListAsync(ct);
        var schedules = await context.Schedules.ToListAsync(ct);
        var patientIds = await context.Patients.OrderBy(p => p.Id).Select(p => p.Id).ToListAsync(ct);
        if (doctors.Count == 0 || patientIds.Count == 0) return;

        var today = ClinicTime.Today(DateTime.UtcNow);
        var rng = new Random(2026); // fixed seed -> same demo data every time
        var patientCursor = 0;
        int NextPatient() => patientIds[patientCursor++ % patientIds.Count];

        // Clinic-local time -> UTC (the app stores check-in/start/end as UTC).
        static DateTime ToUtc(DateOnly date, TimeOnly time) =>
            new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0, DateTimeKind.Utc) - ClinicTime.Offset;

        // ---- History: the previous 5 days, everything finished -------------
        for (var back = 5; back >= 1; back--)
        {
            var date = today.AddDays(-back);
            foreach (var doctor in doctors)
            {
                var schedule = schedules.FirstOrDefault(s => s.DoctorId == doctor.Id && s.DayOfWeek == date.DayOfWeek);
                if (schedule is null) continue;

                var slot = schedule.OpenTime;
                var sequence = 0;
                var count = rng.Next(4, 8);

                for (var i = 0; i < count && slot < schedule.CloseTime; i++)
                {
                    var roll = rng.Next(100);
                    var appt = new Appointment
                    {
                        DoctorId = doctor.Id,
                        PatientId = NextPatient(),
                        Date = date,
                        SlotTime = slot
                    };

                    if (roll < 75) // completed visit
                    {
                        sequence++;
                        var checkIn = slot.AddMinutes(-rng.Next(0, 10));
                        var start = slot.AddMinutes(rng.Next(0, 15));
                        var end = start.AddMinutes(Math.Max(3, doctor.AvgConsultMinutes + rng.Next(-4, 6)));

                        appt.Status = AppointmentStatus.Completed;
                        appt.QueueNo = QueueNumberFormatter.Format(doctor.QueueLetter, sequence);
                        appt.CheckInAt = ToUtc(date, checkIn);
                        appt.StartAt = ToUtc(date, start);
                        appt.EndAt = ToUtc(date, end);
                        appt.Events.Add(Event(null, AppointmentStatus.Booked, "Booked"));
                        appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.Waiting, $"Checked in, queue number {appt.QueueNo}"));
                        appt.Events.Add(Event(AppointmentStatus.Waiting, AppointmentStatus.Serving, "Consultation started"));
                        appt.Events.Add(Event(AppointmentStatus.Serving, AppointmentStatus.Completed, "Consultation finished"));
                    }
                    else if (roll < 90) // no-show
                    {
                        appt.Status = AppointmentStatus.NoShow;
                        appt.Events.Add(Event(null, AppointmentStatus.Booked, "Booked"));
                        appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.NoShow, "Did not arrive"));
                    }
                    else // cancelled
                    {
                        appt.Status = AppointmentStatus.Cancelled;
                        appt.Events.Add(Event(null, AppointmentStatus.Booked, "Booked"));
                        appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.Cancelled, "Cancelled by patient"));
                    }

                    context.Appointments.Add(appt);
                    slot = slot.AddMinutes(schedule.SlotMinutes * 2); // leave gaps so it looks realistic
                }
            }
        }

        // ---- Today: a live queue per doctor ---------------------------------
        foreach (var doctor in doctors)
        {
            var schedule = schedules.FirstOrDefault(s => s.DoctorId == doctor.Id && s.DayOfWeek == today.DayOfWeek);
            if (schedule is null) continue;

            var slot = schedule.OpenTime;
            var sequence = 0;
            // Plan: 1 completed, 1 serving, 2 waiting, 2 booked, 1 no-show, 1 cancelled
            AppointmentStatus[] plan =
            [
                AppointmentStatus.Completed, AppointmentStatus.Serving,
                AppointmentStatus.Waiting,   AppointmentStatus.Waiting,
                AppointmentStatus.Booked,    AppointmentStatus.Booked,
                AppointmentStatus.NoShow,    AppointmentStatus.Cancelled,
            ];

            foreach (var status in plan)
            {
                if (slot >= schedule.CloseTime) break;

                var appt = new Appointment
                {
                    DoctorId = doctor.Id,
                    PatientId = NextPatient(),
                    Date = today,
                    SlotTime = slot,
                    Status = status
                };
                appt.Events.Add(Event(null, AppointmentStatus.Booked, "Booked"));

                if (status is AppointmentStatus.Completed or AppointmentStatus.Serving
                    or AppointmentStatus.Waiting)
                {
                    sequence++;
                    appt.QueueNo = QueueNumberFormatter.Format(doctor.QueueLetter, sequence);
                    appt.CheckInAt = ToUtc(today, slot.AddMinutes(-5));
                    appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.Waiting, $"Checked in, queue number {appt.QueueNo}"));
                }

                if (status is AppointmentStatus.Completed or AppointmentStatus.Serving)
                {
                    appt.StartAt = ToUtc(today, slot);
                    appt.Events.Add(Event(AppointmentStatus.Waiting, AppointmentStatus.Serving, "Consultation started"));
                }

                if (status == AppointmentStatus.Completed)
                {
                    appt.EndAt = ToUtc(today, slot.AddMinutes(doctor.AvgConsultMinutes));
                    appt.Events.Add(Event(AppointmentStatus.Serving, AppointmentStatus.Completed, "Consultation finished"));
                }

                if (status == AppointmentStatus.NoShow)
                    appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.NoShow, "Did not arrive"));

                if (status == AppointmentStatus.Cancelled)
                    appt.Events.Add(Event(AppointmentStatus.Booked, AppointmentStatus.Cancelled, "Cancelled by patient"));

                context.Appointments.Add(appt);
                slot = slot.AddMinutes(schedule.SlotMinutes);
            }

            // One walk-in, waiting (walk-ins have no slot conflict; they share the queue numbering).
            sequence++;
            var walkInQueueNo = QueueNumberFormatter.Format(doctor.QueueLetter, sequence);
            var walkIn = new Appointment
            {
                DoctorId = doctor.Id,
                PatientId = NextPatient(),
                Date = today,
                SlotTime = TimeOnly.FromDateTime(ClinicTime.ToLocal(DateTime.UtcNow)),
                Status = AppointmentStatus.Waiting,
                IsWalkIn = true,
                QueueNo = walkInQueueNo,
                CheckInAt = DateTime.UtcNow
            };
            walkIn.Events.Add(Event(null, AppointmentStatus.Waiting, $"Walk-in, queue number {walkInQueueNo}"));
            context.Appointments.Add(walkIn);
        }

        // ---- Upcoming: a few future bookings --------------------------------
        for (var ahead = 1; ahead <= 3; ahead++)
        {
            var date = today.AddDays(ahead);
            foreach (var doctor in doctors)
            {
                var schedule = schedules.FirstOrDefault(s => s.DoctorId == doctor.Id && s.DayOfWeek == date.DayOfWeek);
                if (schedule is null) continue;

                var slot = schedule.OpenTime.AddMinutes(schedule.SlotMinutes * rng.Next(0, 3));
                for (var i = 0; i < 2 && slot < schedule.CloseTime; i++)
                {
                    var appt = new Appointment
                    {
                        DoctorId = doctor.Id,
                        PatientId = NextPatient(),
                        Date = date,
                        SlotTime = slot,
                        Status = AppointmentStatus.Booked
                    };
                    appt.Events.Add(Event(null, AppointmentStatus.Booked, "Booked"));
                    context.Appointments.Add(appt);
                    slot = slot.AddMinutes(schedule.SlotMinutes * 3);
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private static AppointmentEvent Event(AppointmentStatus? from, AppointmentStatus to, string note) =>
        new() { FromStatus = from, ToStatus = to, Note = note };
}