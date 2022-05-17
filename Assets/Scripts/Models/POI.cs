using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.Models
{
    public class POI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Coordinates Coordinates { get; set; }
        public string Desctiption { get; set; }
        public decimal Rating { get; set; }
    }

}
