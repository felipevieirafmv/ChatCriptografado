# 💬 Comandos do Chat Criptografado

## Como usar o chat entre Bruno e Felipe

### 1. 🚀 Iniciar os dois terminais

**Terminal 1 - Bruno (Porta 5001):**
```powershell
dotnet run --launch-profile Bruno
```

**Terminal 2 - Felipe (Porta 5000):**
```powershell
dotnet run --launch-profile Felipe
```

---

## 2. 🤝 Conectar os dois terminais (Handshake)

### Do terminal do Bruno, conectar com Felipe:
```powershell
curl -X POST "http://localhost:5001/chat/connect" -H "Content-Type: application/json" -d '{"OtherUrl": "http://localhost:5000"}'
```

### Do terminal do Felipe, conectar com Bruno:
```powershell
curl -X POST "http://localhost:5000/chat/connect" -H "Content-Type: application/json" -d '{"OtherUrl": "http://localhost:5001"}'
```

---

## 3. 📤 Enviar mensagens

### Bruno enviando para Felipe:
```powershell
curl -X POST "http://localhost:5001/chat/send-simple" -H "Content-Type: application/json" -d '{"Receiver": "Felipe", "Message": "Oi Felipe, tudo bem?"}'
```

### Felipe enviando para Bruno:
```powershell
curl -X POST "http://localhost:5000/chat/send-simple" -H "Content-Type: application/json" -d '{"Receiver": "Bruno", "Message": "Oi Bruno! Tudo ótimo e você?"}'
```

---

## 4. 📥 Ver mensagens recebidas

### Ver mensagens do Bruno:
```powershell
curl "http://localhost:5001/chat/messages"
```

### Ver mensagens do Felipe:
```powershell
curl "http://localhost:5000/chat/messages"
```

---

## 5. 🔍 Verificar conexões ativas

### Bruno verificar conexões:
```powershell
curl "http://localhost:5001/chat/connections"
```

### Felipe verificar conexões:
```powershell
curl "http://localhost:5000/chat/connections"
```

---

## 6. 🗑️ Limpar mensagens

### Limpar mensagens do Bruno:
```powershell
curl -X DELETE "http://localhost:5001/chat/messages"
```

### Limpar mensagens do Felipe:
```powershell
curl -X DELETE "http://localhost:5000/chat/messages"
```

---

## 🎯 Fluxo Completo de Teste

1. **Abra dois terminais** e execute os comandos do item 1
2. **Faça o handshake** usando os comandos do item 2
3. **Envie mensagens** de um para o outro usando o item 3
4. **Veja as mensagens** que chegaram usando o item 4
5. **As mensagens aparecerão no console de cada terminal também!**

---

## 🔒 Como funciona a criptografia

- ✅ Cada mensagem é **criptografada com RSA** usando a chave pública do destinatário
- ✅ Cada mensagem é **assinada digitalmente** para garantir autenticidade
- ✅ Apenas o destinatário pode **descriptografar** a mensagem
- ✅ A assinatura garante que a mensagem **não foi alterada**

---

## 🌐 URLs dos Swagger

- **Bruno Swagger**: http://localhost:5001/swagger
- **Felipe Swagger**: http://localhost:5000/swagger

Você pode usar o Swagger para testar os endpoints também! 