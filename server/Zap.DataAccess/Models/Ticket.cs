namespace Zap.DataAccess.Models;

public class Ticket
{
   public string Id { get; set; } = Guid.NewGuid().ToString();
   public string Name { get; set; } 
   public string Description { get; set; }
   public string Priority { get; set; }
   public string Status { get; set; }
   public string Type { get; set; }
   public AppUser? Submitter { get; set; }
   public AppUser? Assignee { get; set; }
}