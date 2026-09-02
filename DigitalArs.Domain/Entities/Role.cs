namespace DigitalArs.Domain.Entities
{
    // Representa el rol que tiene un usuario dentro del sistema.
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


        // Lista de usuarios que tienen asignado este rol.
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
// Identifica entre el Administrador y el Usuario