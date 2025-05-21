using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingIt.Shared.Dtos
{
    public class LocationDto
    {
        public int? Id { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
