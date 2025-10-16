# ImportDataWebApi

API em **.NET 6 / ASP.NET Core** para **importação de dados de planilhas (Excel / CSV)** e cadastro automático de clientes em banco **SQL Server**.

---

## 🧰 Tecnologias / Stack

- .NET 8 / ASP.NET Core  
- C#  
- Entity Framework Core  
- SQL Server   

---

## 📂 Estrutura do Projeto (esquemática)

ImportDataWebApi/
├─ Controllers/ ← APIs / Endpoints
├─ Models/ ← Modelos / DTOs
├─ Services/ ← Lógica de importação e persistência
├─ Data/ ← DbContext, configurações de EF Core
├─ Importers/ ← Classes que leem planilhas CSV/Excel
├─ ImportDataWebApi.csproj
├─ appsettings.json
├─ README.md
└─ .gitignore

