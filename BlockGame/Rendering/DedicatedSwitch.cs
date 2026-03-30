namespace BlockGame.Rendering
{
    internal class DedicatedSwitch
    {
        /*
         *  This script is used to force the OS to switch to the dedicated GPU instead of an intergraded one.
         */


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool SetDllDirectory(string lpPathName);

        [System.Runtime.InteropServices.DllImport("nvapi64.dll", EntryPoint = "fake")]
        private static extern void Fake();

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        public static void Switch()
        {
            SetEnvironmentVariable("SHIM_MCCOMPAT", "0x800000001");
            SetEnvironmentVariable("NvOptimusEnablement", "0x00000001");
            SetEnvironmentVariable("AmdPowerXpressRequestHighPerformance", "1");
        }
    }
}
