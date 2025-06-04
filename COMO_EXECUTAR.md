# Como Executar em Dois Terminais Simultaneamente

Agora que a aplicação foi configurada para usar URLs específicas por environment, vocês podem executar nos dois terminais ao mesmo tempo:

## Opção 1: Usando Profiles do launchSettings.json (RECOMENDADO)

### Terminal 1 - Environment Bruno (Porta 5001)
```powershell
dotnet run --launch-profile Bruno
```

### Terminal 2 - Environment Felipe (Porta 5000)
```powershell
dotnet run --launch-profile Felipe
```

## Opção 2: Usando Variáveis de Ambiente

### Terminal 1 - Environment Bruno (Porta 5001)
```powershell
$env:ASPNETCORE_ENVIRONMENT="Bruno"; dotnet run
```

### Terminal 2 - Environment Felipe (Porta 5000)
```powershell
$env:ASPNETCORE_ENVIRONMENT="Felipe"; dotnet run
```

## Verificação
Após executar ambos:
- Bruno: http://localhost:5001
- Felipe: http://localhost:5000

## Swagger UI
- Bruno: http://localhost:5001/swagger
- Felipe: http://localhost:5000/swagger

## Observações
- Cada environment agora roda em uma porta diferente
- O arquivo `appsettings.{Environment}.json` será carregado automaticamente
- Não haverá mais conflito de porta entre os dois environments
- A **Opção 1** é mais fácil e confiável pois usa os profiles configurados 