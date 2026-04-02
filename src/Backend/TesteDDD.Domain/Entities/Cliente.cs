using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        
        public string Endereco { get; set; } = string.Empty;

        public string Cep { get; set; } = string.Empty;

        protected Cliente () { }

        public Cliente(Guid id, string nome, string endereco, string cep)
        {
            Id = id;
            Nome = nome;
            Endereco = endereco;
            Cep = cep;
        }
        public void Update(string nome, string endereco, string cep)
        {
            Nome = nome;
            Endereco = endereco;
            Cep = cep;
        }
    }
}
