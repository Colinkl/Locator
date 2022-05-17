using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarScript : MonoBehaviour
{
    class Dot
    {
        public POI POI { get; set; }
        public float Distance { get; set; }
        public Vector2 RelPosition { get; set; }


    }


    private Vector2 _radarLocation;
    private float _radarSize;
    private List<POI> _points;
    private Coordinates _currentLocation;
    private List<Dot> _dots;
    public float Scale { get; set; }
    public RadarScript(List<POI> points)
    {
        _points = points;
        _radarLocation = gameObject.transform.position;
        _radarSize = gameObject.transform.localScale.x;        
    }
    private float CalcDistanceBetween(Coordinates p1, Coordinates p2)
    {
        Func<float, float> Radians = (angle) =>
        {
            return (float)(angle * (Math.PI / 180));
        };

        float R = 6371f; // km

        float sLat1 = Mathf.Sin(Radians(p1.Latitude));
        float sLat2 = Mathf.Sin(Radians(p2.Latitude));
        float cLat1 = Mathf.Cos(Radians(p1.Latitude));
        float cLat2 = Mathf.Cos(Radians(p2.Latitude));
        float cLon = Mathf.Cos(Radians(p1.Longitude) - Radians(p2.Longitude));

        float cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

        float d = Mathf.Acos(cosD);

        float dist = R * d;

        return dist;
    }
    private void locationMapper()
    {
        _dots = new List<Dot>();

        foreach (POI p in _points)
        {
            var distance = CalcDistanceBetween(_currentLocation, p.Coordinates);
            if (distance <= Scale)
            {
                var rel = _radarLocation + _radarSize * distance * new Vector2(p.Coordinates.Latitude - _currentLocation.Latitude, p.Coordinates.Longitude - _currentLocation.Longitude).normalized / Scale ; 
                _dots.Add(new Dot() { POI = p, Distance = distance, RelPosition = rel});
            }
        }
    }

    // Start is called before the first frame update

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
