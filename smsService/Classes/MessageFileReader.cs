using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsService.Classes
{
    public class MessageFileReader
    {
        public static List<string> ReadPhoneNumbers(string messagePath)
        {
            try
            {
                List<string> phoneNumbers = new List<string>();
                StreamReader sr = new StreamReader(messagePath);

                string line = sr.ReadLine();

                if (line != null && line == "[NUMBERS]")
                {
                    while (line != null)
                    {
                        if (line == "[MESSAGE]")
                        {
                            sr.Close();
                            return phoneNumbers;
                        }
                        if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                        {
                            phoneNumbers.Add(line);
                        }
                        line = sr.ReadLine();
                    }
                }
                sr.Close();
                return phoneNumbers;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string ReadMessage(string messagePath)
        {
            try
            {
                StreamReader sr = new StreamReader(messagePath);
                string line = sr.ReadLine();

                while (line != null)
                {
                    if(line == "[MESSAGE]")
                    {
                        line = sr.ReadToEnd();
                        sr.Close();
                        return line;
                    }
                    line = sr.ReadLine();
                }

                return null;
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
