﻿using System;

namespace wpf_sol_pets.Models
{
    public class Login
    {
        public int IdLogin { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime DhUltimaAtualizacao { get; set; }
    }
}
