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
        
        // Uma categoria pode ter muitos produtos
        public ICollection<Produto> Produtos { get; private set; } = new List<Produto>();

        protected Categoria() { }

        public Categoria(string nome, string descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(descricao));

            Id = Guid.NewGuid();
            Nome = nome;
            Descricao = descricao;
        }

        public void Update(string nome, string descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));

            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Descrição é obrigatória", nameof(descricao));

            Nome = nome;
            Descricao = descricao;
        }
    }
}
