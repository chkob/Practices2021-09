using AlchemyCirclesGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AlchemyCirclesGenerator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GeneratorController : BaseController
    {
        private readonly ILogger<GeneratorController> _logger;

        public GeneratorController(ILogger<GeneratorController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Get([FromQuery] CallingParam param)
        {
            if (param == null) return BadRequest();
            if (param.ImageBlockSize <= 0) return BadRequest();

            var imageBlockSize = param.ImageBlockSize;

            if (imageBlockSize < 64) imageBlockSize = 64;

            var now = DateTime.Now;
            var unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var nowTimestamp = (DateTime.UtcNow.Subtract(new DateTime(now.Year, 1, 1))).TotalSeconds;
            var nowMillisecs = now.Millisecond;

            var rng = new Random(nowMillisecs);

            var pixelsize = 2;
            var size = imageBlockSize * pixelsize;
            var width = size;
            var height = size;

            using (var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    //var whiteColor = Color.FromArgb(255, 255, 255, 255);
                    var penColor = Color.FromArgb(255, 255, 255, 255);

                    if (param.Colored)
                    {
                        var red = rng.Next(60, 255);
                        var green = rng.Next(60, 255);
                        var blue = rng.Next(60, 255);

                        penColor = Color.FromArgb(255, red, green, blue);
                    }

                    var backgroundColor = Color.FromArgb(255, 30, 30, 30);

                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(backgroundColor);

                    var centerX = width / 2; //for readability
                    var centerY = height / 2; //for readability
                    var radius = ((Math.Min(width, height) / 2) * 3) / 4;

                    // draw a full circle in the foreground color
                    var pen = new Pen(penColor, 1.0f);
                    g.DrawArc(pen, 0, 0, width, height, 0.0f, 360.0f);

                    // draw a n-sided polygon with n between 3 and 13
                    var lati = rng.Next(3, 13);
                    var polygon1 = CreatePolygonPoints(lati, 0, radius, Math.Min(width, height));
                    g.DrawPolygon(pen, polygon1.Select(p => new PointF(p.X, p.Y)).ToArray());

                    // draw lines from center to each of vervices
                    for (var l1 = 0; l1 < lati; l1++)
                    {
                        var ang = ConvertDegreesToRadians((360.0 / (lati))) * l1;
                        g.DrawLine(pen, new PointF(centerX, centerY), new PointF((float)(centerX + radius * Math.Cos(ang)), (float)(centerY + radius * Math.Sin(ang))));
                    }

                    int latis = 0;

                    // if polygon has even number of sides
                    if (lati % 2 == 0)
                    {
                        // generate 2 with a probability of 1/5, or something from {4, 6} with probability 2/5 each

                        latis = rng.Next(2, 6);
                        while(latis % 2 != 0) latis = rng.Next(3, 6);

                        var colorBrush = new SolidBrush(backgroundColor);
                        var polygon2 = CreatePolygonPoints(latis, 180, radius, Math.Min(width, height));
                        var drawingPolygon = polygon2.Select(p => new PointF(p.X, p.Y)).ToArray();
                        g.FillPolygon(colorBrush, drawingPolygon);
                        g.DrawPolygon(pen, drawingPolygon);

                        for (var l2 = 0; l2 < lati; l2++)
                        {
                            var ang = ConvertDegreesToRadians((360.0 / (latis))) * l2;
                            g.DrawLine(pen, new PointF(centerX, centerY), new PointF((float)(centerX + radius * Math.Cos(ang)), (float)(centerY + radius * Math.Sin(ang))));
                        }
                    }
                    else
                    {
                        // generate random number from the set {4, 6} by generating 2 or 3 and multiplying it by 2
                        latis = rng.Next(2, 3)*2;

                        var colorBrush = new SolidBrush(backgroundColor);
                        var polygon2 = CreatePolygonPoints(latis, 180, radius, Math.Min(width, height));
                        var drawingPolygon = polygon2.Select(p => new PointF(p.X, p.Y)).ToArray();
                        g.FillPolygon(colorBrush, drawingPolygon);
                        g.DrawPolygon(pen, drawingPolygon);
                    }

                    // with a 50% chance:
                    if (rng.Next(0, 1) % 2 == 0)
                    {
                        var ronad = rng.Next(0, 4);

                        // some trigonometry magic happens below
                        if (ronad%2 == 1)
                        {
                            for (var l3 = 0; l3 < lati + 4; l3++)
                            {
                                var ang = ConvertDegreesToRadians((360.0 / (lati + 4))) * l3;
                                g.DrawLine(pen, new PointF(centerX, centerY), new PointF((float)(centerX + (((radius / 8) * 5) + 2) * Math.Cos(ang)), (float)(centerY + (((radius / 8) * 5) + 2) * Math.Sin(ang))));
                            }

                            var colorBrush = new SolidBrush(backgroundColor);
                            var polygon3 = CreatePolygonPoints(lati + 4, 180, radius / 2, Math.Min(width, height));
                            var drawingPolygon = polygon3.Select(p => new PointF(p.X, p.Y)).ToArray();
                            g.FillPolygon(colorBrush, drawingPolygon);
                            g.DrawPolygon(pen, drawingPolygon);
                        }
                        else if (ronad % 2 == 0 && lati > 5)
                        {
                            for (var l3 = 0; l3 < lati - 2; l3++)
                            {
                                var ang = ConvertDegreesToRadians((360.0 / (lati - 2))) * l3;
                                g.DrawLine(pen, new PointF(centerX, centerY), new PointF((float)(centerX + (((radius / 8) * 5) + 2) * Math.Cos(ang)), (float)(centerY + (((radius / 8) * 5) + 2) * Math.Sin(ang))));
                            }

                            var colorBrush = new SolidBrush(backgroundColor);
                            var polygon3 = CreatePolygonPoints(lati - 2, 180, radius / 4, Math.Min(width, height));
                            var drawingPolygon = polygon3.Select(p => new PointF(p.X, p.Y)).ToArray();
                            g.FillPolygon(colorBrush, drawingPolygon);
                            g.DrawPolygon(pen, drawingPolygon);
                        }
                    }

                    // with a 60% chance:
                    if (rng.Next(0, 4) % 2 == 0)
                    {
                        var offsetX = (radius / 8) * 11;
                        var offsetY = (radius / 8) * 11;
                        g.DrawArc(pen, centerX - (offsetX / 2), centerY - (offsetY / 2), offsetX, offsetY, 0.0f, 360.0f);

                        if (lati % 2 == 0)
                        {
                            // generate 2 with a probability of 1/7, or something from {4, 6, 8} with probability 2/7 each
                            latis = rng.Next(2, 8);
                            while (latis % 2 != 0) latis = rng.Next(3, 8);

                            var polygon4 = CreatePolygonPoints(latis, 180, (radius / 3) * 2, Math.Min(width, height));
                            var drawingPolygon = polygon4.Select(p => new PointF(p.X, p.Y)).ToArray();
                            g.DrawPolygon(pen, drawingPolygon);
                        }
                        else
                        {
                            // generate random number from the set {3, 5, 7} by calculating 2*x+1, where x is in {1, 2, 3}
                            latis = rng.Next(1, 3) + 1;

                            var polygon4 = CreatePolygonPoints(latis, 180, (radius / 3) * 2, Math.Min(width, height));
                            var drawingPolygon = polygon4.Select(p => new PointF(p.X, p.Y)).ToArray();
                            g.DrawPolygon(pen, drawingPolygon);
                        }
                    }

                    var numSatellite = GetNumSatellite(lati, rng.Next(0, 2), rng.Next(2, 7));

                    if (param.DrawInnerCircles) 
                    {
                        var colorBrush = new SolidBrush(backgroundColor);
                        var radiusOffset1 = (radius / 18) * 12;
                        var radiusOffset2 = (radius / 22) * 12;

                        g.DrawArc(pen, centerX - radiusOffset1 / 2, centerY - radiusOffset1 / 2, radiusOffset1, radiusOffset1, 0.0f, 360.0f);
                        g.FillPie(colorBrush, centerX - radiusOffset2 / 2, centerY - radiusOffset2 / 2, radiusOffset2, radiusOffset2, 0.0f, 360.0f);
                        g.DrawArc(pen, centerX - radiusOffset2 / 2, centerY - radiusOffset2 / 2, radiusOffset2, radiusOffset2, 0.0f, 360.0f);
                    }

                    var satelliteOption = rng.Next(0, 4);

                    _logger.LogInformation($"numSatellite: {numSatellite}, satelliteOption: {satelliteOption}");

                    switch (satelliteOption)
                    {
                        case 0:
                            {
                                var centers = GetSatelliteCenter(numSatellite, radius, 18, 11);

                                if (param.DrawSatellite)
                                {
                                    var colorBrush = new SolidBrush(backgroundColor);
                                    var radiusOffset = (radius / 44.0f) * 12.0f;

                                    var red = rng.Next(60, 255);
                                    var green = rng.Next(60, 255);
                                    var blue = rng.Next(60, 255);

                                    var penColor1 = Color.FromArgb(255, red, green, blue);
                                    var pen0 = new Pen(penColor1, 1.0f);

                                    for (var i = 0; i < centers.Count; i++)
                                    {
                                        var bound = new Rectangle(
                                            new Point()
                                            {
                                                X = (int)(centerX + centers[i].X - (radiusOffset / 2.0f)),
                                                Y = (int)(centerY + centers[i].Y - (radiusOffset / 2.0f))
                                            },
                                            new Size()
                                            {
                                                Width = (int)radiusOffset,
                                                Height = (int)radiusOffset
                                            });
                                        g.FillPie(colorBrush, bound, 0.0f, 360.0f);
                                        g.DrawArc(pen0, bound, 0.0f, 360.0f);
                                    }
                                }

                                if (param.DrawLineToSatellite)
                                {
                                    var red = rng.Next(60, 255);
                                    var green = rng.Next(60, 255);
                                    var blue = rng.Next(60, 255);

                                    var penColor1 = Color.FromArgb(255, red, green, blue);
                                    var pen0 = new Pen(penColor1, 1.0f);

                                    for (var i = 0; i < centers.Count; i++)
                                    {
                                        g.DrawLine(pen0,
                                            new PointF()
                                            {
                                                X = centerX,
                                                Y = centerY
                                            },
                                            new PointF()
                                            {
                                                X = (float)(centerX + centers[i].X),
                                                Y = (float)(centerY + centers[i].Y)
                                            });
                                    }
                                }
                            }
                            break;
                        case 1:
                            {
                                var centers = GetSatelliteCenter(numSatellite, radius, 1, 1);

                                if (param.DrawSatellite)
                                {
                                    var colorBrush = new SolidBrush(backgroundColor);
                                    var radiusOffset = (radius / 44.0f) * 12.0f;

                                    var red = rng.Next(60, 255);
                                    var green = rng.Next(60, 255);
                                    var blue = rng.Next(60, 255);

                                    var penColor1 = Color.FromArgb(255, red, green, blue);
                                    var pen0 = new Pen(penColor1, 1.0f);

                                    for (var i = 0; i < centers.Count; i++)
                                    {
                                        var bound = new Rectangle(
                                            new Point()
                                            {
                                                X = (int)(centerX + centers[i].X - (radiusOffset / 2.0f)),
                                                Y = (int)(centerY + centers[i].Y - (radiusOffset / 2.0f))
                                            },
                                            new Size()
                                            {
                                                Width = (int)radiusOffset,
                                                Height = (int)radiusOffset
                                            });
                                        g.FillPie(colorBrush, bound, 0.0f, 360.0f);
                                        g.DrawArc(pen0, bound, 0.0f, 360.0f);
                                    }
                                }

                                if (param.DrawLineToSatellite)
                                {
                                    var red = rng.Next(60, 255);
                                    var green = rng.Next(60, 255);
                                    var blue = rng.Next(60, 255);

                                    var penColor1 = Color.FromArgb(255, red, green, blue);
                                    var pen0 = new Pen(penColor1, 1.0f);

                                    for (var i = 0; i < centers.Count; i++)
                                    {
                                        g.DrawLine(pen0,
                                            new PointF()
                                            {
                                                X = centerX,
                                                Y = centerY
                                            },
                                            new PointF()
                                            {
                                                X = (float)(centerX + centers[i].X),
                                                Y = (float)(centerY + centers[i].Y)
                                            });
                                    }
                                }
                            }
                            break;
                        case 2:
                            break;
                        case 3:
                        default:
                            {
                                var red = rng.Next(60, 255);
                                var green = rng.Next(60, 255);
                                var blue = rng.Next(60, 255);

                                var penColor1 = Color.FromArgb(255, red, green, blue);
                                var pen3 = new Pen(penColor1, 1.0f);

                                for (var l4 = 0; l4 < numSatellite; l4++)
                                {
                                    var ang = ConvertDegreesToRadians((360.0 / numSatellite)) * l4;
                                    g.DrawLine(pen3,
                                        new PointF((float)(centerX + (radius / 3) * 2 * Math.Cos(ang)), (float)(centerY + (radius / 3) * 2 * Math.Sin(ang))),
                                        new PointF((float)(centerX + radius * Math.Cos(ang)), (float)(centerY + radius * Math.Sin(ang))));
                                }

                                if (numSatellite != lati)
                                {
                                    var colorBrush = new SolidBrush(backgroundColor);
                                    var radiusOffset1 = (radius / 3) * 4;
                                    g.FillPie(colorBrush, centerX - radiusOffset1 / 2, centerY - radiusOffset1 / 2, radiusOffset1, radiusOffset1, 0.0f, 360.0f);
                                    g.DrawArc(pen, centerX - radiusOffset1 / 2, centerY - radiusOffset1 / 2, radiusOffset1, radiusOffset1, 0.0f, 360.0f);
                                    lati = rng.Next(3, 17);
                                    var polygon5 = CreatePolygonPoints(numSatellite, 0, (radius / 4) * 5, Math.Min(width, height));
                                    var polygon6 = CreatePolygonPoints(numSatellite, 180, (radius / 3) * 2, Math.Min(width, height));
                                    var drawingPolygon5 = polygon5.Select(p => new PointF(p.X, p.Y)).ToArray();
                                    var drawingPolygon6 = polygon6.Select(p => new PointF(p.X, p.Y)).ToArray();
                                    g.DrawPolygon(pen, drawingPolygon5);
                                    g.DrawPolygon(pen, drawingPolygon6);
                                }
                            }
                            break;
                    }

                    if (param.ForceBGTransparent)
                    {
                        bmp.MakeTransparent(backgroundColor);
                    }
                }

                var ms = new MemoryStream();

                bmp.Save(ms, ImageFormat.Png);

                return new FileContentResult(ms.ToArray(), "image/png");
            }
        }

        private IList<PolygonPoint> CreatePolygonPoints(int numEdges, double rotAngleDeg, double radius, int size)
        {
            var angdiff = ConvertDegreesToRadians(360.0 / (numEdges * 2.0));
            var rotAngleRad = ConvertDegreesToRadians(rotAngleDeg);

            var returnPoints = new List<PolygonPoint>();
            for (var i = 0; i < numEdges * 2; i++)
            {
                returnPoints.Add(new PolygonPoint()
                {
                    X = (float)((size / 2) + radius * Math.Cos(i * angdiff + rotAngleRad)),
                    Y = (float)((size / 2) + radius * Math.Sin(i * angdiff + rotAngleRad))
                });
            }

            return returnPoints;
        }

        private IList<PolygonPoint> GetSatelliteCenter(int numSatellite, float radius, int factor1, int factor2)
        {
            var ang = ConvertDegreesToRadians((360.0 / (float)numSatellite), true);

            if (factor1 <= 0) factor1 = 2;

            var returnPoints = new List<PolygonPoint>();

            for (var i = 0; i < numSatellite; i++)
            {
                returnPoints.Add(new PolygonPoint()
                {
                    X = (float)((radius / factor1) * factor2 * Math.Cos((double)(i * ang))),
                    Y = (float)((radius / factor1) * factor2 * Math.Sin((double)(i * ang)))
                });
            }

            return returnPoints;
        }

        private static double ConvertDegreesToRadians(double degrees, bool useLocalPi = false)
        {
            var pi = useLocalPi ? 4.0 * Math.Atan(1.0) : Math.PI;
            var radians = (pi / 180.0) * degrees;
            return radians;
        }

        private static int GetNumSatellite(int numEdge, int random1, int random2)
        {
            if (numEdge % 2 == 0)
            {
                if (random1 == 1)
                {
                    return numEdge * 2;
                }
                else
                {
                    return numEdge;
                }
            }
            else
            {
                if (random2 == 3 || random2 == 4)
                {
                    return numEdge;
                }
                else
                {
                    return (2 * numEdge);
                }
            }
        }
    }
}
