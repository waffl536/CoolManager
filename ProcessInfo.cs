using System.Diagnostics;
class ProcessInfo
{
    private TimeSpan lastTotalProcessorTime;

    private string _processName = "";
    public string ProcessName 
    {
        get => _processName;
        set {
            _processName = string.IsNullOrEmpty(value) ? "<no name>" : value;
        }
    }
    public TimeSpan CpuTime {get; set;}
    public float CpuPercent {get; set;}
    public long MemoryUsage {get; set;}
    public float MemoryUsagePercent {get; set;}
    private Process _processObject;
    public Process ProcessObject
    {
        get => _processObject;
    }
    
    private ProcessInfo(Process p)
    {
        _processObject = p;
        ProcessName = p.ProcessName;
        try
        {
            CpuTime = p.TotalProcessorTime;
            MemoryUsage = p.WorkingSet64;
        }
        catch
        {
            CpuTime = TimeSpan.FromMilliseconds(0);
            MemoryUsage = 0;
        }
        CpuPercent = 0; 
        MemoryUsagePercent = MemoryUsage == 0 ? 0 : (float)(MemoryUsage / (double)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes);
    }

    public static List<ProcessInfo> GetProcessesInfo()
    {
        Process[] processes = Process.GetProcesses();
        List<ProcessInfo> infoList = new List<ProcessInfo>();
        DateTime lastProcessorTime = DateTime.Now;
        foreach (Process p in processes)
        {
            if(string.IsNullOrEmpty(p.ProcessName)) { continue; }
            ProcessInfo processInfo = new ProcessInfo(p);
            try
            {
                processInfo.lastTotalProcessorTime = processInfo._processObject.TotalProcessorTime;
            }
            catch
            {
                processInfo.lastTotalProcessorTime = TimeSpan.FromMilliseconds(0);
            }
            infoList.Add(processInfo);
        }

        Thread.Sleep(250);
        DateTime now = DateTime.Now;
        for (int i = 0; i < infoList.Count; i++)
        {
            ProcessInfo processInfo = infoList[i];
            try
            {
                processInfo._processObject = Process.GetProcessById(processInfo._processObject.Id);
            }
            catch
            {
                continue;
            }

            TimeSpan curTotalProcessorTime;
            try
            {
                curTotalProcessorTime = processInfo._processObject.TotalProcessorTime;
            }
            catch
            {
                curTotalProcessorTime = TimeSpan.FromMilliseconds(0);
            }
            double elapsedTime = now.Subtract(lastProcessorTime).TotalMilliseconds;
            if (elapsedTime <= 0) elapsedTime = 1;

            double CPUUsage = (
                (curTotalProcessorTime.TotalMilliseconds - processInfo.lastTotalProcessorTime.TotalMilliseconds) /
                elapsedTime / double.CreateChecked(Environment.ProcessorCount)
            );

            // processInfo.CpuPercent = (float)Math.Round(CPUUsage * 100, 2);
            processInfo.CpuPercent = float.CreateChecked(CPUUsage * 100);
        }

        return infoList;
    }

    public string HumanCpuTimeFormat() => $"{CpuTime.TotalSeconds:F2}s";
    public string HumanCpuPercentFormat() => $"{CpuPercent * 100:F2}%";
    public string HumanMemoryUsageFormat()
    {
        int bytesInKbytes = 1000;
        long bytes = MemoryUsage;
        long kbytes = bytes / bytesInKbytes;
        long mbytes = kbytes / bytesInKbytes;
        long gbytes = mbytes / bytesInKbytes;
        long tbytes = gbytes / bytesInKbytes;
        bytes %= bytesInKbytes;
        kbytes %= bytesInKbytes;
        mbytes %= bytesInKbytes;
        gbytes %= bytesInKbytes;
        tbytes %= bytesInKbytes;

        if (tbytes > 0) {
            return tbytes + "." + (gbytes * 10 / bytesInKbytes) + " TB";
        } else if (gbytes > 0) {
            return gbytes + "." + (mbytes * 10 / bytesInKbytes) + " GB";
        } else if (mbytes > 0) {
            return mbytes + "." + (kbytes * 10 / bytesInKbytes) + " MB";
        } else if (kbytes > 0) {
            return kbytes + "." +(bytes * 10 / bytesInKbytes) + " KB";
        } else {
            return bytes + " B";
        }
    }
    public string HumanMemoryUsagePercentFormat() => $"{MemoryUsagePercent * 100:F2}%";
}
