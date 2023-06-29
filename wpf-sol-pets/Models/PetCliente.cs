using System;

namespace wpf_sol_pets.Models
{
    public class PetCliente
    {
        public int IdCliente { get; set; }
        public string NomePet { get; set; }
        public string RacaPet { get; set; }
        public string TipoAnimalPet { get; set; }
        public DateTime DataNascimentoPet { get; set; }
        public DateTime DHUltimaAtualizacao { get; set; }
    }
}
