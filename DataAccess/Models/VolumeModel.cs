using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class VolumeModel
    {
        public int VolumeId { get; set; }
        public int NovelId { get; set; }
        public string Title { get; set; }
        public int? VolumeNumber { get; set; }
        public string Description { get; set; }
    }
}
