using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace smsService.Classes
{
    public class SmsSender
    {
        private string modemIp = "192.168.36.2";
        private int modemPort = 23;
        private string username = "admin";
        private string password = "12345";

        public SmsSender() { }

        public SmsSender(string modemIp, int modemPort, string username, string password)
        {
            this.modemIp = modemIp;
            this.modemPort = modemPort;
            this.username = username;
            this.password = password;
        }

        public bool sendSms(string recipientNumber, string message)
        {
            try
            {

                using (TcpClient client = new TcpClient(modemIp, modemPort))
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine(response);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine(response);

                    byte[] usernameBytes = Encoding.ASCII.GetBytes(username + "\r");
                    stream.Write(usernameBytes, 0, usernameBytes.Length);
                    Thread.Sleep(600);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine(response);

                    byte[] passwordBytes = Encoding.ASCII.GetBytes(password + "\r");
                    stream.Write(passwordBytes, 0, passwordBytes.Length);
                    Thread.Sleep(600);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine(response);

                    //Console.WriteLine($"Sending SMS to number: {recipientNumber}\n");

                    string smsCommand = $"AT+MMMGS={recipientNumber}\r";
                    byte[] smsCommandBytes = Encoding.ASCII.GetBytes(smsCommand);
                    stream.Write(smsCommandBytes, 0, smsCommandBytes.Length);
                    Thread.Sleep(600);

                    byte[] messageBytes = Encoding.ASCII.GetBytes(message + "\r");
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    Thread.Sleep(600);

                    byte[] ctrlZ = Encoding.ASCII.GetBytes("\x1A");
                    stream.Write(ctrlZ, 0, ctrlZ.Length);
                    Thread.Sleep(600);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //Console.WriteLine(response);
                }

                Thread.Sleep(700);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
