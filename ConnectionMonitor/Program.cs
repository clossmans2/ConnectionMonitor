using System.Globalization;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace ConnectionMonitor
{
    public class Program
    {
        private static Socket? _socket;
        private static ConnectionOptions? _options;
        private static StreamWriter? _logFile;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
            // Creat the config builder
            // Make sure it knows to look in the current directory
            // for the appsettings.json file
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(
                    "config.json",
                    optional: false,
                    reloadOnChange: true
                    );

            // Build the configuration
            IConfiguration? config = builder.Build();

            // Create the log file
            _logFile = File.AppendText("connection.log");

            // Bind the ConnectionOptions section to a new instance of ConnectionOptions
            _options = config.GetSection("ConnectionOptions").Get<ConnectionOptions>();

            // Start monitoring
            MonitorConnection();
        }

        /// <summary>
        /// Handler for when Ctrl + C or Ctrl + Break is pressed
        /// Shuts down the socket and closes the log file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void ExitHandler(object? sender, ConsoleCancelEventArgs args)
        {
            // Set the Cancel property to true to prevent the process from terminating.
            args.Cancel = true;
            Console.WriteLine("Ctrl + C keys pressed.  Program is stopping now.");

            // Log the end of the monitoring session
            var programEnd = DateTime.Now;
            var endMessage = $"Connection monitoring ended successfully at {programEnd.ToString("yyyy-MM-dd HH:mm:ss")}" +
                "\n----------------------------------------------------------------------------------------\n\n\n";

            Log(endMessage);

            if (_logFile is null)
            {
                _logFile = File.AppendText("connection.log");
            }

            // Clean up resources
            _logFile?.Close();
            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                _socket?.Close();
            }
            Log("Session exited successfully.");
            // Allow the app to exit
            args.Cancel = false;
        }

        /// <summary>
        /// Attempts to connect to the host and port specified in the ConnectionOptions object
        /// By default, the host is Google's public DNS server and the port is 53
        /// </summary>
        /// <returns>True if the connection was successful, False if there was any kind of connection issue.</returns>
        public static bool AttemptConnection()
        {
            if (_options is null)
                _options = new ConnectionOptions();

            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            bool retVal;
            try
            {
                _socket.Connect(_options.Host, _options.Port);
                _socket.Shutdown(SocketShutdown.Both);
                _socket?.Close();
                retVal = true;
            }
            catch (SocketException ex)
            {
                Log($"An error occurred. {ex.Message} \n");
                retVal = false;
            }
            return retVal;            
        }

        /// <summary>
        /// Writes a message to the console and the log file
        /// </summary>
        /// <param name="logMessage"></param>
        public static void Log(string logMessage)
        {
            Console.WriteLine(logMessage);
            _logFile?.Write(logMessage);
        }

        /// <summary>
        /// Calculates the duration of an outage
        /// </summary>
        /// <param name="timeBackOn">DateTime of when the network connection was restored</param>
        /// <param name="timeOut">DateTime of when the network connection went down</param>
        /// <returns>A TimeSpan object that represents the difference between the time the network connection went down and the time it came back up.</returns>
        public static TimeSpan GetOutageDuration(DateTime timeBackOn, DateTime timeOut) => timeBackOn - timeOut;


        /// <summary>
        /// Monitors the network connection. If the connection is down, it will log the outage and duration.
        /// </summary>
        public static void MonitorConnection()
        {
            Log("\nMonitoring your connection.  Press Ctrl + C to exit the session. \n");
            DateTime monitorStartTime = DateTime.Now;
            string startTime = monitorStartTime.ToString("yyyy-MM-dd HH:mm:ss");

            Log($"----------------------------------------------------------------------------------------\n");
            Log($"Connection monitoring session started at: {startTime}\n");

            while (true)
            {
                if (_options is null)
                    _options = new ConnectionOptions();
                

                if (AttemptConnection())
                {
                    Thread.Sleep(_options.SleepInterval);
                }
                else
                {
                    int totalDownTimeSeconds = 0;
                    DateTime downTimeStart = DateTime.Now;
                    Log("---\n");
                    Log($"Offline as of: {downTimeStart.ToString("yyyy-MM-dd HH:mm:ss")} \n");

                    while (!AttemptConnection())
                    {
                        
                        Thread.Sleep(_options.SleepInterval);
                        totalDownTimeSeconds += _options.SleepInterval;
                    }

                    DateTime backOnlineTime = DateTime.Now;
                    string backOnline = backOnlineTime.ToString("yyyy-MM-dd HH:mm:ss");

                    Log($"Connection restored at: {backOnline} \n");

                    TimeSpan duration = GetOutageDuration(backOnlineTime, downTimeStart);

                    Log($"Duration of outage: {duration.ToString("c", new CultureInfo("en-US"))} \n");
                    Log($"---\n");
                }
            }
        }
    }
}