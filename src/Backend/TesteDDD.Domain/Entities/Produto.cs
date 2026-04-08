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

        public Guid CategoriaId { get; private set; }
        public Categoria Categoria { get; private set; } = null!;

        protected Produto() { }

        public Produto(string nome, decimal preco, Guid categoriaId)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));
            if (preco <= 0)
                throw new ArgumentException("Preço deve ser maior que zero", nameof(preco));
            if (categoriaId == Guid.Empty)
                throw new ArgumentException("CategoriaId é obrigatório", nameof(categoriaId));

            Id = Guid.NewGuid();
            Nome = nome;
            Preco = preco;
            CategoriaId = categoriaId;
        }

        public void Update(string nome, decimal preco)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório", nameof(nome));
            if (preco <= 0)
                throw new ArgumentException("Preço deve ser maior que zero", nameof(preco));

            Nome = nome;
            Preco = preco;
        }

        public void DefinirCategoria(Categoria categoria)
        {
            if (categoria == null)
                throw new ArgumentNullException(nameof(categoria));

            Categoria = categoria;
            CategoriaId = categoria.Id;
        }
    }
}