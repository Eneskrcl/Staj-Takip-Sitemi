using System;

namespace StajTakipSistemi
{
    public enum Priority { Low, Medium, High }
    public enum FilterType { All, Pending, Done, HighPriority, Overdue }

    /// <summary>
    /// Staj görevi veri modeli - OOP'nin temel taşı
    /// </summary>
    public class InternTask
    {
        private static int _nextId = 1;

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public Priority Priority { get; set; }
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Computed properties - LINQ ve business logic için
        public bool IsOverdue => !IsDone && Deadline.HasValue && Deadline.Value.Date < DateTime.Today;
        public int DaysLeft => Deadline.HasValue ? (Deadline.Value.Date - DateTime.Today).Days : 0;
        public int DaysOverdue => IsOverdue ? Math.Abs(DaysLeft) : 0;

        public InternTask() { } // JSON deserialization için

        public InternTask(string title, string description, DateTime? deadline, Priority priority)
        {
            Id = _nextId++;
            Title = title;
            Description = description;
            Deadline = deadline;
            Priority = priority;
            IsDone = false;
            CreatedAt = DateTime.Now;
        }

        // Static factory - Id counter'ı dışarıdan set etmek için
        public static void SetNextId(int id) => _nextId = id;

        public override string ToString() =>
            $"[{(IsDone ? "✓" : "○")}] #{Id:D3} {Title} ({Priority}) {(Deadline.HasValue ? Deadline.Value.ToString("dd.MM.yyyy") : "Tarifsiz")}";
    }
}