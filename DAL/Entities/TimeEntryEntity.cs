using DAL.Entities.Abstract;

namespace DAL.Entities;

public class TimeEntryEntity: BaseEntity
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class TimeEntryModel
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}