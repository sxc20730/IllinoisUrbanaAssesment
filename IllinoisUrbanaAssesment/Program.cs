using System.Globalization;
using IllinoisUrbanaAsssesment.Models;
using Newtonsoft.Json;

namespace IllinoisUrbanaAssesment
{

    public class Program
    {
        static void Main(string[] args)
        {
            string inputFile = "./trainings.json";
            string outputTrainingCompletionFile = "trainingCompletionCounts.json";
            string outputPeopleInFiscalYearFile = "peopleInFiscalYear.json";
            string outputExpiredTrainingsFile = "expiredTrainings.json";

            List<Person> people = ReadJsonData(inputFile);

            var trainingCompletionCounts = GetTrainingCompletionCounts(people);
            WriteToFile(outputTrainingCompletionFile, trainingCompletionCounts);

            DateTime fiscalYearStart = new DateTime(2023, 7, 1);
            DateTime fiscalYearEnd = new DateTime(2024, 6, 30);
            string[] trainings = { "Electrical Safety for Labs", "X-Ray Safety", "Laboratory Safety Training" };
            var peopleInFiscalYear = GetPeopleInFiscalYear(people, trainings, fiscalYearStart, fiscalYearEnd);
            WriteToFile(outputPeopleInFiscalYearFile, peopleInFiscalYear);
            
            DateTime checkDate = new DateTime(2023, 10, 1);
            var expiredTrainings = GetExpiredTrainings(people, checkDate);
            WriteToFile(outputExpiredTrainingsFile, expiredTrainings);
        }

        private static List<Person> ReadJsonData(string filePath)
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Person>>(jsonData);
        }

        private static void WriteToFile(string filePath, object data)
        {
            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented); 
            File.WriteAllText(filePath, json);
        }

        private static List<dynamic> GetTrainingCompletionCounts(List<Person> people)
        {
            return people
                .SelectMany(p => p.Completions)
                .GroupBy(c => c.Name)
                .Select(g => new { Training = g.Key, Count = g.Count() })
                .Cast<dynamic>()
                .ToList();
        }

        private static List<dynamic> GetPeopleInFiscalYear(List<Person> people, string[] trainings, DateTime fiscalYearStart, DateTime fiscalYearEnd)
        {
            return people
                .Select(p => new
                {
                    Person = p.Name,
                    Trainings = p.Completions
                        .Where(c => trainings.Contains(c.Name) && DateTime.Parse(c.Timestamp, CultureInfo.InvariantCulture) >= fiscalYearStart && DateTime.Parse(c.Timestamp, CultureInfo.InvariantCulture) <= fiscalYearEnd)
                        .Select(c => c.Name)
                        .ToList()
                })
                .Where(p => p.Trainings.Any())
                .Cast<dynamic>()
                .ToList();
        }

        private static List<dynamic> GetExpiredTrainings(List<Person> people, DateTime checkDate)
        {
            DateTime expirationThreshold = checkDate.AddMonths(1);
            return people
                .Select(p => new
                {
                    Person = p.Name,
                    ExpiringTrainings = p.Completions
                        .Where(c => !string.IsNullOrEmpty(c.Expires) && DateTime.Parse(c.Expires, CultureInfo.InvariantCulture) <= expirationThreshold)
                        .Select(c => new
                        {
                            Training = c.Name,
                            ExpirationDate = c.Expires,
                            Status = DateTime.Parse(c.Expires, CultureInfo.InvariantCulture) <= checkDate ? "Expired" : "Expiring Soon"
                        })
                        .ToList()
                })
                .Where(p => p.ExpiringTrainings.Any())
                .Cast<dynamic>()
                .ToList();
        }
    }
}
