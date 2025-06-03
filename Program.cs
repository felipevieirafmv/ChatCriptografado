using Utils;

var keys = EncoderRSA.GenerateKeys();

string message = "teste";

var encrypted = EncoderRSA.Encrypt(message, keys.E, keys.N);
Console.WriteLine("Encrypted: " + encrypted);

var decrypted = EncoderRSA.Decrypt(encrypted, keys.D, keys.N);
Console.WriteLine("Decrypted: " + decrypted);