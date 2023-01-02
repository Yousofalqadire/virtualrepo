using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("Tutorials")]
    public class Tutorial
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }
}