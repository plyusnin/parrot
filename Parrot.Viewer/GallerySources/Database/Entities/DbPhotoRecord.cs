using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parrot.Viewer.GallerySources.Database.Entities
{
    [Table("Photos")]
    public class DbPhotoRecord
    {
        [Key]
        public int Id { get; set; }

        public string File { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}
