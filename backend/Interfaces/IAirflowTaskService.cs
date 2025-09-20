namespace backend.Interfaces;

public interface IAirflowTasksService
{
    void TriggerDag(string dagId);
}