using System.Numerics;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private static Dictionary<string, (BigInteger N, BigInteger E)> PublicKeys = new();

    [HttpPost("key")]
    public IActionResult ReceivePublicKey([FromBody] PublicKeyDTO key)
    {
        BigInteger n = BigInteger.Parse(key.N);
        BigInteger e = BigInteger.Parse(key.E);

        PublicKeys[key.Name] = (n, e);

        System.Console.WriteLine($"Received public key from {key.Name}: N = {n}, E = {e}");

        return Ok("Chave recebida");
    }

    [HttpPost("message")]
    public IActionResult ReceiveMessage([FromBody] MessageDTO message)
    {
        return Ok("Mensagem recebida");
    }
}