using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Communication.Responses
{
    public class ResponseProdutoJson
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public Guid CategoriaId { get; set; }
    }
}
