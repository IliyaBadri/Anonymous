namespace Anonymous.State
{
    public class ApplicationState
    {
        private static int lastProcessId = 0; 
        private static readonly List<ProcessInfo> processList = [];

        public class ProcessInfo
        {
            public required int Id { get; set; }
            public required string Name { get; set; }
            public required int Percentage { get; set; }

            public bool IsComplete()
            {
                if(Percentage >= 100)
                {
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        public class StateInfo
        {
            public required int Percentage { get; set; }
            public required int ProcessesLeft { get; set; }
            public required string MainTask { get; set; }

        }

        public static event EventHandler? StateUpdated;

        public static void CallUpdate()
        {
            StateUpdated?.Invoke(null, EventArgs.Empty);
        }

        public static int AddProcess(string name, int percentage)
        {
            int clampedPercentage = Math.Clamp(percentage, 0, 100);
            ProcessInfo process = new()
            {
                Id = lastProcessId + 1,
                Name = name,
                Percentage = clampedPercentage
            };
            processList.Add(process);
            lastProcessId++;
            return process.Id;
        }

        public static void DeleteProcess(int processId)
        {
            foreach (ProcessInfo process in processList)
            {
                if (process.Id == processId)
                {
                    processList.Remove(process);
                    break;
                }
            }
        }

        public static int GetProcessPercentage(int processId)
        {
            foreach (ProcessInfo process in processList)
            {
                if (process.Id == processId)
                {
                    return process.Percentage;
                }
            }
            return 0;
        }

        public static void EditProcessPercentage(int processId, int newPercentage)
        {
            foreach (ProcessInfo process in processList)
            {
                if (process.Id == processId)
                {
                    ProcessInfo newProcess = new()
                    {
                        Id = process.Id,
                        Name = process.Name,
                        Percentage = newPercentage
                    };
                    processList.Remove(process);
                    processList.Add(newProcess);
                    break;
                }
            }
        }

        public static void EditProcessName(int processId, string newName)
        {
            foreach (ProcessInfo process in processList)
            {
                if (process.Id == processId)
                {
                    ProcessInfo newProcess = new()
                    {
                        Id = process.Id,
                        Name = newName,
                        Percentage = process.Percentage
                    };
                    processList.Remove(process);
                    processList.Add(newProcess);
                    break;
                }
            }
        }

        public static StateInfo GetState()
        {
            int percentageSum = 0;
            int completeProcesses = 0;
            string mainTaskName = "Halt";
            if(processList.Count > 0)
            {
                mainTaskName = processList[0].Name;
            }
            foreach (ProcessInfo process in processList)
            {
                percentageSum += process.Percentage;
                if (process.IsComplete())
                {
                    completeProcesses++;
                }
            }
            int totalPercentage = 100;
            if (processList.Count > 0)
            {
                totalPercentage = (int)((double)percentageSum / (double)processList.Count);
            }
            StateInfo stateInfo = new() { 
                Percentage=totalPercentage,
                ProcessesLeft = processList.Count - completeProcesses,
                MainTask = mainTaskName
            };
            return stateInfo;
        }
    }
}
