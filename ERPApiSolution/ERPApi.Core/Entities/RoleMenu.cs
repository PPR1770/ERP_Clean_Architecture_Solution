namespace ERPApi.Core.Entities
{
    public class RoleMenu
    {
        public string RoleId { get; set; } = string.Empty;
        public int MenuId { get; set; }

        // Navigation properties
        public virtual ApplicationRole Role { get; set; } = null!;
        public virtual Menu Menu { get; set; } = null!;
    }
}