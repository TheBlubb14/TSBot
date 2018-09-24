using System.ComponentModel.DataAnnotations;

namespace TSBot.DB
{
    public class TSUser
    {
        [Key]
        public string UID { get; set; }

        public string Name { get; set; }

        public bool Accepted { get; set; }

        public TSUser(string uID, string name, bool accepted = false)
        {
            this.UID = uID;
            this.Name = name;
            this.Accepted = accepted;
        }
    }
}
