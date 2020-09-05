using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimTrayFramework
{
    public static class ValidateFileName
    {
        public static bool Validate(string fileName)
        {
            string fileAllUpper = fileName.ToUpper();

            if (
                (fileAllUpper == "CON") || 
                (fileAllUpper == "PRN") ||
                (fileAllUpper == "AUX") ||
                (fileAllUpper== "NUL") ||
                (fileAllUpper == "COM0") ||
                (fileAllUpper == "COM1") ||
                (fileAllUpper == "COM2") ||
                (fileAllUpper == "COM3") ||
                (fileAllUpper == "COM4") ||
                (fileAllUpper == "COM5") ||
                (fileAllUpper == "COM6") ||
                (fileAllUpper == "COM7") ||
                (fileAllUpper == "COM8") ||
                (fileAllUpper == "COM9") ||
                (fileAllUpper == "LPT0") ||
                (fileAllUpper == "LPT1") ||
                (fileAllUpper == "LPT2") ||
                (fileAllUpper == "LPT3") ||
                (fileAllUpper == "LPT4") ||
                (fileAllUpper == "LPT5") ||
                (fileAllUpper == "LPT6") ||
                (fileAllUpper == "LPT7") ||
                (fileAllUpper == "LPT8") ||
                (fileAllUpper == "LPT9")
               )
            {
                return false;
            }

            if(
                (fileName.Contains("?")) ||
                (fileName.Contains("\\")) ||
                (fileName.Contains("/")) ||
                (fileName.Contains("<")) ||
                (fileName.Contains(">")) ||
                (fileName.Contains(":")) ||
                (fileName.Contains("|")) ||
                (fileName.Contains("\"")) ||
                (fileName.Contains("*"))
              )
            {
                return false;
            }

            foreach (Char c in fileName.ToCharArray())
            {
                if ((UInt16)c < 32)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
