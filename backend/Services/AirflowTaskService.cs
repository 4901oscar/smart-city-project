using backend.Interfaces;

namespace backend.Services;

using System.Diagnostics;

public class AirflowTasksService : IAirflowTasksService
{
    public void TriggerDag(string dagId)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "airflow",
                Arguments = $"dags trigger {dagId}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine($"Airflow DAG triggered: {output}");
    }
}
