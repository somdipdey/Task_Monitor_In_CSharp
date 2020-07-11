using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using SystemPerformance;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TaskMonitor
{
    class Program
    {
        /// <summary>
        /// Provide an appGuid for tracking purposes
        /// </summary>
        private static string appGuid = "f6d4871e-b63e-4685-89fc-155bb5cd95e5";

        static void Main(string[] args)
        {
            // Check for valid arguments
            /*if (args.Length < 1)
            {
                Console.WriteLine("Please enter valid argument:\n" +
                                    "help: for more info\n" + ">");
                string enteredArgs = Console.ReadLine();
                ExecuteCommand(enteredArgs);

            }
            else*/ if (args.Length > 1)
            {
                Console.WriteLine("Please enter only one valid argument at a time:\n" +
                    "help: for more info\n" + ">");
                string enteredArgs = Console.ReadLine();
                ExecuteCommand(enteredArgs);
            }
            // Passes the command "run" by default for debugging purposes.
            else
            {
                ExecuteCommand("run");
            }
        }

        async private static void ExecuteCommand(string arg)
        {
            // If "help" is passed as argument
            if (arg.ToLower().Trim() == "help")
            {
                Console.WriteLine("Valid Argument::\n" +
                                    "run: To start monitoring tasks and system performance\n" +
                                    "stop: To stop monitoring tasks and system performance\n" +
                                    "exit: To exit from this program" + ">");
                string enteredArg = Console.ReadLine();
                ExecuteCommand(enteredArg);
            }
            // If "exit" is passed as argument
            else if (arg.ToLower().Trim() == "exit")
            {
                Environment.Exit(0);
            }
            // If "run" is passed as argument
            else if (arg.ToLower().Trim() == "run")
            {
                if(ThisProgramAlreadyRunning())
                {
                    Console.WriteLine("The program is already running. Please stop the previous instance and then run again.\n" + ">");
                    string enteredArg = Console.ReadLine();
                    ExecuteCommand(enteredArg);
                }
                // System monitoring is performed in a separate thread with lowest priority for maximum responsiveness
                //Task executeThread = new Task();
                //executeThread.Priority = ThreadPriority.Lowest;
                //executeThread.IsBackground = true;
                //executeThread.Start();

                //Using a Task instead of a Thread to avoid the Exception on File Creation.
                await Execute();
            }
            // If "stop" is passed as argument
            else if (arg.ToLower().Trim() == "stop")
            {
                TerminateThisProgram();
            }
            else {
                Console.WriteLine("Invalid argument. Program exiting with code 0.");
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// The method that keeps track of the system performance and writes it to a CSV file
        /// </summary>
        /// <returns></returns>
        private static async Task Execute()
        {
            Console.WriteLine("Task-monitor started running....");
            try
            {
                while (true)
                {
                    PerformanceTracker sysPerformance = new PerformanceTracker();
                    CheckUsers users = new CheckUsers();

                    var machineName = users.GetMachineName();
                    var totalUsers = users.GetTotalUserCount();
                    var totalSessions = users.GetTotalSessionCount();
                    var cpuUsage = sysPerformance.Current_CPU_Usage;
                    var ramUsageInPercent = sysPerformance.Percent_RAM_Used;
                    var ramUsageInMb = sysPerformance.RAM_Used;
                    var totalRam = sysPerformance.Total_SYS_RAM_In_MB;
                    Process[] allProcesses = Process.GetProcesses();

                    // CSV Format:: ServerName,TotalUsers,Sessions,ProcessesRunning,CPUUsage,RAMInPercent,RAMUsed,TotalRAM,DateTime
                    string outputString = machineName + "," + totalUsers + "," + totalSessions + "," + allProcesses.Length + "," + cpuUsage + "," + ramUsageInPercent + "," +
                                            ramUsageInMb + "," + totalRam + "," + DateTime.Now;
                    WriteToFile(outputString, "task-monitor");
                    // Thread sleeps for 30 seconds before repeating the task
                    Thread.Sleep(30000);

                    // Initiate everything as null before starting again. This part is redundant and unnecessary.
                    machineName = null;
                    totalUsers = -1;
                    totalSessions = -1;
                    cpuUsage = -1;
                    ramUsageInPercent = -1;
                    ramUsageInMb = -1;
                    totalRam = -1;
                    allProcesses = null;
                    
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Exception occured: " + ex.Message + "\n Program exiting with code 1.");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Check if this program instance is already running in the system
        /// </summary>
        /// <returns></returns>
        private static bool ThisProgramAlreadyRunning()
        {

            using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
            {
                if (!mutex.WaitOne(0, false))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Terminate this program forcefully using command
        /// </summary>
        private static void TerminateThisProgram()
        {
            string process = Process.GetCurrentProcess().ProcessName;
            Process.Start("cmd.exe", "/c taskkill /F /IM " + process + ".exe /T");
        }

        // Method to write out to a CSV file
        private static void WriteToFile(string message, string fileName)
        {
            try
            {
                string Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                using (StreamWriter SW = new StreamWriter(Path + "\\" + fileName.Trim() + ".CSV", true))
                {
                    SW.WriteLine(message);
                    SW.Flush();
                    SW.Close();

                }
            }
            catch (Exception ex) { Console.WriteLine("Error writing out to file - " + fileName + ". Exception: " + ex.Message); }
        }

    }
}
