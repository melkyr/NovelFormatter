using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class VolumeModel
    {
        public int Volume_Id { get; set; }
        public int Novel_Id { get; set; }
        public string Title { get; set; }
        public int? Volume_Number { get; set; }
        public string Description { get; set; }
    }
}
