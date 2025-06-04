using System.Numerics;
using Utils;

namespace Services;

public class RSAKeyService
{
    public BigInteger N { get; }
    public BigInteger E { get; }
    public BigInteger D { get; }

    public RSAKeyService()
    {
        var keyPair = EncoderRSA.GenerateKeys(512); // Tamanho ajustável
        N = keyPair.N;
        E = keyPair.E;
        D = keyPair.D;

        Console.WriteLine($"Minhas chaves geradas: N = {N}, E = {E}, D = {D}");
    }
}