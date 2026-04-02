using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Domain.Entities
{
    public class Produto
    {
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }

        //Construtor para o Entity FrameWork
        protected Produto() { }

        public Produto(string nome, decimal preco)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Preco = preco;
        }

        public void Update (string nome, decimal preco)
        {
            // Aqui entrariam validações de domínio (ex: preço não pode ser negativo)
            Nome = nome;
            Preco = preco;
        }
    }
}
