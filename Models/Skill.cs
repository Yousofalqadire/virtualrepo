using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("Skills")]
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
         public Photo Photo { get; set; }
    }
}