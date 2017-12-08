using System.Data.Entity;

namespace Parrot.Viewer.GallerySources.Database.Entities
{
    public class GalleryContext : DbContext
    {
        public GalleryContext()
            : base("DefaultConnection") { }

        public DbSet<DbPhotoRecord> Photos { get; set; }
    }
}
