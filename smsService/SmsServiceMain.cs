using smsService.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace smsService
{
    public partial class SmsServiceMain : ServiceBase
    {
        private const string _incomingDirectory = @"C:\smsService\Incoming";
        private const string _workingDirectory = @"C:\smsService\Working";
        private const string _errorDirectory = @"C:\smsService\Error";
        private const string _processedDirectory = @"C:\smsService\Processed";
        private const string _fileName = @"message.txt";

        private bool _processing = false;

        private EventLog _eventLog;
        private Timer _systemTimer;
        private Timer _messageTimer;
        public SmsServiceMain()
        {
            InitializeComponent();
            _eventLog = new EventLog();
            if (!EventLog.SourceExists("smsEventSource"))
            {
                EventLog.CreateEventSource("smsEventSource", "smsLogs");
            }
            _eventLog.Source = "smsEventSource";
            _eventLog.Log = "smsLogs";
        }

        protected override void OnStart(string[] args)
        {
            _eventLog.WriteEntry("SMS Service Started");

            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            _systemTimer = new Timer
            {
                Interval = 60000
            };
            _systemTimer.Elapsed += new ElapsedEventHandler(this.OnSystemCheck);
            _systemTimer.Start();

            _messageTimer = new Timer
            {
                Interval = 7000
            };
            _messageTimer.Elapsed += new ElapsedEventHandler(this.OnMessageCheck);
            _messageTimer.Start();
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            _systemTimer?.Stop();
            _messageTimer?.Stop();
            _eventLog.WriteEntry("SMS service stopped");
        }

        protected override void OnContinue()
        {
            _eventLog.WriteEntry("SMS service unpaused (continued).");
        }

        protected override void OnPause()
        {
            _eventLog.WriteEntry("SMS service paused.");
        }

        protected void OnSystemCheck(object sender, ElapsedEventArgs args)
        {
            _eventLog.WriteEntry("SMS service checked\nNo problems found.");
        }

        protected void OnMessageCheck(object sender, ElapsedEventArgs args)
        {
            try
            {
                if (_processing)
                {
                    _eventLog.WriteEntry($"Cannot process message: There is already a message processing.");
                    return;
                }

                _eventLog.WriteEntry("Checking for incoming message...");

                if (!Directory.Exists(_incomingDirectory))
                {
                    _eventLog.WriteEntry($"ERROR: {_incomingDirectory}\nDirectory not detected.\nMissing directory was created.");
                    Directory.CreateDirectory(_incomingDirectory);
                }
                if (!Directory.Exists(_workingDirectory))
                {
                    _eventLog.WriteEntry($"ERROR: {_workingDirectory}\nDirectory not detected.\nMissing directory was created.");
                    Directory.CreateDirectory(_workingDirectory);
                }
                if (!Directory.Exists(_errorDirectory))
                {
                    _eventLog.WriteEntry($"ERROR: {_workingDirectory}\nDirectory not detected.\nMissing directory was created.");
                    Directory.CreateDirectory(_errorDirectory);
                }
                if (!Directory.Exists(_processedDirectory))
                {
                    _eventLog.WriteEntry($"WARNING: {_processedDirectory}\nDirectory not detected.\nMissing directory was created.");
                    Directory.CreateDirectory(_processedDirectory);
                }
                if (!File.Exists(Path.Combine(_incomingDirectory, _fileName)))
                {
                    _eventLog.WriteEntry($"WARNING: message.txt not detected.");
                    return;
                }

                _eventLog.WriteEntry($"SUCCESS: message.txt was detected.");

                try
                {
                    _processing = true;

                    File.Move(Path.Combine(_incomingDirectory, _fileName), Path.Combine(_workingDirectory, _fileName));

                    _eventLog.WriteEntry("Starting to send SMS...");

                    OnSendSms();
                }
                catch (Exception ex)
                {
                    _eventLog.WriteEntry($"ERROR: {ex.Message}");
                }

                _processing = false;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private void OnSendSms()
        {
            List<string> phoneNumbers = MessageFileReader.ReadPhoneNumbers(Path.Combine(_workingDirectory,_fileName));
            string message = MessageFileReader.ReadMessage(Path.Combine(_workingDirectory, _fileName));

            try
            {
                bool result =  sendSms(phoneNumbers, message);

                if (result)
                {
                    FileCopier.CopyFileWithUniqueName(Path.Combine(_workingDirectory, _fileName), _processedDirectory);
                    File.Delete(Path.Combine(_workingDirectory, _fileName));
                }
                else
                {
                    FileCopier.CopyFileWithUniqueName(Path.Combine(_workingDirectory, _fileName), _errorDirectory);
                    File.Delete(Path.Combine(_workingDirectory, _fileName));
                }
            }
            catch(Exception ex)
            {
                _eventLog.WriteEntry($"ERROR: {ex.Message}");
            }
        }

        private bool sendSms(List<string> phoneNumbers, string message)
        {
            SmsSender sender = new SmsSender();

            foreach(string number in phoneNumbers)
            {
                if (PhoneValidator.isValidPhoneNumer(number))
                {
                    bool result = sender.sendSms(number, message);

                    if (!result) return false;

                    _eventLog.WriteEntry($"Successfully sent SMS to {number} with message {message}");
                }
            }
            _eventLog.WriteEntry("Finished sending SMS process.");
            return true;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
    }
}
