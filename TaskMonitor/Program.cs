using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using SystemPerformance;

namespace TaskMonitor
{
    class Program
    {
        // Provide an appGuid for tracking purposes
        private static string appGuid = "f6d4871e-b63e-4685-89fc-155bb5cd95e5";

        static void Main(string[] args)
        {
            // Check for valid arguments
            if (args.Length < 1)
            {
                Console.WriteLine("Please enter valid arugment:\n" +
                                    "help: for more info\n" + ">");
                string enteredArgs = Console.ReadLine();
                Program.ExecuteCommand(enteredArgs);

            }
            else if (args.Length > 1)
            {
                Console.WriteLine("Please enter only one valid arugment at a time:\n" +
                    "help: for more info\n" + ">");
                string enteredArgs = Console.ReadLine();
                Program.ExecuteCommand(enteredArgs);
            }
            else
            {
                Program.ExecuteCommand(args[0]);
            }
        }

        private static void ExecuteCommand(string arg)
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
                if(ThisProgamAlreadyRunning())
                {
                    Console.WriteLine("The program is already running. Please stop the previous instance and then run again.\n" + ">");
                    string enteredArg = Console.ReadLine();
                    ExecuteCommand(enteredArg);
                }
                // System monitoring is performed in a seperate thread with lowest priority for maximum responsiveness
                Thread executeThread = new Thread(Execute);
                executeThread.Priority = ThreadPriority.Lowest;
                executeThread.IsBackground = true;
                executeThread.Start();
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

        // Summary:
        // The method that keeps track of the system performance and writes it to a CSV file
        private static void Execute()
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

        // Check if this program instance is already running in the system
        private static bool ThisProgamAlreadyRunning()
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

        // Terminate this program forcefully using command
        private static void TerminateThisProgram()
        {
            String process = Process.GetCurrentProcess().ProcessName;
            Process.Start("cmd.exe", "/c taskkill /F /IM " + process + ".exe /T");
        }

        // Method to write out to a CSV file
        private static void WriteToFile(string message, string fileName)
        {
            try
            {
                string Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                StreamWriter SW = new StreamWriter(Path + "\\" + fileName.Trim() + ".CSV", true);
                SW.WriteLine(message);
                SW.Flush();
                SW.Close();
            }
            catch (Exception ex) { Console.WriteLine("Error writing out to file - " + fileName + ". Exception: " + ex.Message); }
        }

    }
}
