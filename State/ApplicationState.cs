namespace Anonymous.State
{
    public class ApplicationState
    {
        private static int lastProcessId = 0; 
        private static List<ProcessInfo> processList = [];

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

        public static int AddToQueue(ProcessInfo process)
        {
            ProcessInfo editedProcess = new()
            {
                Id = lastProcessId + 1,
                Name = process.Name,
                Percentage = process.Percentage
            };

            processList.Add(editedProcess);

            lastProcessId += 1;

            return editedProcess.Id;
        }

        public static int GetPercentage(int processId)
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

        public static void EditPercentage(int processId, int newPercentage)
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
                }
            }
        }
    }
}
