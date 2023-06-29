using System;
using System.Collections.Generic;

namespace wpf_sol_pets.Models
{
    public class Fornecedor
    {
        public Fornecedor()
        {
            Contatos = new List<Contato>();
        }
        public string NomeFornecedor { get; set; }
        public string CnpjFornecedor { get; set; }
        public DateTime DhUltimaAtualizacao { get; set; }
        public List<Contato> Contatos { get; set; }
    }
}
