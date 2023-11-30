using System.Text.RegularExpressions;

namespace ICS.Calculate.API.Util;

public static class Utilities
{
    public static bool CheckEmail(string email)
    {
        return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
    }

    public static int ConvertDeadlineInDays(string start, string deadline)
    {
        int days = (Convert.ToDateTime(deadline) - Convert.ToDateTime(start)).Days;
        return days;
    }

    public static dynamic NumberConvert(string number)
    {

        number = number.Trim().Replace("- ", "-");
        double convertedNumber = 0.0;

        //Valida números apenas com vírgula 11111111111111,11
        if (Regex.IsMatch(number, @"^[+-]?\d*([,]\d*)?$"))
        {
            convertedNumber = Convert.ToDouble(number);
        }
        //Valida números apenas com ponto 11111111111111.11
        else if (Regex.IsMatch(number, @"^[+-]?\d*([.]\d*)?$"))
        {
            convertedNumber = Convert.ToDouble(number.Replace(".", ","));
        }
        //Valida Padrão Brasileiro 1.111.111,11 || 1.111,11111 || 11,11 || 1,11
        else if (Regex.IsMatch(number, @"^[+-]?(?:\d{0,3}(?:\.\d{3})*|0)(,\d*)?$"))
        {
            convertedNumber = Convert.ToDouble(number.Replace(".", ""));
        }
        //Valida Padrão Americano 1,111,111.11 || 1,111.11111 || 11.11 || 1.11
        else if (Regex.IsMatch(number, @"^[+-]?(?:\d{0,3}(?:\,\d{3})*|0)(\.\d*)?$"))
        {
            convertedNumber = Convert.ToDouble(number.Replace(",", "").Replace(".", ","));
        }
        else
        {
            return -999999999;
        }

        return convertedNumber;
    }
}
