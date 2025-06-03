using System.ComponentModel;
using System.IO.Pipelines;
using System.IO;
using System.Net;
using System.Text;

namespace Utils;

public class SHA256Assigner
{
    private static uint[] h =
    [
        0b01101010000010011110011001100111,
        0b10111011011001111010111010000101,
        0b00111100011011101111001101110010,
        0b10100101010011111111010100111010,
        0b01010001000011100101001001111111,
        0b10011011000001010110100010001100,
        0b00011111100000111101100110101011,
        0b01011011111000001100110100011001
    ]; 

    private static readonly uint[] k = 
    [
        0b01000010100010100010111110011000,
        0b01110001001101110100010010010001,
        0b10110101110000001111101111001111,
        0b11101001101101011101101110100101,
        0b00111001010101101100001001011011,
        0b01011001111100010001000111110001,
        0b10010010001111111000001010100100,
        0b10101011000111000101111011010101,
        0b11011000000001111010101010011000,
        0b00010010100000110101101100000001,

        0b00100100001100011000010110111110,
        0b01010101000011000111110111000011,
        0b01110010101111100101110101110100,
        0b10000000110111101011000111111110,
        0b10011011110111000000011010100111,
        0b11000001100110111111000101110100,
        0b11100100100110110110100111000001,
        0b11101111101111100100011110000110,
        0b00001111110000011001110111000110,
        0b00100100000011001010000111001100,

        0b00101101111010010010110001101111,
        0b01001010011101001000010010101010,
        0b01011100101100001010100111011100,
        0b01110110111110011000100011011010,
        0b10011000001111100101000101010010,
        0b10101000001100011100011001101101,
        0b10110000000000110010011111001000,
        0b10111111010110010111111111000111,
        0b11000110111000000000101111110011,
        0b11010101101001111001000101000111,

        0b00000110110010100110001101010001,
        0b00010100001010010010100101100111,
        0b00100111101101110000101010000101,
        0b00101110000110110010000100111000,
        0b01001101001011000110110111111100,
        0b01010011001110000000110100010011,
        0b01100101000010100111001101010100,
        0b01110110011010100000101010111011,
        0b10000001110000101100100100101110,
        0b10010010011100100010110010000101,

        0b10100010101111111110100010100001,
        0b10101000000110100110011001001011,
        0b11000010010010111000101101110000,
        0b11000111011011000101000110100011,
        0b11010001100100101110100000011001,
        0b11010110100110010000011000100100,
        0b11110100000011100011010110000101,
        0b00010000011010101010000001110000,
        0b00011001101001001100000100010110,
        0b00011110001101110110110000001000,

        0b00100111010010000111011101001100,
        0b00110100101100001011110010110101,
        0b00111001000111000000110010110011,
        0b01001110110110001010101001001010,
        0b01011011100111001100101001001111,
        0b01101000001011100110111111110011,
        0b01110100100011111000001011101110,
        0b01111000101001010110001101101111,
        0b10000100110010000111100000010100,
        0b10001100110001110000001000001000,

        0b10010000101111101111111111111010,
        0b10100100010100000110110011101011,
        0b10111110111110011010001111110111,
        0b11000110011100010111100011110010
    ];

    private static uint rotateRight(uint value, int count)
    {
        return (value >> count) | (value << (32 - count));
    }

    private static uint[] generateChunk(string input, bool isFile = false)
    {
        byte[] bytes;
        if(isFile)
            bytes = File.ReadAllBytes(input);
        else
            bytes = Encoding.UTF8.GetBytes(input);

        string resultString = "";
        foreach (var item in bytes)
        {
            resultString += Convert.ToString(item, 2).PadLeft(8, '0');
        }
        resultString += '1';
        int size = resultString.Length - 1;
        int numberChunks = (size+64)/512 + 1;
        resultString = resultString.PadRight(numberChunks*512 - 64, '0');
        resultString += Convert.ToString(size, 2).PadLeft(64, '0');

        int linesQuant = resultString.Length/32;
        uint[] resultMatrix = new uint[linesQuant];

        for(int i=0; i<linesQuant; i++)
        {
            string oneByte = resultString.Substring(i*32, 32);
            resultMatrix[i] = Convert.ToUInt32(oneByte, 2);
        }

        return resultMatrix;
    }

