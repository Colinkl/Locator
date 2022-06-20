using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geolocation;
using Unity.Mathematics;

public class RadarScript : MonoBehaviour
{
    class Dot
    {
        public POI POI { get; set; }
        public float Distance { get; set; }
        public Vector2 RelPosition { get; set; }
        public Coordinate Coordinate { get; set; }
        public float BaseAngle { get; set; }
        public bool IsVisible { get; set; }
        public GameObject sprite { get; set; }
    }

    [SerializeField]
    public GameObject Sprite;

    private GameObject _radarPlane;
    private Vector2 _radarLocation;
    private float _radarSize;

    private List<POI> _points;
    public List<POI> Points
    {
        get
        {
            return _points;
        }
        set
        {
            _points = value;
            poiInit();
        }
    }
    private Coordinate _currentCoordinate;
    private List<Dot> _dots;
    public float Scale { get; private set; } = 500;
    public void Startup(List<POI> points)
    {
        _points = points;

    }
    private float CalcDistanceTo(Coordinate point)
    {
        float distance = (float)GeoCalculator.GetDistance(_currentCoordinate, point, 5, DistanceUnit.Meters);
        return distance;
    }

    private float CalcAngleTo(Coordinate point)
    {
        float angle = (float)GeoCalculator.GetBearing(_currentCoordinate, point);
        return angle;
    }
    private void poiInit()
    {
        _dots = new List<Dot>();

        foreach (POI p in _points)
        {
            var dot = new Dot() { POI = p, Coordinate = new Coordinate(p.Coordinates.Latitude, p.Coordinates.Longitude) };
            _dots.Add(dot);
        }
    }

    private void CalcDistance()
    {
        _dots.ForEach(dot =>
        {
            dot.Distance = CalcDistanceTo(dot.Coordinate);
            dot.BaseAngle = CalcAngleTo(dot.Coordinate);
            dot.RelPosition = _radarSize * dot.Distance * new Vector2(MathF.Cos(dot.BaseAngle), Mathf.Sin(dot.BaseAngle)) / Scale;
            if (dot.Distance <= Scale)
            {
                dot.IsVisible = true;
            }
            else
            {
                dot.IsVisible = false;
            }
        });

    }

    public void OnCoordsUpdate(LocationInfo locationInfo)
    {
        //_currentCoordinate = new Coordinate(locationInfo.latitude, locationInfo.longitude);
        //CalcDistance();
        //ShowDots();
    }

    public void UpdateScale(float scale)
    {
        Scale = scale;
        _dots.ForEach(dot =>
        {
            if (dot.Distance <= Scale)
            {
                dot.IsVisible = true;
            }
            else
            {
                dot.IsVisible = false;
            }
        });
    }
    [ContextMenu("ShowDots")]
    public void ShowDots()
    {
        _dots.ForEach(dot =>
        {
            if (dot.sprite is null && dot.IsVisible)
            {
                Vector3 loc = _radarLocation + dot.RelPosition;
                dot.sprite = Instantiate(Sprite, loc, Quaternion.identity, _radarPlane.transform);
            }
            if (dot.sprite is not null && dot.IsVisible)
            {
                Vector3 loc = _radarLocation + dot.RelPosition;
                dot.sprite.transform.position = loc;
            }
            if (dot.sprite is not null && !dot.IsVisible)
            {
                Vector3 loc = _radarLocation + dot.RelPosition;
                Destroy(dot.sprite);
            }
        });
    }
    private double lastTimeStamp;

    float Yp; 
    float Ypp;
    float Yppp;
    float Xp;
    float Xpp;

    public float thirdOrder_lowpassFilter(float X, float beta)
    {
        float Y;

        Y = beta * X + beta * (1 - beta) * Xp + beta * (1 - beta) * (1 - beta) * Xpp + (1 - beta) * (1 - beta) * (1 - beta) * Yppp;

        Xpp = Xp;
        Xp = X;
        Yppp = Ypp;
        Ypp = Yp;
        Yp = Y;

        return Y;
    }
    private void Start()
    {
        Input.compass.enabled = true;

        lastTimeStamp = 10;
        _radarLocation = gameObject.transform.position;
        _radarSize = gameObject.transform.localScale.x / 2;
        _radarPlane = GameObject.Find("Radar");
        StartCoroutine(StartCompass());
    }

    IEnumerator StartCompass()
    {
        _radarPlane.transform.rotation = Quaternion.Euler(0, 0, thirdOrder_lowpassFilter(Input.compass.trueHeading, 0.6f));
        yield return new WaitForSeconds(0.5f);
    }
    public void Update()
    {
       


    }

    // Start is called before the first frame update



}
