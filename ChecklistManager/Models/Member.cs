using System.ComponentModel.DataAnnotations;

namespace ChecklistManager.Models
{
    public class Member
    {
        [Key]
        public string Name { get; set; }
        public int Score { get; set; }

        public Member(string name)
        {
            Name = name;
            Score = 0;
        }
    }
}
