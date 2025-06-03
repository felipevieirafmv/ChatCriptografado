using System.Numerics;

namespace Utils;

public static class PrimeNumberGenerator
{
    public static BigInteger GenerateRandomBigInteger(int bitLength)
    {
        int byteLength = (bitLength + 7) / 8;
        byte[] bytes = new byte[byteLength];
        Random rng = new Random();
        rng.NextBytes(bytes);
        bytes[^1] &= (byte)((1 << (bitLength % 8)) - 1);
        bytes[^1] |= (byte)((1 << (bitLength % 8)) - 1);

        return new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
    }

    public static bool IsProbablyPrime(BigInteger n, int k = 10)
    {
        if (n == 2 || n == 3)
            return true;
        if (n < 2 || n % 2 == 0)
            return false;

        BigInteger d = n - 1;
        int r = 0;
        while (d % 2 == 0)
        {
            d /= 2;
            r += 1;
        }

        Random rng = new Random();
        byte[] bytes = new byte[n.ToByteArray().LongLength];

        for (int i = 0; i < k; i++)
        {
            BigInteger a;
            do
            {
                rng.NextBytes(bytes);
                a = new BigInteger(bytes);
            } while (a < 2 || a >= n - 2);

            BigInteger x = BigInteger.ModPow(a, d, n);
            if (x == 1 || x == n - 1)
                continue;

            bool continueOuter = false;
            for (int j = 0; j < r - 1; j++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1)
                {
                    continueOuter = true;
                    break;
                }
            }

            if (continueOuter)
                continue;

            return false;
        }

        return true;
    }

    public static BigInteger GeneratePrime(int bitLength)
    {
        while (true)
        {
            BigInteger candidate = GenerateRandomBigInteger(bitLength);
            candidate |= 1;

            if (IsProbablyPrime(candidate))
                return candidate;
        }
    }
}