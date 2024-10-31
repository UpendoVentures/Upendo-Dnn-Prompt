using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upendo.Modules.UpendoPrompt.Utility
{
    public class PasswordGenerator
    {
        private const string upperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string numbers = "0123456789";
        private const string symbols = "!*";
        private const string GenericPasswordValue = "UpendoRocks!";

        public static string GeneratePassword(int length)
        {
            if (length >= 7 && length <= 12)
            {
                return GenericPasswordValue;
            }
            else
            {
                string allCharacters = upperCaseLetters + lowerCaseLetters + numbers + symbols;
                Random random = new Random();

                StringBuilder password = new StringBuilder();
                password.Append(upperCaseLetters[random.Next(upperCaseLetters.Length)]);
                password.Append(lowerCaseLetters[random.Next(lowerCaseLetters.Length)]);
                password.Append(numbers[random.Next(numbers.Length)]);
                password.Append(symbols[random.Next(symbols.Length)]);

                for (int i = 0; i < length; i++)
                {
                    password.Append(allCharacters[random.Next(allCharacters.Length)]);
                }

                return ShuffleString(password.ToString());
            }
        }

        private static string ShuffleString(string input)
        {
            char[] array = input.ToCharArray();
            Random random = new Random();
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }
    }
}
