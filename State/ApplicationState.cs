namespace Anonymous.State
{
    public class ApplicationState
    { 
        private static readonly List<ProcessState> processStates = [];

        public class ProcessState
        {
            public string title;
            private readonly int totalTasks;
            private int completedTasks = 0;

            public ProcessState(string title, int totalTasks)
            {
                this.totalTasks = totalTasks;
                this.title = title;
                processStates.Add(this);
            }

            public void Increment()
            {
                if (completedTasks + 1 <= totalTasks) {
                    completedTasks++;
                }
            }

            public int GetPercentage()
            {
                if (totalTasks > 0) {
                    return Math.Clamp((int)(((float)(completedTasks) / (float)(totalTasks)) * 100.0f), 0, 100);
                } else
                {
                    return 100;
                }
            }

            public bool IsComplete()
            {
                if(GetPercentage() >= 100)
                {
                    return true;
                } else
                {
                    return false;
                }
            }

            public void Destroy()
            {
                processStates.Remove(this);
            }
        }

        public class StateInfo
        {
            public required int Percentage { get; set; }
            public required int ProcessesLeft { get; set; }
            public required string MainState { get; set; }

        }

        public static event EventHandler? StateUpdated;

        public static void CallUpdate()
        {
            StateUpdated?.Invoke(null, EventArgs.Empty);
        }

        public static StateInfo GetState()
        {
            int percentageSum = 0;
            int completeProcesses = 0;
            string mainTaskName = "Halt";

            if(processStates.Count > 0)
            {
                mainTaskName = processStates[^1].title;
            }

            foreach (ProcessState processState in processStates)
            {
                percentageSum += processState.GetPercentage();

                if (processState.IsComplete())
                {
                    completeProcesses++;
                }
            }

            int totalPercentage = 100;
            if (processStates.Count > 0)
            {
                totalPercentage = (int)((float)percentageSum/(float)processStates.Count);
            }

            StateInfo stateInfo = new() { 
                Percentage=totalPercentage,
                ProcessesLeft = processStates.Count - completeProcesses,
                MainState = mainTaskName
            };

            return stateInfo;
        }
    }
}
