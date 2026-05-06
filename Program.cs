using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StajTakipSistemi
{
    class Program
    {
        static TaskManager manager = new TaskManager();
        static AnalyticsEngine analytics = new AnalyticsEngine(manager);

        static void Main(string[] args)
        {
            Console.Title = "Akıllı Staj Takip Sistemi";
            manager.LoadFromFile();

            while (true)
            {
                Console.Clear();
                PrintHeader();
                PrintSummaryBar();
                PrintMenu();

                string choice = Console.ReadLine()?.Trim() ?? "";
                HandleMenu(choice);
            }
        }

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║        AKILLI STAJ TAKİP SİSTEMİ  v1.0          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void PrintSummaryBar()
        {
            var stats = analytics.GetQuickStats();
            Console.Write("  Toplam: ");
            Console.ForegroundColor = ConsoleColor.White; Console.Write(stats.Total);
            Console.ResetColor(); Console.Write("  |  Tamamlanan: ");
            Console.ForegroundColor = ConsoleColor.Green; Console.Write(stats.Done);
            Console.ResetColor(); Console.Write("  |  Bekleyen: ");
            Console.ForegroundColor = ConsoleColor.Yellow; Console.Write(stats.Pending);
            Console.ResetColor(); Console.Write("  |  Geciken: ");
            Console.ForegroundColor = ConsoleColor.Red; Console.Write(stats.Overdue);
            Console.ResetColor();
            Console.WriteLine($"  |  Oran: %{stats.CompletionRate:F0}");
            Console.WriteLine(new string('─', 52));
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n  [1] Görevleri Listele      [5] Analitik Panel");
            Console.WriteLine("  [2] Görev Ekle             [6] Günlük Liste");
            Console.WriteLine("  [3] Görev Güncelle         [7] Görev Ara");
            Console.WriteLine("  [4] Görev Sil              [0] Çıkış\n");
            Console.Write("  Seçiminiz: ");
        }

        static void HandleMenu(string choice)
        {
            switch (choice)
            {
                case "1": ListTasksMenu(); break;
                case "2": AddTaskMenu(); break;
                case "3": UpdateTaskMenu(); break;
                case "4": DeleteTaskMenu(); break;
                case "5": ShowAnalytics(); break;
                case "6": ShowDailyList(); break;
                case "7": SearchMenu(); break;
                case "0": ExitApp(); break;
                default: ShowError("Geçersiz seçim."); break;
            }
        }

        static void ListTasksMenu()
        {
            Console.Clear();
            Console.WriteLine("\n  FİLTRE: [1] Tümü  [2] Bekleyen  [3] Tamamlanan  [4] Yüksek Öncelik  [5] Geciken");
            Console.Write("  Seçim: ");
            string f = Console.ReadLine()?.Trim() ?? "1";

            FilterType filter = f switch
            {
                "2" => FilterType.Pending,
                "3" => FilterType.Done,
                "4" => FilterType.HighPriority,
                "5" => FilterType.Overdue,
                _ => FilterType.All
            };

            var tasks = manager.GetFiltered(filter);
            PrintTaskList(tasks);
            Pause();
        }

        static void PrintTaskList(List<InternTask> tasks)
        {
            if (!tasks.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  Görev bulunamadı.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            foreach (var task in tasks)
            {
                PrintTaskRow(task);
            }
        }

        static void PrintTaskRow(InternTask task)
        {
            string status = task.IsDone ? "✓" : "○";
            ConsoleColor statusColor = task.IsDone ? ConsoleColor.Green :
                                       task.IsOverdue ? ConsoleColor.Red : ConsoleColor.Yellow;

            Console.ForegroundColor = statusColor;
            Console.Write($"  [{status}] ");
            Console.ResetColor();

            Console.Write($"#{task.Id:D3} ");

            if (task.IsDone)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{task.Title,-30}");
            Console.ResetColor();

            PrintPriorityBadge(task.Priority);

            if (task.Deadline.HasValue)
            {
                int days = (task.Deadline.Value - DateTime.Today).Days;
                if (task.IsDone)
                { Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write($"  {task.Deadline.Value:dd.MM.yyyy}"); }
                else if (days < 0)
                { Console.ForegroundColor = ConsoleColor.Red; Console.Write($"  {Math.Abs(days)} gün GECIKTI"); }
                else if (days == 0)
                { Console.ForegroundColor = ConsoleColor.Red; Console.Write("  BUGÜN!"); }
                else
                { Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write($"  {days} gün kaldı"); }
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        static void PrintPriorityBadge(Priority p)
        {
            Console.ForegroundColor = p switch
            {
                Priority.High => ConsoleColor.Red,
                Priority.Medium => ConsoleColor.Yellow,
                Priority.Low => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };
            string label = p switch { Priority.High => "[YÜK]", Priority.Medium => "[ORT]", _ => "[DÜŞ]" };
            Console.Write(label);
            Console.ResetColor();
        }

        static void AddTaskMenu()
        {
            Console.Clear();
            Console.WriteLine("\n  ── YENİ GÖREV EKLE ──\n");

            Console.Write("  Başlık: ");
            string title = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(title)) { ShowError("Başlık boş olamaz."); Pause(); return; }

            Console.Write("  Açıklama (Enter ile geç): ");
            string desc = Console.ReadLine()?.Trim() ?? "";

            Console.Write("  Deadline (gg.aa.yyyy, Enter=bugün): ");
            string dateStr = Console.ReadLine()?.Trim() ?? "";
            DateTime? deadline = null;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime dt))
                deadline = dt;
            else
                deadline = DateTime.Today;

            Console.Write("  Öncelik [1=Yüksek, 2=Orta, 3=Düşük, Enter=Orta]: ");
            string pStr = Console.ReadLine()?.Trim() ?? "2";
            Priority priority = pStr switch { "1" => Priority.High, "3" => Priority.Low, _ => Priority.Medium };

            var task = new InternTask(title, desc, deadline, priority);
            manager.Add(task);
            manager.SaveToFile();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Görev eklendi: #{task.Id:D3} — {task.Title}");
            Console.ResetColor();
            Pause();
        }

        static void UpdateTaskMenu()
        {
            Console.Clear();
            PrintTaskList(manager.GetFiltered(FilterType.All));
            Console.Write("\n  Güncellenecek görev ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { ShowError("Geçersiz ID."); Pause(); return; }

            var task = manager.GetById(id);
            if (task == null) { ShowError("Görev bulunamadı."); Pause(); return; }

            Console.WriteLine($"\n  Görev: {task.Title}");
            Console.WriteLine("  [1] Tamamlandı olarak işaretle  [2] Başlık değiştir  [3] Deadline değiştir  [4] Öncelik değiştir");
            Console.Write("  Seçim: ");
            string action = Console.ReadLine()?.Trim() ?? "";

            switch (action)
            {
                case "1":
                    task.IsDone = !task.IsDone;
                    task.CompletedAt = task.IsDone ? DateTime.Now : null;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(task.IsDone ? "  ✓ Tamamlandı!" : "  ○ Geri alındı.");
                    Console.ResetColor();
                    break;
                case "2":
                    Console.Write("  Yeni başlık: ");
                    string newTitle = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(newTitle)) task.Title = newTitle;
                    break;
                case "3":
                    Console.Write("  Yeni deadline (gg.aa.yyyy): ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime nd)) task.Deadline = nd;
                    break;
                case "4":
                    Console.Write("  Öncelik [1=Yüksek, 2=Orta, 3=Düşük]: ");
                    string pp = Console.ReadLine()?.Trim() ?? "2";
                    task.Priority = pp switch { "1" => Priority.High, "3" => Priority.Low, _ => Priority.Medium };
                    break;
            }

            manager.SaveToFile();
            Pause();
        }

        static void DeleteTaskMenu()
        {
            Console.Clear();
            PrintTaskList(manager.GetFiltered(FilterType.All));
            Console.Write("\n  Silinecek görev ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) { ShowError("Geçersiz ID."); Pause(); return; }

            var task = manager.GetById(id);
            if (task == null) { ShowError("Görev bulunamadı."); Pause(); return; }

            Console.Write($"  '{task.Title}' silinecek. Emin misiniz? (E/H): ");
            if (Console.ReadLine()?.Trim().ToUpper() == "E")
            {
                manager.Delete(id);
                manager.SaveToFile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✓ Görev silindi.");
                Console.ResetColor();
            }
            Pause();
        }

        static void ShowAnalytics()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  ── ANALİTİK PANEL ──\n");
            Console.ResetColor();

            var report = analytics.GenerateReport();

            Console.WriteLine($"  Tamamlanma Oranı : %{report.CompletionRate:F1}");
            PrintProgressBar(report.CompletionRate);

            Console.WriteLine($"\n  Toplam Görev     : {report.Total}");
            Console.WriteLine($"  Tamamlanan       : {report.Done}");
            Console.WriteLine($"  Bekleyen         : {report.Pending}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Geciken          : {report.Overdue}");
            Console.ResetColor();

            Console.WriteLine("\n  ── Öncelik Dağılımı ──");
            PrintPriorityStats(report);

            if (report.MostOverdue.Any())
            {
                Console.WriteLine("\n  ── En Çok Geciken Görevler ──");
                foreach (var (task, days) in report.MostOverdue)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"  ! {days} gün : ");
                    Console.ResetColor();
                    Console.WriteLine(task.Title);
                }
            }

            Pause();
        }

        static void PrintProgressBar(double pct)
        {
            int filled = (int)(pct / 100 * 40);
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('█', filled));
            Console.ResetColor();
            Console.Write(new string('░', 40 - filled));
            Console.Write($"] %{pct:F0}\n");
        }

        static void PrintPriorityStats(AnalyticsReport report)
        {
            void Row(string label, int count, int total, ConsoleColor color)
            {
                Console.ForegroundColor = color;
                Console.Write($"  {label,-8}");
                Console.ResetColor();
                int w = total > 0 ? (int)((double)count / total * 20) : 0;
                Console.Write($"  {'█'.ToString().PadRight(w > 0 ? w : 1, '█').PadRight(20, '░')}  {count}");
                Console.WriteLine();
            }
            Row("Yüksek", report.HighCount, report.Total, ConsoleColor.Red);
            Row("Orta", report.MediumCount, report.Total, ConsoleColor.Yellow);
            Row("Düşük", report.LowCount, report.Total, ConsoleColor.Green);
        }

        static void ShowDailyList()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  ── GÜNLÜK LİSTE — {DateTime.Today:dd MMMM yyyy} ──\n");
            Console.ResetColor();

            var todayTasks = manager.GetTodayTasks();
            var upcomingTasks = manager.GetUpcomingTasks(5);

            Console.WriteLine("  BUGÜNÜN GÖREVLERİ:");
            if (todayTasks.Any())
                PrintTaskList(todayTasks);
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Bugün için görev yok! 🎉");
                Console.ResetColor();
            }

            if (upcomingTasks.Any())
            {
                Console.WriteLine("\n  YAKLAŞAN GÖREVLER:");
                PrintTaskList(upcomingTasks);
            }

            Pause();
        }

        static void SearchMenu()
        {
            Console.Clear();
            Console.Write("\n  Arama terimi: ");
            string q = Console.ReadLine()?.Trim() ?? "";
            var results = manager.Search(q);
            Console.WriteLine($"\n  '{q}' için {results.Count} sonuç:");
            PrintTaskList(results);
            Pause();
        }

        static void ExitApp()
        {
            manager.SaveToFile();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Güle güle! Veriler kaydedildi.\n");
            Console.ResetColor();
            Environment.Exit(0);
        }

        static void ShowError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  Hata: {msg}");
            Console.ResetColor();
        }

        static void Pause()
        {
            Console.WriteLine("\n  [Enter] Ana menüye dön...");
            Console.ReadLine();
        }
    }
}