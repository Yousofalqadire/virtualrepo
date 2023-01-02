using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
      [Table("HomeWorks")]
    public class HomeWork
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string FileUrl { get; set; }
        public string Name { get; set; }
    }
}