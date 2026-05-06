using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace StajTakipSistemi
{
    public class TaskManager
    {
        private List<InternTask> tasks = new List<InternTask>();
        private const string FilePath = "tasks.json";

        public void Add(InternTask task)
        {
            tasks.Add(task);
        }

        public void Delete(int id)
        {
            var task = GetById(id);
            if (task != null)
                tasks.Remove(task);
        }

        public InternTask? GetById(int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }

        public List<InternTask> GetFiltered(FilterType filter)
        {
            return filter switch
            {
                FilterType.Pending => tasks.Where(t => !t.IsDone).ToList(),
                FilterType.Done => tasks.Where(t => t.IsDone).ToList(),
                FilterType.HighPriority => tasks.Where(t => t.Priority == Priority.High).ToList(),
                FilterType.Overdue => tasks.Where(t => t.IsOverdue).ToList(),
                _ => tasks.ToList()
            };
        }

        public List<InternTask> Search(string query)
        {
            return tasks
                .Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || t.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<InternTask> GetTodayTasks()
        {
            return tasks
                .Where(t => t.Deadline.HasValue && t.Deadline.Value.Date == DateTime.Today)
                .ToList();
        }

        public List<InternTask> GetUpcomingTasks(int days)
        {
            var limit = DateTime.Today.AddDays(days);
            return tasks
                .Where(t => t.Deadline.HasValue &&
                            t.Deadline.Value.Date > DateTime.Today &&
                            t.Deadline.Value.Date <= limit)
                .ToList();
        }

        public void SaveToFile()
        {
            var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }

        public void LoadFromFile()
        {
            if (!File.Exists(FilePath)) return;

            var json = File.ReadAllText(FilePath);
            tasks = JsonSerializer.Deserialize<List<InternTask>>(json) ?? new List<InternTask>();

            // ID fix (ÇOK ÖNEMLİ)
            if (tasks.Any())
            {
                int maxId = tasks.Max(t => t.Id);
                InternTask.SetNextId(maxId + 1);
            }
        }
    }
}