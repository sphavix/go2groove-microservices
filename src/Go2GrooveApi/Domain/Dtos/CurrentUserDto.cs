namespace Go2GrooveApi.Domain.Dtos
{
    public record CurrentUserDto(string Id, string email, IEnumerable<string> Roles)
    {
        public bool IsInRole(string role) => Roles.Contains(role);
    }
}
