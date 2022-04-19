using AddTimeEntryServices.Commands;

namespace AddTimeEntryServices.Services;

public interface IAddTimeEntryService
{
    void AddTimeEntry(AddTimeEntryCommand command);
}

public class AddTimeEntryService: IAddTimeEntryService
{
    public void AddTimeEntry(AddTimeEntryCommand command)
    {
        throw new NotImplementedException();
    }
}