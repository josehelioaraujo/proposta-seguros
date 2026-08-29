namespace PropostaService.Domain.ValueObjects;

public record Cpf
{
    public string Value { get; }

    public Cpf(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());

        if (!IsValid(digits))
            throw new ArgumentException("CPF invalido", nameof(value));

        Value = digits;
    }

    public string Formatted =>
        $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..]}";

    public override string ToString() => Formatted;

    private static bool IsValid(string digits)
    {
        if (digits.Length != 11)             return false;
        if (digits.Distinct().Count() == 1)  return false;

        return ValidateDigit(digits, 9) && ValidateDigit(digits, 10);
    }

    private static bool ValidateDigit(string digits, int position)
    {
        var sum = 0;
        for (var i = 0; i < position; i++)
            sum += int.Parse(digits[i].ToString()) * (position + 1 - i);

        var remainder = sum % 11;
        var digit     = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(digits[position].ToString()) == digit;
    }
}
