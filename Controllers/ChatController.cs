using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using DTO;
using Services;
using Utils;
using Microsoft.Extensions.Options;

namespace Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private static Dictionary<string, (BigInteger N, BigInteger E, string Url)> PublicKeys = new();
    private static List<ReceivedMessageDTO> ReceivedMessages = new();
    private static RSAKeyService _rsaKeyService;
    private readonly ChatConfig _chatConfig;

    public ChatController(RSAKeyService rsaKeyService, IOptions<ChatConfig> chatConfig)
    {
        _rsaKeyService = rsaKeyService;
        _chatConfig = chatConfig.Value;
    }

    //Realiza o handshake
    [HttpPost("connect")]
    public async Task<IActionResult> ConnectToOther([FromBody] ConnectRequestDTO request)
    {
        try
        {
            // Prepara nossa chave pública para enviar ao outro terminal
            var myKey = new PublicKeyDTO
            {
                Name = _chatConfig.Name,
                N = _rsaKeyService.N.ToString(),
                E = _rsaKeyService.E.ToString(),
                Url = _chatConfig.Url
            };

            // Envia nossa chave pública para o outro terminal e recebe a dele
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync($"{request.OtherUrl}/chat/key", myKey);

            if (response.IsSuccessStatusCode)
            {
                // Processa a resposta contendo a chave pública do outro terminal
                var otherKey = await response.Content.ReadFromJsonAsync<PublicKeyDTO>();
                if (otherKey is not null)
                {
                    // Converte as strings para BigInteger e armazena a chave pública
                    BigInteger n = BigInteger.Parse(otherKey.N);
                    BigInteger e = BigInteger.Parse(otherKey.E);
                    PublicKeys[otherKey.Name] = (n, e, otherKey.Url);
                    
                    Console.WriteLine($"Conectado com {otherKey.Name}!");
                    return Ok($"Conectado com {otherKey.Name} em {otherKey.Url}");
                }
            }
            
            return BadRequest("Falha ao conectar");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }

    //Envia mensagens
    [HttpPost("send-simple")]
    public async Task<IActionResult> SendSimpleMessage([FromBody] SimpleMessageDTO message)
    {
        // Verifica se o destinatário está conectado (temos sua chave pública)
        if (!PublicKeys.TryGetValue(message.Receiver, out var receiverPublicKey))
            return BadRequest($"{message.Receiver} não está conectado. Use /chat/connect primeiro.");
        
        try
        {
            //Criptografa a mensagem usando a chave pública do destinatário
            string encryptedMessage = EncoderRSA.Encrypt(
                message.Message,
                receiverPublicKey.E,
                receiverPublicKey.N
            );

            string hash = SHA256Assigner.AssignString(message.Message);

            //Criptografa a assinatura usando a propria chave privada, fazendo com que apenas esse terminal
            //possa assinar.
            string signature = EncoderRSA.Encrypt(
                hash,
                _rsaKeyService.D,
                _rsaKeyService.N
            );

            //Monta o payload
            var payload = new MessageDTO
            {
                Sender = _chatConfig.Name,
                EncryptedMessage = encryptedMessage,
                Signature = signature
            };

            //Envia a mensagem
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{receiverPublicKey.Url}/chat/message",
                payload
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Mensagem enviada para {message.Receiver}: {message.Message}");
                return Ok($"Mensagem enviada para {message.Receiver}");
            }
                
            return StatusCode((int)response.StatusCode, "Erro ao enviar mensagem");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }

    //Pega as mensagens
    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        return Ok(new
        {
            TotalMessages = ReceivedMessages.Count,
            Messages = ReceivedMessages.OrderBy(m => m.Timestamp).ToList()
        });
    }

    //Lista todas as conexoes
    [HttpGet("connections")]
    public IActionResult GetConnections()
    {
        var connections = PublicKeys.Select(kv => new
        {
            Name = kv.Key,
            Url = kv.Value.Url
        }).ToList();

        return Ok(new
        {
            MyName = _chatConfig.Name,
            MyUrl = _chatConfig.Url,
            ConnectedTo = connections
        });
    }
}
