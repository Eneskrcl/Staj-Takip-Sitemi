using System;
using System.Collections.Generic;
using System.Linq;

namespace StajTakipSistemi
{
    public class AnalyticsEngine
    {
        private TaskManager manager;

        public AnalyticsEngine(TaskManager manager)
        {
            this.manager = manager;
        }

        public QuickStats GetQuickStats()
        {
            var tasks = manager.GetFiltered(FilterType.All);

            return new QuickStats
            {
                Total = tasks.Count,
                Done = tasks.Count(t => t.IsDone),
                Pending = tasks.Count(t => !t.IsDone),
                Overdue = tasks.Count(t => t.IsOverdue),
                CompletionRate = tasks.Count == 0 ? 0 :
                    (double)tasks.Count(t => t.IsDone) / tasks.Count * 100
            };
        }

        public AnalyticsReport GenerateReport()
        {
            var tasks = manager.GetFiltered(FilterType.All);

            return new AnalyticsReport
            {
                Total = tasks.Count,
                Done = tasks.Count(t => t.IsDone),
                Pending = tasks.Count(t => !t.IsDone),
                Overdue = tasks.Count(t => t.IsOverdue),

                HighCount = tasks.Count(t => t.Priority == Priority.High),
                MediumCount = tasks.Count(t => t.Priority == Priority.Medium),
                LowCount = tasks.Count(t => t.Priority == Priority.Low),

                MostOverdue = tasks
                    .Where(t => t.IsOverdue)
                    .OrderByDescending(t => t.DaysOverdue)
                    .Take(5)
                    .Select(t => (t, t.DaysOverdue))
                    .ToList()
            };
        }
    }

    public class QuickStats
    {
        public int Total { get; set; }
        public int Done { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }
        public double CompletionRate { get; set; }
    }

    public class AnalyticsReport
    {
        public int Total { get; set; }
        public int Done { get; set; }
        public int Pending { get; set; }
        public int Overdue { get; set; }

        public int HighCount { get; set; }
        public int MediumCount { get; set; }
        public int LowCount { get; set; }
        public double CompletionRate { get; set; }
        public List<(InternTask task, int days)> MostOverdue { get; set; }
            = new();
    }
}