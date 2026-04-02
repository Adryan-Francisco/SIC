using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteDDD.Domain.Entities.Categoria
{
    public class Categoria
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public string Descricao { get; set; } = string.Empty;

        protected Categoria() { }

        public Categoria (string name, string descricao)
        {
            Id = Guid.NewGuid();
            Name = name;
            Descricao = descricao;
        }

        public void Update (string name, string descricao)
        {
            Name = name;
            Descricao = descricao;
        }
    }
}
