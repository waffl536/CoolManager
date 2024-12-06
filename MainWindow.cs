using System.Data;
using System.Diagnostics;
using Terminal.Gui;

class MainWindow : Window
{
    private TableView processesTableView;
    public MainWindow()
    {
        Title = "CoolManager";
        Button quitBtn = new Button() 
        {
            X = Pos.Right(this) -10,
            Y = 0,
            Text = "Exit",
        };
        quitBtn.Accept += (s,e) => Application.RequestStop();
        Add(quitBtn);
        Button killBtn = new Button() 
        {
            X = Pos.Right(this) -25,
            Y = 0,
            Text = "Kill",
        };
        killBtn.Accept += async (s,e) => await KillSelectedAsync();
        Add(killBtn);
        processesTableView = new TableView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            MinCellWidth = 25
        };
        Add(processesTableView);
    }
    public void UpdateProcessesTable(List<ProcessInfo> infoList)
    {
        int lastSelectedRow = processesTableView.SelectedRow;
        int lastSelectedColumn = processesTableView.SelectedColumn;
        DataTable dt = new DataTable(); 
        DataColumn processName = dt.Columns.Add("Process Name");
        DataColumn cpuTime = dt.Columns.Add("CPU time");
        DataColumn cpuPercent = dt.Columns.Add("CPU %");
        DataColumn memoryUsage = dt.Columns.Add("RAM");
        DataColumn memoryPercent = dt.Columns.Add("RAM %");
        foreach (ProcessInfo p in infoList)
        {
            dt.Rows.Add(p.ProcessName, p.HumanCpuTimeFormat(), p.HumanCpuPercentFormat(), p.HumanMemoryUsageFormat(), p.HumanMemoryUsagePercentFormat());
        }
        processesTableView.Table = new DataTableSource(dt);
        if (lastSelectedRow >= 0 && lastSelectedRow < dt.Rows.Count && lastSelectedColumn >= 0 && lastSelectedColumn < dt.Columns.Count)
        {
            processesTableView.SelectedRow = lastSelectedRow;
            processesTableView.SelectedColumn = lastSelectedColumn;
        }
    }
    private async Task KillSelectedAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                var processName = processesTableView.Table[processesTableView.SelectedRow, processesTableView.SelectedColumn].ToString();
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                process?.Kill();
            });
        }
        catch
        {
            // Handle exceptions
        }
    }
    public bool Update()
    {
        UpdateProcessesTable(ProcessInfo.GetProcessesInfo());
        return true;
    }
}