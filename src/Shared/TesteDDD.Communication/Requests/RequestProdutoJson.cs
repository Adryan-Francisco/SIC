using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Communication.Requests
{
    public class RequestProdutoJson
    {
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }
}
