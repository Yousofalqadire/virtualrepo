using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("Reflections")]
    public class Refliction
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
    }
}