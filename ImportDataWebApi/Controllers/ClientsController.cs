using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using static ImportDataWebApi.Controllers.ClientsController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImportDataWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        public class Client
        {
            [Key]
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            [SensitiveData]
            public string? CellPhone { get; set; }
            public DateTime? BirthDate { get; set; }
            public int? Sex { get; set; }
            public int Status { get; set; }
            public DateTime CreatedDate { get; set; }
            [SensitiveData]
            public string? Cpf { get; set; }
            public string? Complement { get; set; }
        }
        public class ClientDTO
        {

            public string? Name { get; set; }
            public string? Email { get; set; }
            [SensitiveData]
            public string? CellPhone { get; set; }
            public string? BirthDate { get; set; }
            public int? Sex { get; set; }
            public int Status { get; set; }
            public string? Complement { get; set; }
            [SensitiveData]
            public string? Cpf { get; set; }
        }
        public class ClientClinic
        {
            [Key]
            public int Id { get; set; }
            public int IdClient { get; set; }
            public int IdClinic { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> Post(
            [FromBody] List<ClientDTO> clients,
            [FromHeader] int idClinic
            )
        {

            foreach (var item in clients)
            {
                try
                {


                    var client = new Client
                    {
                        CreatedDate = DateTime.Now,
                        CellPhone = formatCellPhone(item.CellPhone),
                        Name = item.Name,
                        BirthDate = item.BirthDate != null ? DateTime.Parse(item.BirthDate) : null,
                        Sex = item.Sex,
                        Status = item.Status,
                        Email = item.Email,
                        Cpf = item.Cpf,
                    };
                    await _context.Set<Client>().AddAsync(client);
                    await _context.SaveChangesAsync();

                    await _context.Set<ClientClinic>().AddAsync(new ClientClinic
                    {
                        IdClient = client.Id,
                        IdClinic = idClinic
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return Ok("Cadastro salvo com sucesso!");

        }

        private string formatCellPhone(string? item)
        {
            string CellPhone = "";
            if (item != null)
            {
                CellPhone = item.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
                //se numero não for valido salva vazio
                CellPhone = RemoverCodigoPais(CellPhone);
                if (!ValidarNumeroTelefone(CellPhone))
                {
                    CellPhone = "";
                }
            }
            return CellPhone;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file, [FromHeader] int idClinic)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, envie um arquivo válido.");

            // Garantir que o EPPlus esteja configurado para uso não comercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = new List<dynamic>();

            using (var stream = new MemoryStream())
            {
                // Copiar o arquivo para o MemoryStream
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    // Selecionar a primeira planilha
                    var worksheet = package.Workbook.Worksheets[0];
                    int rows = worksheet.Dimension.Rows;
                    int columns = worksheet.Dimension.Columns;

                    // Processar as células
                    for (int i = 2; i <= rows; i++) // Pula o cabeçalho (linha 1)
                    {
                        var rowData = new Client
                        {
                            Name = worksheet.Cells[i, 1].Text,
                            Email = worksheet.Cells[i, 2].Text,
                            CellPhone = formatCellPhone(worksheet.Cells[i, 3].Text),
                            BirthDate = formatDate(worksheet.Cells[i, 4].Text),
                            Complement = worksheet.Cells[i, 5].Text,
                            CreatedDate = DateTime.Now,
                            Status = 0,
                        };

                        data.Add(rowData);
                    }
                }
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var item in data)
                    {
                        await _context.Set<Client>().AddAsync(item);
                        await _context.SaveChangesAsync();

                        await _context.Set<ClientClinic>().AddAsync(new ClientClinic
                        {
                            IdClient = item.Id,
                            IdClinic = idClinic
                        });
                        await _context.SaveChangesAsync();
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {

                    transaction.Rollback();
                    return BadRequest("Erro, registro não efetuado!");
                }
                
            }
            // Retornar os dados processados
            return Ok("Salvo com sucesso.");
        }
        private DateTime? formatDate(string? inputDate)
        {
            //string inputDate = "15 de Outubro de 1984";

            // Define o formato da data
            string format = "dd 'de' MMMM 'de' yyyy";
            DateTime dateTime = DateTime.Now;
            // Cultura para reconhecer os nomes dos meses em português
            var culture = new CultureInfo("pt-BR");

            // Converte a data para o formato desejado
            if (DateTime.TryParseExact(inputDate, format, culture, DateTimeStyles.None, out DateTime parsedDate))
            {
                // Exibe no formato ISO 8601 (yyyy-MM-dd)
                return parsedDate;
            }
            else
            {
                return null;
            }
        }

        private bool ValidarNumeroTelefone(string numero)
        {
            string padrao = @"^\(?\d{2}\)?\s?\d{4,5}-?\d{4}$";
            return Regex.IsMatch(numero, padrao);
        }
        static string RemoverCodigoPais(string telefone)
        {
            // Regex para identificar o código do país +55 ou 55 no início do número
            string padrao = @"^\+?55";
            return Regex.Replace(telefone, padrao, "");
        }

    }
}
