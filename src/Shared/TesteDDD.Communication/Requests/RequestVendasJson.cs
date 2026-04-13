using System.ComponentModel.DataAnnotations;

namespace TesteDDD.Communication.Requests
{
   public class RequestVendasJson
    {
        public Guid Id { get; set; }
        [Required] 
        public DateTime DataVenda { get; set; }
        [Required]
        public decimal ValorTotal { get; set; }
        [Required]
        public List<RequestItemVendasJson> Itens { get; set; } = new List<RequestItemVendasJson>();

    }
}
