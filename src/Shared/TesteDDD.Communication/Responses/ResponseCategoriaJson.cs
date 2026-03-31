using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Communication.Responses
{
    public class ResponseCategoriaJson
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Descricao { get; set; } 
    }
}
