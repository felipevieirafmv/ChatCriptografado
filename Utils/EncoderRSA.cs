using System;
using System.Numerics;
using System.Text;

namespace Utils;

public static class EncoderRSA
{
    public static string Encrypt(string text, BigInteger e, BigInteger n)
    {
        StringBuilder encrypted = new StringBuilder();

        foreach (char c in text)
        {
            BigInteger m = c;
            BigInteger ciph = BigInteger.ModPow(m, e, n);
            encrypted.Append(ciph + " ");
        }

        return encrypted.ToString().Trim();
    }

    public static string Decrypt(string input, BigInteger d, BigInteger n)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder result = new StringBuilder();

        foreach (string part in parts)
        {
            BigInteger ciph = BigInteger.Parse(part);
            BigInteger m = BigInteger.ModPow(ciph, d, n);
            result.Append((char)(int)m);
        }

        return result.ToString();
    }

    public static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m;
        BigInteger y = 0, x = 1;

        if (m == 1)
            return 0;

        while (a > 1)
        {
            BigInteger q = a / m;
            BigInteger t = m;

            m = a % m;
            a = t;
            t = y;

            y = x - q * y;
            x = t;
        }

        if (x < 0)
            x += m0;

        return x;
    }

    public static BigInteger GCD(BigInteger a, BigInteger b)
    {
        while (b != 0)
        {
            BigInteger temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static RSAKeyPair GenerateKeys(int bitLength = 512)
    {
        BigInteger p = PrimeNumberGenerator.GeneratePrime(bitLength);
        BigInteger q = PrimeNumberGenerator.GeneratePrime(bitLength);
        BigInteger n = p * q;
        BigInteger totient = (p - 1) * (q - 1);

        BigInteger e = 65537; // Valor padrão comum, rápido e seguro

        if (GCD(e, totient) != 1)
        {
            // Caso raro, escolher outro e
            e = 3;
            while (GCD(e, totient) != 1)
            {
                e += 2;
            }
        }

        BigInteger d = ModInverse(e, totient);

        return new RSAKeyPair
        {
            N = n,
            E = e,
            D = d
        };
    }
}

public class RSAKeyPair
{
    public BigInteger N { get; set; }
    public BigInteger E { get; set; }
    public BigInteger D { get; set; }
}

