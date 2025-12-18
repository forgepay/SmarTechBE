using MicPic.Infrastructure.Extensions;
using Microsoft.Extensions.Primitives;
using System.Globalization;
using System.Numerics;

namespace MicPic.Infrastructure.Helpers;

#pragma warning disable CA1062 // Validate arguments of public methods

public static class BigIntegerHelper
{
    public static bool TryParseNumber(string input, out BigInteger value)
        => BigInteger.TryParse(input, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out value);

    public static bool TryParseHexNumber(string input, out BigInteger value)
        => BigInteger.TryParse(String.Concat("0", input.Chop0x()), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out value);

    public static BigInteger ParseHexNumber(string input)
        => BigInteger.Parse(String.Concat("0", input.Chop0x()), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo);

    public static BigInteger Sum(IEnumerable<BigInteger> values)
    {
        var result = BigInteger.Zero;

        foreach(var value in values)
            result += value;

        return result;
    }
    
    public static BigInteger Max(IEnumerable<BigInteger> values)
    {
        var list = values
            .ToList();
        
        var result = list
            .First();

        foreach (var value in list)
            if (result < value)
                result = value;

        return result;
    }
    
    public static async Task<BigInteger> MultiplyAsync(params Task<BigInteger>[] tasks)
    {
        var result = BigInteger.One;
        
        var values = await Task.WhenAll(tasks);
        
        foreach(var value in values)
            result = BigInteger.Multiply(result, value);
        
        return result;
    }

    public static bool TryConvertToBigInteger(string amount, int decimals, out BigInteger result)
    {
        result = BigInteger.Zero;

        var input = new StringSegment(amount);

        while (input.Length > 0)
        {
            if (input[0] == '.')
            {
                input = new StringSegment(input.Buffer!, input.Offset + 1, input.Length - 1);
                break;
            }

            if ('0' <= input[0] && input[0] <= '9')
            {
                result *= 10;
                result += BigInteger.Parse(input.AsSpan(0, 1), NumberStyles.Any, CultureInfo.InvariantCulture);
                input = new StringSegment(input.Buffer!, input.Offset + 1, input.Length - 1);
                continue;
            }

            return false;
        }

        for(var i = 0; i < decimals; i++)
        {
            result *= 10;
            
            if (input.Length > 0)
            {
                if ('0' <= input[0] && input[0] <= '9')
                {
                    result += BigInteger.Parse(input.AsSpan(0, 1), NumberStyles.Any, CultureInfo.InvariantCulture);
                    input = new StringSegment(input.Buffer!, input.Offset + 1, input.Length - 1);
                    continue;
                }
                
                return false;
            }
        }

        return true;
    }    
}