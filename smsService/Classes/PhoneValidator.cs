using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace smsService.Classes
{
    public class PhoneValidator
    {
        public static bool isValidPhoneNumer(string number)
        {
            if(string.IsNullOrEmpty(number) || string.IsNullOrWhiteSpace(number)) return false;

            string pattern = @"^\+?(\d{1,3})?[-.\s]?(\d{3})[-.\s]?(\d{3})[-.\s]?(\d{4})$";
            Regex regex = new Regex(pattern);

            if(!regex.IsMatch(number)) return false;

            string numericNumber = Regex.Replace(number, @"\D", "");

            if(isRepetitiveOrSequential(numericNumber)) return false;

            return true;
        }

        private static bool isRepetitiveOrSequential(string number)
        {
            if (new string(number[0], number.Length) == number) return true;

            bool bSequential = true;
            bool bReverseSequential = true;

            for(int i = 1; i < number.Length; i++)
            {
                if (number[i] != number[i - 1] + 1) bSequential = false;
                if (number[i] != number[i - 1] - 1) bReverseSequential = false;
            }

            return bSequential || bReverseSequential;
        }
    }
}
