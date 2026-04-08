using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Domain.Entities
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        
        public string Endereco { get; private set; } = string.Empty;

        public string Cep { get; private set; } = string.Empty;

        protected Cliente () { }

        public Cliente(Guid id, string nome, string endereco, string cep)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));
            if (string.IsNullOrWhiteSpace(endereco))
                throw new ArgumentException("Endereço é obrigatório", nameof(endereco));
            if (string.IsNullOrWhiteSpace(cep))
                throw new ArgumentException("CEP é obrigatório", nameof(cep));

            Id = id;
            Nome = nome;
            Endereco = endereco;
            Cep = cep;
        }
        public void Update(string nome, string endereco, string cep)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));
            if (string.IsNullOrWhiteSpace(endereco))
                throw new ArgumentException("Endereço é obrigatório", nameof(endereco));
            if (string.IsNullOrWhiteSpace(cep))
                throw new ArgumentException("CEP é obrigatório", nameof(cep));

            Nome = nome;
            Endereco = endereco;
            Cep = cep;
        }
    }
}
