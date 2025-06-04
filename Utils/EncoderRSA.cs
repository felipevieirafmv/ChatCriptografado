using System;
using System.Numerics;
using System.Text;

namespace Utils;

public static class EncoderRSA
{
    // Criptografa texto usando a chave pública RSA (e, n).
    public static string Encrypt(string text, BigInteger e, BigInteger n)
    {
        StringBuilder encrypted = new StringBuilder();

        foreach (char c in text)
        {
            BigInteger m = c; // Caractere para número
            BigInteger ciph = BigInteger.ModPow(m, e, n); // Criptografia: c = m^e mod n
            encrypted.Append(ciph + " "); // Adiciona ao resultado
        }

        return encrypted.ToString().Trim();
    }

    // Descriptografa texto usando a chave privada RSA (d, n).
    public static string Decrypt(string input, BigInteger d, BigInteger n)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder result = new StringBuilder();

        foreach (string part in parts)
        {
            BigInteger ciph = BigInteger.Parse(part); // Número criptografado
            BigInteger m = BigInteger.ModPow(ciph, d, n); // Descriptografia: m = c^d mod n
            result.Append((char)(int)m); // Adiciona caractere original
        }

        return result.ToString();
    }

    // Calcula o inverso modular de 'a' modulo 'm' (a^-1 mod m). Essencial para 'd'.
    public static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m;
        BigInteger y = 0, x = 1;

        if (m == 1) return 0;

        // Algoritmo Estendido de Euclides
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

        if (x < 0) x += m0; // Garante resultado positivo

        return x;
    }

    // Calcula o Máximo Divisor Comum (MDC) de dois números.
    public static BigInteger GCD(BigInteger a, BigInteger b)
    {
        // Algoritmo de Euclides
        while (b != 0)
        {
            BigInteger temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    // Gera um par de chaves RSA (pública e privada).
    // bitLength: Tamanho dos primos em bits.
    public static RSAKeyPair GenerateKeys(int bitLength = 512)
    {
        // Geração de primos p e q (assume PrimeNumberGenerator existente)
        BigInteger p = PrimeNumberGenerator.GeneratePrime(bitLength);
        BigInteger q = PrimeNumberGenerator.GeneratePrime(bitLength);
        
        BigInteger n = p * q; // Módulo N
        BigInteger totient = (p - 1) * (q - 1); // Função Totiente de Euler

        BigInteger e = 65537; // Expoente público comum

        // Garante que 'e' e 'totient' são coprimos
        if (GCD(e, totient) != 1)
        {
            e = 3;
            while (GCD(e, totient) != 1)
            {
                e += 2;
            }
        }

        BigInteger d = ModInverse(e, totient); // Expoente privado

        return new RSAKeyPair
        {
            N = n,
            E = e,
            D = d
        };
    }
}

// Estrutura para armazenar as chaves RSA (N, E, D).
public class RSAKeyPair
{
    public BigInteger N { get; set; } // Módulo
    public BigInteger E { get; set; } // Expoente Público
    public BigInteger D { get; set; } // Expoente Privado
}
