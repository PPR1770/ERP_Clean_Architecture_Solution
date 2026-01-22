namespace ERPApi.Core.Entities
{
    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }

        // Navigation properties
        public virtual ApplicationRole Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}