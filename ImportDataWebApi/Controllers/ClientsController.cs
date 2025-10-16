using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using static ImportDataWebApi.Controllers.ClientsController;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            public string ?Color { get; set; }
            public string ?ZipCode { get; set; }
            public string ?Address { get; set; }
            public string ?Bairro { get; set; }
            public string ?NameState { get; set; }
            public string ?NameCity { get; set; }
            public string ?CnhRecord { get; set; }
            public string ?Identifier { get; set; }
            public int Number { get; set; }
            public string ?MunicipalCodeIbge { get; set; }
            public byte[] ?CustomerPhoto { get; set; }
           
        }
        public class ClientDTO
        {

            public string? Name { get; set; }
            public string? Email { get; set; }
            [SensitiveData]
            public string? CellPhone { get; set; }
            public string? BirthDate { get; set; }
            public string? Sex { get; set; }//0=masculinmo, 1=femini
            public int Status { get; set; }
            public string? Complement { get; set; }
            [SensitiveData]
            public string? Cpf { get; set; }
            public string? Color { get; set; }
            public string? ZipCode { get; set; }
            public string? Address { get; set; }
            public string? Bairro { get; set; }
            public string? NameState { get; set; }
            public string? NameCity { get; set; }
            public string? CnhRecord { get; set; }
            public string? Identifier { get; set; }
            public int Number { get; set; }
            public string? MunicipalCodeIbge { get; set; }
            public byte[]? CustomerPhoto { get; set; }
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
                        Sex =(item.Sex!=null &&item.Sex.ToLower().Contains("F"))?1:0,
                        Status = item.Status,
                        Email = item.Email,
                        Cpf = item.Cpf,
                        Address = item.Address,
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
            if (!string.IsNullOrEmpty( item) )
            {
                CellPhone = RemoveSpecialCharactersAndKeepNumbers(item);
                //se numero não for valido salva vazio
                CellPhone = RemoverCodigoPais(CellPhone).Trim();
                //if (!ValidarNumeroTelefone(CellPhone))
                //{
                //    CellPhone = "";
                //}
            }
            else
            {
                CellPhone = "99999999999";
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
                            Sex =string.IsNullOrEmpty( worksheet.Cells[i, 5].Text)?null: worksheet.Cells[i, 5].Text.ToLower(). Contains("F") ? 1 : 0 ,
                            Status= 0,
                            CreatedDate = DateTime.Now,//worksheet.Cells[i, 7].Text,
                            Cpf = worksheet.Cells[i, 8].Text.Replace(".", "").Replace("-", "").Trim(),
                            Complement = worksheet.Cells[i, 9].Text,
                            Address = worksheet.Cells[i, 10].Text,
                            Bairro= worksheet.Cells[i, 11].Text,
                            Number= string.IsNullOrEmpty(worksheet.Cells[i, 12].Text)?0:  int.Parse(ApenasNumeros(worksheet.Cells[i, 12].Text)),
                            NameState= worksheet.Cells[i, 13].Text,
                            NameCity= worksheet.Cells[i, 14].Text,
                            ZipCode = worksheet.Cells[i, 15].Text,
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
        [HttpPut("alter")]
        public async Task<IActionResult> alter(IFormFile file, [FromHeader] int idClinic)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Por favor, envie um arquivo válido.");

            // Garantir que o EPPlus esteja configurado para uso não comercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = new List<Client>();

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
                       
                            //Name = worksheet.Cells[i, 1].Text,
                            //Email = worksheet.Cells[i, 2].Text,
                            CellPhone = formatCellPhone(worksheet.Cells[i, 3].Text),
                            //BirthDate = formatDate(worksheet.Cells[i, 4].Text),
                            //Sex = string.IsNullOrEmpty(worksheet.Cells[i, 5].Text) ? null : worksheet.Cells[i, 5].Text.ToLower().Contains("F") ? 1 : 0,
                            //Status = 0,
                            //CreatedDate = DateTime.Now,//worksheet.Cells[i, 7].Text,
                            //Cpf = worksheet.Cells[i, 8].Text.Replace(".", "").Replace("-", "").Trim(),
                            //Complement = worksheet.Cells[i, 9].Text,
                            //Address = worksheet.Cells[i, 10].Text,
                            //Bairro = worksheet.Cells[i, 11].Text,
                            //Number = string.IsNullOrEmpty(worksheet.Cells[i, 12].Text) ? 0 : int.Parse(ApenasNumeros(worksheet.Cells[i, 12].Text)),
                            //NameState = worksheet.Cells[i, 13].Text,
                            //NameCity = worksheet.Cells[i, 14].Text,
                            //ZipCode = worksheet.Cells[i, 15].Text,
                            Id =int.Parse( worksheet.Cells[i, 16].Text),
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
                        var cli= _context.Set<Client>().Find(item.Id);
                        if (cli != null)
                        {
                            cli.CellPhone = item.CellPhone;
                            _context.Set<Client>().Update(cli);
                            await _context.SaveChangesAsync();
                        }
                        //await _context.Set<ClientClinic>().AddAsync(new ClientClinic
                        //{
                        //    IdClient = item.Id,
                        //    IdClinic = idClinic
                        //});
                        //await _context.SaveChangesAsync();
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
        [HttpGet("download-clients")]
        public IActionResult DownloadClients()
        {
            //List<Client> clients = GetClients(); // Simulação de dados
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Clients");

                // Cabeçalhos da planilha
               
                worksheet.Cells[1, 1].Value = "Nome";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Telefone";
                worksheet.Cells[1, 4].Value = "Data de Nascimento";
                worksheet.Cells[1, 5].Value = "Sexo";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Data de Criação";
                worksheet.Cells[1, 8].Value = "CPF";
                worksheet.Cells[1, 9].Value = "Complemento";
                worksheet.Cells[1, 10].Value = "Address";
                worksheet.Cells[1, 11].Value = "Bairro";
                worksheet.Cells[1, 12].Value = "Number";
                worksheet.Cells[1, 13].Value = "NameState";
                worksheet.Cells[1, 14].Value = "NameCity";
                worksheet.Cells[1, 15].Value = "ZipCode";

                // Auto ajustar colunas
                worksheet.Cells.AutoFitColumns();

                // Gerar arquivo Excel na memória
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Clientes.xlsx");
            }
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
            else if  (DateTime.TryParseExact(inputDate, "d/M/yyyy", culture, DateTimeStyles.None, out DateTime parsedDate2))
            {
                // Exibe no formato ISO 8601 (yyyy-MM-dd)
                return parsedDate2;
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
        public static string ApenasNumeros(string input)
        {
            return Regex.Replace(input, "[^0-9]", ""); // Remove tudo que não for número
        }

        public static string RemoveSpecialCharactersAndKeepNumbers(string input)
        {
            // Define uma expressão regular para manter apenas os números
            string pattern = @"[^0-9]";
            string replacement = "";

            // Substitui todos os caracteres que não são números por uma string vazia
            string result = Regex.Replace(input, pattern, replacement);
            return result;
        }

    }
}
