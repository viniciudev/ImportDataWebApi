using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
            public int Sex { get; set; }
            public int Status { get; set; }
            public DateTime CreatedDate { get; set; }
            [SensitiveData]
            public string? Cpf { get; set; }
        }
        public class ClientDTO
        {
   
            public string? Name { get; set; }
            public string? Email { get; set; }
            [SensitiveData]
            public string? CellPhone { get; set; }
            public string? BirthDate { get; set; }
            public int Sex { get; set; }
            public int Status { get; set; }
         
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
                    if (item.CellPhone != null)
                    {
                        item.CellPhone = item.CellPhone.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
                        //se numero não for valido salva vazio
                        item.CellPhone=RemoverCodigoPais(item.CellPhone);
                        if (!ValidarNumeroTelefone(item.CellPhone))
                        {
                            item.CellPhone = "";
                        }
                    }
        
                    var client = new Client {
                    CreatedDate= DateTime.Now,
                    CellPhone = item.CellPhone,
                    Name = item.Name,
                    BirthDate = item.BirthDate!=null? DateTime.Parse(item.BirthDate):null,
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
