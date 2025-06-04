using System.Numerics;
using Microsoft.AspNetCore.Mvc;
using DTO;
using Services;
using Utils;
using Microsoft.Extensions.Options;

namespace Controllers;

/// <summary>
/// Controller responsável por gerenciar o chat criptografado entre dois terminais
/// Implementa comunicação segura usando criptografia RSA e assinatura digital
/// </summary>
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    #region Propriedades Estáticas
    
    /// <summary>
    /// Dicionário que armazena as chaves públicas dos outros terminais conectados
    /// Key: Nome do terminal (ex: "Bruno", "Felipe")
    /// Value: Tupla com (Módulo N, Expoente E, URL do terminal)
    /// </summary>
    private static Dictionary<string, (BigInteger N, BigInteger E, string Url)> PublicKeys = new();
    
    /// <summary>
    /// Lista que armazena todas as mensagens recebidas pelo terminal atual
    /// Permite visualizar o histórico de conversas
    /// </summary>
    private static List<ReceivedMessageDTO> ReceivedMessages = new();
    
    /// <summary>
    /// Serviço responsável por gerenciar as chaves RSA (pública e privada) do terminal atual
    /// </summary>
    private static RSAKeyService _rsaKeyService;
    
    #endregion

    #region Propriedades de Instância
    
    /// <summary>
    /// Configurações específicas do terminal atual (nome e URL)
    /// Carregadas do arquivo appsettings.{Ambiente}.json
    /// </summary>
    private readonly ChatConfig _chatConfig;
    
    #endregion

    #region Construtor
    
    /// <summary>
    /// Construtor do ChatController
    /// Injeta as dependências necessárias para o funcionamento do chat
    /// </summary>
    /// <param name="rsaKeyService">Serviço de chaves RSA</param>
    /// <param name="chatConfig">Configurações do terminal</param>
    public ChatController(RSAKeyService rsaKeyService, IOptions<ChatConfig> chatConfig)
    {
        _rsaKeyService = rsaKeyService;
        _chatConfig = chatConfig.Value;
    }
    
    #endregion

    #region Endpoints de Conexão

    /// <summary>
    /// Conecta automaticamente com outro terminal realizando o handshake de chaves públicas
    /// Este é o primeiro passo para estabelecer comunicação segura
    /// </summary>
    /// <param name="request">URL do outro terminal para conexão</param>
    /// <returns>Status da conexão</returns>
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
                if (otherKey != null)
                {
                    // Converte as strings para BigInteger e armazena a chave pública
                    BigInteger n = BigInteger.Parse(otherKey.N);
                    BigInteger e = BigInteger.Parse(otherKey.E);
                    PublicKeys[otherKey.Name] = (n, e, otherKey.Url);
                    
                    Console.WriteLine($"🤝 Conectado com {otherKey.Name}!");
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

    /// <summary>
    /// Recebe e processa chaves públicas de outros terminais (usado no handshake)
    /// Chamado automaticamente quando outro terminal tenta se conectar
    /// </summary>
    /// <param name="key">Chave pública do terminal que está se conectando</param>
    /// <returns>Nossa chave pública como resposta</returns>
    [HttpPost("key")]
    public IActionResult ReceivePublicKey([FromBody] PublicKeyDTO key)
    {
        // Converte e armazena a chave pública recebida
        BigInteger n = BigInteger.Parse(key.N);
        BigInteger e = BigInteger.Parse(key.E);
        PublicKeys[key.Name] = (n, e, key.Url);

        Console.WriteLine($"🔑 Chave recebida de {key.Name}: {key.Url}");

        // Retorna nossa chave pública como resposta
        var myKey = new PublicKeyDTO
        {
            Name = _chatConfig.Name,
            N = _rsaKeyService.N.ToString(),
            E = _rsaKeyService.E.ToString(),
            Url = _chatConfig.Url
        };

        return Ok(myKey);
    }

    #endregion

    #region Endpoints de Mensagens

    /// <summary>
    /// Envia uma mensagem criptografada para outro terminal de forma simplificada
    /// A mensagem é criptografada com RSA e assinada digitalmente
    /// </summary>
    /// <param name="message">Dados da mensagem (destinatário e conteúdo)</param>
    /// <returns>Status do envio</returns>
    [HttpPost("send-simple")]
    public async Task<IActionResult> SendSimpleMessage([FromBody] SimpleMessageDTO message)
    {
        // Verifica se o destinatário está conectado (temos sua chave pública)
        if (!PublicKeys.TryGetValue(message.Receiver, out var receiverPublicKey))
            return BadRequest($"❌ {message.Receiver} não está conectado. Use /chat/connect primeiro.");
        
        try
        {
            // PASSO 1: Criptografa a mensagem usando a chave pública do destinatário
            // Apenas o destinatário poderá descriptografar usando sua chave privada
            string encryptedMessage = EncoderRSA.Encrypt(
                message.Message,
                receiverPublicKey.E,
                receiverPublicKey.N
            );

            // PASSO 2: Cria um hash da mensagem original para assinatura digital
            string hash = SHA256Assigner.AssignString(message.Message);

            // PASSO 3: Criptografa o hash usando nossa chave privada (assinatura digital)
            // Isso garante que a mensagem não foi alterada e comprova nossa identidade
            string signature = EncoderRSA.Encrypt(
                hash,
                _rsaKeyService.D,
                _rsaKeyService.N
            );

            // PASSO 4: Prepara o payload com mensagem criptografada e assinatura
            var payload = new MessageDTO
            {
                Sender = _chatConfig.Name,
                EncryptedMessage = encryptedMessage,
                Signature = signature
            };

            // PASSO 5: Envia a mensagem para o terminal destinatário
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync(
                $"{receiverPublicKey.Url}/chat/message",
                payload
            );

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"📤 Mensagem enviada para {message.Receiver}: {message.Message}");
                return Ok($"✅ Mensagem enviada para {message.Receiver}");
            }
                
            return StatusCode((int)response.StatusCode, "❌ Erro ao enviar mensagem");
        }
        catch (Exception ex)
        {
            return BadRequest($"❌ Erro: {ex.Message}");
        }
    }

    /// <summary>
    /// Recebe e processa mensagens criptografadas de outros terminais
    /// Descriptografa a mensagem e valida a assinatura digital
    /// </summary>
    /// <param name="message">Mensagem criptografada recebida</param>
    /// <returns>Status do processamento</returns>
    [HttpPost("message")]
    public IActionResult ReceiveMessage([FromBody] MessageDTO message)
    {
        // Verifica se temos a chave pública do remetente
        if(!PublicKeys.TryGetValue(message.Sender, out var publicKey))
            return BadRequest("Chave pública não encontrada");
        
        try
        {
            // PASSO 1: Descriptografa a mensagem usando nossa chave privada
            string decryptedMessage = EncoderRSA.Decrypt(
                message.EncryptedMessage,
                _rsaKeyService.D,
                _rsaKeyService.N
            );

            // PASSO 2: Descriptografa a assinatura usando a chave pública do remetente
            // Isso nos dá o hash original da mensagem
            string decryptedSignature = EncoderRSA.Decrypt(
                message.Signature,
                publicKey.E,
                publicKey.N
            );

            // PASSO 3: Valida a integridade da mensagem
            // Compara o hash da mensagem descriptografada com o hash da assinatura
            bool isValid = SHA256Assigner.StringAuthenticator(decryptedMessage, decryptedSignature);

            if(!isValid)
                return BadRequest("Mensagem não autenticada");

            // PASSO 4: Armazena a mensagem no histórico
            var receivedMessage = new ReceivedMessageDTO
            {
                Sender = message.Sender,
                Message = decryptedMessage,
                Timestamp = DateTime.Now
            };

            ReceivedMessages.Add(receivedMessage);

            Console.WriteLine($"📥 Nova mensagem de {message.Sender}: {decryptedMessage}");
            
            return Ok("Mensagem recebida");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao processar mensagem: {ex.Message}");
            return BadRequest($"Erro ao processar mensagem: {ex.Message}");
        }
    }

    #endregion

    #region Endpoints de Consulta

    /// <summary>
    /// Lista todas as mensagens recebidas pelo terminal atual
    /// Útil para ver o histórico de conversas
    /// </summary>
    /// <returns>Lista de mensagens ordenadas por data</returns>
    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        return Ok(new
        {
            TotalMessages = ReceivedMessages.Count,
            Messages = ReceivedMessages.OrderBy(m => m.Timestamp).ToList()
        });
    }

    /// <summary>
    /// Lista todas as conexões ativas (terminais conectados)
    /// Mostra quais terminais estão disponíveis para comunicação
    /// </summary>
    /// <returns>Lista de conexões e informações do terminal atual</returns>
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

    #endregion

    #region Endpoints de Limpeza

    /// <summary>
    /// Remove todas as mensagens armazenadas no terminal atual
    /// Útil para limpar o histórico de conversas
    /// </summary>
    /// <returns>Confirmação da limpeza</returns>
    [HttpDelete("messages")]
    public IActionResult ClearMessages()
    {
        ReceivedMessages.Clear();
        Console.WriteLine("🗑️ Mensagens limpas");
        return Ok("Mensagens limpas");
    }

    #endregion
}