# 🚀 Guia Rápido - Chat Criptografado

## ✅ O que foi implementado

Seu projeto agora é um **chat criptografado com RSA** onde Bruno e Felipe podem trocar mensagens seguras!

---

## 📋 Passo a Passo Simples

### 1. Abrir os dois terminais

**Terminal 1:**
```powershell
dotnet run --launch-profile Bruno
```

**Terminal 2:**
```powershell
dotnet run --launch-profile Felipe
```

### 2. Conectar os dois (apenas uma vez)

**No terminal do Bruno:**
```powershell
curl -X POST "http://localhost:5001/chat/connect" -H "Content-Type: application/json" -d '{"OtherUrl": "http://localhost:5000"}'
```

### 3. Agora podem conversar!

**Bruno manda mensagem para Felipe:**
```powershell
curl -X POST "http://localhost:5001/chat/send-simple" -H "Content-Type: application/json" -d '{"Receiver": "Felipe", "Message": "Oi Felipe!"}'
```

**Felipe responde para Bruno:**
```powershell
curl -X POST "http://localhost:5000/chat/send-simple" -H "Content-Type: application/json" -d '{"Receiver": "Bruno", "Message": "Oi Bruno! Como vai?"}'
```

### 4. Ver mensagens recebidas

**Ver mensagens do Bruno:**
```powershell
curl "http://localhost:5001/chat/messages"
```

**Ver mensagens do Felipe:**
```powershell
curl "http://localhost:5000/chat/messages"
```

---

## 🎯 URLs Úteis

- **Bruno**: http://localhost:5001
- **Felipe**: http://localhost:5000
- **Swagger Bruno**: http://localhost:5001/swagger
- **Swagger Felipe**: http://localhost:5000/swagger

---

## 💡 Dicas

- ✅ As mensagens aparecerão **no console** também!
- ✅ Todas as mensagens são **criptografadas**
- ✅ Use o **Swagger** para teste visual
- ✅ As mensagens ficam **salvas** até limpar ou reiniciar
- ✅ Pode mandar quantas mensagens quiser

---

## 🔒 Segurança

- Cada mensagem é criptografada com **RSA 2048 bits**
- Assinatura digital garante **autenticidade**
- Apenas o destinatário pode **descriptografar**

---

## 🛠️ Endpoints Disponíveis

| Endpoint | Descrição |
|----------|-----------|
| `POST /chat/connect` | Conecta com outro terminal |
| `POST /chat/send-simple` | Envia mensagem simples |
| `GET /chat/messages` | Lista mensagens recebidas |
| `GET /chat/connections` | Lista conexões ativas |
| `DELETE /chat/messages` | Limpa mensagens |

**Agora é só testar! 🚀** 