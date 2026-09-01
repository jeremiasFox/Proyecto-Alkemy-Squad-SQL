namespace DigitalArs.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;

        // Rol asignado al usuario.
        public Role Role { get; set; } = null!;

        // Cuenta bancaria asociada al usuario.
        public Account Account { get; set; } = null!;
    }
}

// Registra a cada persona con sus datos y los guarda
