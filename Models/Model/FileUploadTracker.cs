using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Model
{
    public class FileUploadTracker
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Filename { get; set; }
        [Required]
        public byte FileType { get; set; }
        [Required]
        public byte UploadStatus { get; set; }
        public string? Error { get; set; }
        public string? Remarks { get; set; }
        [Required]
        public byte ReadStatus { get; set; }
        [Required]
        public string UploadedBy { get; set; }
    }
}
