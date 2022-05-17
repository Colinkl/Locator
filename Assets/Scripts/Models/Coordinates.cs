using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class Coordinates
    {
        public Coordinates(float longitude, float latitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
