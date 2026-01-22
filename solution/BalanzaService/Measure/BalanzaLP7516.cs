using System.Text.RegularExpressions;

namespace BalanzaService.Measure;

public class BalanzaLP7516 : IBalanza
{
    public string Nombre => "LP7516";

    public bool PuedeParsesr(string data)
    {
        return data.StartsWith("ST,") || data.StartsWith("US,") || data.StartsWith("OL,");
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            var match = Regex.Match(data, @"(\w+),(\w+),([+-])\s*([\d.]+)(\w+)");

            if (match.Success)
            {
                string signo = match.Groups[3].Value;
                string pesoStr = match.Groups[4].Value;

                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double peso))
                {
                    return signo == "-" ? -peso : peso;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
