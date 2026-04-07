using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Domain.Entities
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;

        protected Categoria() { }

        public Categoria(string nome, string descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("Nome é obrigatório");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new Exception("Descrição é obrigatória");

            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao;
        }

        public void Update(string nome, string descricao)
        {
            Nome = nome;
            Descricao = descricao;
        }
    }
}
