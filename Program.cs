using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using GeoJSON;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;

namespace geojson
{
    class Program
    {
        static void Main(string[] args)
        {
            var pos = new GeoJSON.Net.Geometry.Position(59.264577437894914, 15.27211594029649);
            var kommun = WhereIsPoint(pos);

            Console.WriteLine($"{kommun.Name}, {kommun.Code}");
            Console.WriteLine("Done");
        }

        public class Kommun
        {
            public string Name;
            public string Code;
        }

        public static Kommun WhereIsPoint(Position pos)
        {
           using(WebClient client = new WebClient())
            {
                string json = client.DownloadString("https://gist.githubusercontent.com/Miklar/313c7052ce944b2863748fdde0da6dc3/raw/1aa1a0432d430d431ffe9ef1cabd96f1292f895b/kommuner.geojson");
                var data = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(json);
                
                try
                {
                    foreach(var feat in data.Features)
                    {
                        var poly = (Polygon)feat.Geometry;

                        foreach (var item in poly.Coordinates)
                        {
                            var ring = item.Coordinates.Select(c => (Position)c);
                            var isIn = IsPointInPolygon4(ring.ToArray(), pos);
                            if (isIn)
                            {
                                return new Kommun {
                                    Name = feat.Properties["name"].ToString(),
                                    Code = feat.Properties["code"].ToString()
                                };
                            }
                        }
                    }
                } catch
                {
                    // do nothing and return fallback
                }

                // Fallback if no kommun is found
                return new Kommun {
                    Name = "Stockholms stad",
                    Code = "0180"
                };
            } 
        }

        public static bool IsPointInPolygon4(Position[] polygon, Position testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].Latitude < testPoint.Latitude && polygon[j].Latitude >= testPoint.Latitude || polygon[j].Latitude < testPoint.Latitude && polygon[i].Latitude >= testPoint.Latitude)
                {
                    if (polygon[i].Longitude + (testPoint.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) * (polygon[j].Longitude - polygon[i].Longitude) < testPoint.Longitude)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

    }
}