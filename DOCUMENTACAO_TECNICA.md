# 📚 Documentação Técnica - Chat Criptografado

## 🔒 Como Funciona a Criptografia RSA

### Visão Geral
O sistema implementa comunicação segura entre dois terminais usando **criptografia RSA** com **assinatura digital SHA256**.

---

## 🔑 Gerenciamento de Chaves

### Geração de Chaves
Cada terminal possui:
- **Chave Pública**: (N, E) - Compartilhada com outros terminais
- **Chave Privada**: (N, D) - Mantida em segredo

### Handshake (Troca de Chaves)
```
Terminal A → Terminal B: POST /chat/key
{
  "Name": "Bruno",
  "N": "123456789...",
  "E": "65537",
  "Url": "http://localhost:5001"
}

Terminal B → Terminal A: Resposta
{
  "Name": "Felipe", 
  "N": "987654321...",
  "E": "65537",
  "Url": "http://localhost:5000"
}
```

---

## 📤 Processo de Envio de Mensagem

### 1. Preparação da Mensagem
```csharp
// Mensagem original
string message = "Olá Felipe!";
```

### 2. Criptografia da Mensagem
```csharp
// Criptografa usando a chave PÚBLICA do destinatário
string encryptedMessage = EncoderRSA.Encrypt(
    message,                    // Mensagem original
    destinatarioChavePublica.E, // Expoente público do destinatário
    destinatarioChavePublica.N  // Módulo público do destinatário
);
```

### 3. Criação da Assinatura Digital
```csharp
// Cria hash da mensagem original
string hash = SHA256Assigner.AssignString(message);

// Criptografa o hash usando nossa chave PRIVADA (assinatura)
string signature = EncoderRSA.Encrypt(
    hash,              // Hash da mensagem
    minhaChavePrivada.D, // Expoente privado (segredo)
    minhaChavePrivada.N  // Módulo privado
);
```

### 4. Envio do Payload
```json
{
  "Sender": "Bruno",
  "EncryptedMessage": "Asdrf23Fgh...", 
  "Signature": "Zxc789Qwe..."
}
```

---

## 📥 Processo de Recebimento de Mensagem

### 1. Descriptografia da Mensagem
```csharp
// Descriptografa usando nossa chave PRIVADA
string decryptedMessage = EncoderRSA.Decrypt(
    encryptedMessage,   // Mensagem criptografada recebida
    minhaChavePrivada.D, // Nossa chave privada D
    minhaChavePrivada.N  // Nosso módulo N
);
```

### 2. Validação da Assinatura
```csharp
// Descriptografa a assinatura usando a chave PÚBLICA do remetente
string decryptedSignature = EncoderRSA.Decrypt(
    signature,                // Assinatura recebida
    remetenteChavePublica.E,  // Chave pública do remetente
    remetenteChavePublica.N   // Módulo do remetente
);

// Verifica se o hash da mensagem confere com a assinatura
bool isValid = SHA256Assigner.StringAuthenticator(
    decryptedMessage,    // Mensagem descriptografada
    decryptedSignature   // Hash extraído da assinatura
);
```

### 3. Armazenamento
Se a validação passou, a mensagem é armazenada no histórico.

---

## 🛡️ Segurança Implementada

### Confidencialidade
- ✅ **Apenas o destinatário** pode descriptografar (usa chave privada dele)
- ✅ **Interceptadores** não conseguem ler a mensagem
- ✅ **Chaves privadas** nunca são transmitidas

### Integridade
- ✅ **Hash SHA256** detecta alterações na mensagem
- ✅ **Qualquer modificação** invalida a assinatura
- ✅ **Mensagens corrompidas** são rejeitadas

### Autenticidade
- ✅ **Assinatura digital** comprova quem enviou
- ✅ **Apenas o remetente** pode criar a assinatura
- ✅ **Falsificação** é impossível sem a chave privada

### Não-Repúdio
- ✅ **Remetente não pode negar** ter enviado
- ✅ **Assinatura** serve como prova
- ✅ **Timestamping** registra quando foi recebida

---

## 📊 Fluxo Completo

```
[Bruno]                    [Rede]                    [Felipe]
   |                          |                         |
   |--- POST /chat/key ------>|----------->|            |
   |<-- Resposta (chave) -----|<-----------|            |
   |                          |                         |
   |                          |                         |
   |=== HANDSHAKE COMPLETO ===|                         |
   |                          |                         |
   |                          |                         |
   |-- Criptografa Mensagem --|                         |
   |-- Assina Digitalmente ---|                         |
   |-- POST /chat/message --->|----------->|            |
   |                          |            |-- Descriptografa --|
   |                          |            |-- Valida Assinatura|
   |                          |            |-- Armazena Msg ----|
   |<-- "Mensagem recebida" --|<-----------|            |
```

---

## 🔧 Estrutura dos DTOs

### ConnectRequestDTO
```csharp
{
    string OtherUrl;  // URL do terminal para conectar
}
```

### PublicKeyDTO  
```csharp
{
    string Name;      // Nome do terminal
    string N;         // Módulo RSA (público)
    string E;         // Expoente RSA (público)
    string Url;       // URL do terminal
}
```

### SimpleMessageDTO
```csharp
{
    string Receiver;  // Nome do destinatário
    string Message;   // Mensagem em texto plano
}
```

### MessageDTO
```csharp
{
    string Sender;           // Nome do remetente
    string EncryptedMessage; // Mensagem criptografada
    string Signature;        // Assinatura digital
}
```

### ReceivedMessageDTO
```csharp
{
    string Sender;    // Quem enviou
    string Message;   // Mensagem descriptografada
    DateTime Timestamp; // Quando foi recebida
}
```

---

## ⚡ Performance e Limitações

### Limitações do RSA
- ✅ **Seguro** para chaves 2048+ bits
- ⚠️ **Lento** para mensagens grandes
- ⚠️ **Tamanho máximo** da mensagem = (bits da chave / 8) - padding

### Otimizações Possíveis
- 🔄 **RSA + AES**: RSA para chave simétrica, AES para dados
- 🔄 **Diffie-Hellman**: Para troca de chaves mais eficiente
- 🔄 **ECC**: Curvas elípticas para maior performance

---

## 🧪 Como Testar a Segurança

### 1. Interceptação
Monitore o tráfego HTTP - deve ver apenas dados criptografados.

### 2. Modificação
Altere bytes da mensagem - deve falhar na validação.

### 3. Replay Attack
Reenvie mensagem antiga - será aceita (implementar nonce se necessário).

### 4. Man-in-the-Middle
Substitua chaves públicas - pode comprometer se não houver validação adicional.

---

**Sistema implementado com sucesso! 🚀** 