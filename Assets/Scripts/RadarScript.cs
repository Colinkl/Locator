using Assets.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geolocation;
using Unity.Mathematics;
using UnityEngine.UI;
using TMPro;

public class RadarScript : MonoBehaviour
{
    class Dot
    {
        public POI POI { get; set; }
        public float Distance { get; set; }
        public Vector3 RelPosition { get; set; }
        public Coordinate Coordinate { get; set; }
        public float BaseAngle { get; set; }
        public bool IsVisible { get; set; }
        public GameObject sprite { get; set; }
        public GameObject Item { get; set; }
        public Color Color { get; set; }
    }

    [SerializeField]
    public GameObject Sprite;
    [SerializeField]
    public GameObject Prefab;
    [SerializeField]
    public Text AngleLabel;
    private GameObject _radarPlane;
    private Vector2 _radarLocation;
    private GameObject _scrollView;
    private float radarSize;

    private Quaternion CompasLoc;

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
            PoiInit();
        }
    }
    public Coordinate CurrentCoordinate { get; set; }
    private List<Dot> _dots;
    public float Scale { get; private set; } = 500;
    public void Startup(List<POI> points)
    {
        _points = points;

    }
    private float CalcDistanceTo(Coordinate point)
    {
        float distance = (float)GeoCalculator.GetDistance(CurrentCoordinate, point, 5, DistanceUnit.Meters);
        return distance;
    }

    private float CalcAngleTo(Coordinate point)
    {
        float angle = (float)GeoCalculator.GetBearing(CurrentCoordinate, point);
        return angle;
    }
    public void PoiInit()
    {
        _dots = new List<Dot>();

        foreach (POI p in _points)
        {
            var dot = new Dot() { POI = p, Coordinate = new Coordinate(p.Coordinates.Latitude, p.Coordinates.Longitude) };
            dot.Color = new Color(
                  UnityEngine.Random.Range(0f, 1f),
                  UnityEngine.Random.Range(0f, 1f),
                  UnityEngine.Random.Range(0f, 1f)
                    );
            _dots.Add(dot);
        }
    }

    public void CalcDistance()
    {
        _dots.ForEach(x =>
        {
            if (x.Item is not null)
            {
                Destroy(x.Item);
                x.Item = null;
            }
        });
        _dots.ForEach(dot =>
        {
            radarSize = _radarPlane.GetComponent<SpriteRenderer>().bounds.max.x;
            dot.Distance = CalcDistanceTo(dot.Coordinate);
            dot.BaseAngle = CalcAngleTo(dot.Coordinate);
            
            float len = 0.5f * radarSize * (dot.Distance / Scale);
            //float len = 10f;
            dot.RelPosition = Quaternion.Euler(0, 0, dot.BaseAngle) * new Vector3(len, 0);
            if (dot.Distance <= Scale)
            {
                dot.Item = Instantiate(Prefab, _scrollView.transform);
                var item = dot.Item.transform.Find("Title").gameObject.GetComponent<TextMeshProUGUI>();
                item.color = dot.Color;
                item.text = dot.POI.Name;
                dot.Item.transform.Find("Rating").gameObject.GetComponent<TextMeshProUGUI>().text = dot.POI.Rating.ToString();
                dot.Item.transform.Find("Location").gameObject.GetComponent<TextMeshProUGUI>().text = $"{dot.POI.Coordinates.Latitude} {dot.POI.Coordinates.Longitude}";
                dot.Item.transform.Find("Distance").gameObject.GetComponent<TextMeshProUGUI>().text = $"{(int)dot.Distance} m";

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
        CurrentCoordinate = new Coordinate(locationInfo.latitude, locationInfo.longitude);
        CalcDistance();
        ShowDots();
    }

    public void UpdateScale(float scale)
    {
        Scale = scale;
        CalcDistance();
        ShowDots();
    }
    [ContextMenu("ShowDots")]
    public void ShowDots()
    {
        _dots.ForEach(dot =>
        {
            if (dot.sprite is null && dot.IsVisible)
            {
                dot.sprite = Instantiate(Sprite, _radarPlane.transform.rotation * dot.RelPosition + _radarPlane.transform.position, Quaternion.identity, _radarPlane.transform);
                dot.sprite.transform.localScale = new Vector3(1, 1, 1);
                dot.sprite.GetComponent<SpriteRenderer>().color = dot.Color;
                dot.sprite.GetComponent<Renderer>().sortingOrder = 1;
            }
            if (dot.sprite && dot.IsVisible)
            {
                dot.sprite.transform.position = _radarPlane.transform.rotation * dot.RelPosition + _radarPlane.transform.position;
            }
            if (dot.sprite is not null && !dot.IsVisible)
            {
                Destroy(dot.sprite);
                dot.sprite = null;
            }
        });
    }

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

        _radarPlane = GameObject.Find("Radar");
        _radarLocation = _radarPlane.transform.position;
        _scrollView = GameObject.Find("Content");
        radarSize = _radarPlane.transform.localScale.x / 2;
    }



    public IEnumerator StartCompass()
    {
        while (true)
        {
            CompasLoc = Quaternion.Euler(0, 0, thirdOrder_lowpassFilter(Input.compass.trueHeading, 0.2f));
            AngleLabel.text = $"Angle: {CompasLoc.z}";
            lastRot = _radarPlane.transform.rotation;
            yield return new WaitForSeconds(0.3f);
        }

    }

    float WhereTime = 0;
    float NeedTime = 0.3f;
    private Quaternion lastRot;

    public void Update()
    {
        if (lastRot.eulerAngles.z != CompasLoc.z)
        {
            if (WhereTime < NeedTime)
                _radarPlane.transform.rotation = Quaternion.Lerp(lastRot, CompasLoc, Mathf.Clamp((WhereTime += Time.deltaTime) / NeedTime, 0, 1));
            if (WhereTime >= NeedTime)
                WhereTime = 0;
        }

    }


}

// Start is called before the first frame update




