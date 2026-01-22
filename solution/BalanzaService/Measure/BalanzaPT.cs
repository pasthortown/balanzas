using System.Text.RegularExpressions;

namespace BalanzaService.Measure;

/// <summary>
/// Parser para balanzas en modo Print (PT).
/// Formato: [STX]     1.30 kg PT
/// </summary>
public class BalanzaPT : IBalanza
{
    public string Nombre => "PT";

    public bool PuedeParsesr(string data)
    {
        return data.EndsWith("PT") && data.Contains("kg");
    }

    public double? ParsearPeso(string data)
    {
        try
        {
            // Limpiar caracteres de control (STX = 0x02, etc.)
            var cleanData = new string(data.Where(c => !char.IsControl(c)).ToArray()).Trim();

            // Formato esperado: "1.30 kg PT" o similar
            // Regex para capturar el peso antes de "kg"
            var match = Regex.Match(cleanData, @"([\d.]+)\s*kg\s*PT$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string pesoStr = match.Groups[1].Value;

                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double peso))
                {
                    return peso;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