    private static uint[] calculateW(uint[] chunk)
    {
        uint[] allChunks = new uint[64];
        Array.Copy(chunk, allChunks, 16);

        for(int i = 0; i<48; i++)
        {
            uint w1 = allChunks[i+1];
            uint sigma0temp1 = rotateRight(w1, 7);
            uint sigma0temp2 = rotateRight(w1, 18);
            uint sigma0temp3 = w1 >> 3;
            uint sigma0 = sigma0temp1 ^ sigma0temp2 ^ sigma0temp3;
            
            uint w14 = allChunks[i+14];
            uint sigma1temp1 = rotateRight(w14, 17);
            uint sigma1temp2 = rotateRight(w14, 19);
            uint sigma1temp3 = w14 >> 10;
            uint sigma1 = sigma1temp1 ^ sigma1temp2 ^ sigma1temp3;

            allChunks[i+16] = allChunks[i] + sigma0 + allChunks[i+9] + sigma1;
        }

        return allChunks;
    }

    private static void calculateResult(uint[] w)
    {
        uint a = h[0];
        uint b = h[1];
        uint c = h[2];
        uint d = h[3];
        uint e = h[4];
        uint f = h[5];
        uint g = h[6];
        uint hResult = h[7];

        int size = w.Length;

        for(int i=0; i<size; i++)
        {
            uint sigma1aux1 = rotateRight(e, 6);
            uint sigma1aux2 = rotateRight(e, 11);
            uint sigma1aux3 = rotateRight(e, 25);
            uint sigma1 = sigma1aux1 ^ sigma1aux2 ^ sigma1aux3;

            uint choice = (e & f) ^ ((~e) & g);

            uint temp1 = hResult + sigma1 + choice + k[i] + w[i];

            uint sigma0aux1 = rotateRight(a, 2);
            uint sigma0aux2 = rotateRight(a, 13);
            uint sigma0aux3 = rotateRight(a, 22);
            uint sigma0 = sigma0aux1 ^ sigma0aux2 ^ sigma0aux3;

            uint majority = (a & b) ^ (a & c) ^ (b & c);

            uint temp2 = sigma0 + majority;

            hResult = g;
            g = f;
            f = e;
            e = d + temp1;
            d = c;
            c = b;
            b = a;
            a = temp1 + temp2;
        }

        h[0] += a;
        h[1] += b;
        h[2] += c;
        h[3] += d;
        h[4] += e;
        h[5] += f;
        h[6] += g;
        h[7] += hResult;
    }

    public static string AssignString(string input)
    {
        uint[] chunk = generateChunk(input);

        for(int i=0; i<chunk.Length; i+=16)
        {
            var subArray = new Span<uint>(chunk, i, 16).ToArray();
            uint[] w = calculateW(subArray);
            calculateResult(w);
        }

        string result = "";

        result += h[0].ToString("x8");
        result += h[1].ToString("x8");
        result += h[2].ToString("x8");
        result += h[3].ToString("x8");
        result += h[4].ToString("x8");
        result += h[5].ToString("x8");
        result += h[6].ToString("x8");
        result += h[7].ToString("x8");

        return result;
    }

    public static string AssignFile(string path)
    {
        uint[] chunk = generateChunk(path, true);

        for(int i=0; i<chunk.Length; i+=16)
        {
            var subArray = new Span<uint>(chunk, i, 16).ToArray();
            uint[] w = calculateW(subArray);
            calculateResult(w);
        }

        string result = "";

        result += h[0].ToString("x8");
        result += h[1].ToString("x8");
        result += h[2].ToString("x8");
        result += h[3].ToString("x8");
        result += h[4].ToString("x8");
        result += h[5].ToString("x8");
        result += h[6].ToString("x8");
        result += h[7].ToString("x8");

        return result;
    }

    public static bool Authenticator(string path, string assignature)
    {
        string currAssignature = AssignFile(path);
        return currAssignature == assignature;
    }
}
