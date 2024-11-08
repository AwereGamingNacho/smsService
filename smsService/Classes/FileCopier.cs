using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsService.Classes
{
    public class FileCopier
    {
        public static bool CopyFileWithUniqueName(string source, string destination)
        {
            try
            {
                if (!File.Exists(source)) return false;

                if (!Directory.Exists(destination)) return false;

                string fileName = Path.GetFileNameWithoutExtension(source);
                string fileExtension = Path.GetExtension(source);
                string destinationFilePath = Path.Combine(destination, Path.GetFileName(source));

                int copyNumber = 1;

                while (File.Exists(destinationFilePath))
                {
                    destinationFilePath = Path.Combine(destination, $"{fileName} ({copyNumber}){fileExtension}");
                    copyNumber++;
                }

                File.Copy(source, destinationFilePath);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
