using IllinoisUrbanaAssessment.Models;

namespace IllinoisUrbanaAsssesment.Models
{
    public class Person
    {
        public string Name { get; set; }
        public List<Completion> Completions { get; set; }
    }
}
