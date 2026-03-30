namespace BlockGame
{
    public static class MainThread
    {
        private static List<Action> executionQueue = new List<Action>();
        private static List<Action> stagingQueue = new List<Action>();

        public static void Run(Action action)
        {
            lock (stagingQueue)
            {
                stagingQueue.Add(action);
            }
        }

        public static void Update()
        {
            executionQueue.Clear();

            lock (stagingQueue)
            {
                var temp = stagingQueue;
                stagingQueue = executionQueue;
                executionQueue = temp;
            }

            foreach (var action in executionQueue)
            {
                action?.Invoke();
            }
        }
    }

}
