
namespace GMap.NET.WindowsForms
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.Serialization;
    using GMap.NET;
    using System.Windows.Forms;
    using System;

    /// <summary>
    /// GMap.NET polygon
    /// </summary>
    [System.Serializable]
#if !PocketPC
    public class GMapPolygon : ISerializable, IDeserializationCallback, IDisposable
#else
   public class GMapPolygon : MapRoute, IDisposable
#endif
    {
        private bool visible = true;

        public string Name;

        public object Tag;

        // Polygons shouldn't need these
        public string Duration; 

        public List<string> Instructions = new List<string>();

        public RouteStatusCode Status { get; set; }

        public string ErrorMessage { get; set; }

        public int ErrorCode { get; set; }

        public string WarningMessage { get; set; }

        /// <summary>
        /// is polygon visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return visible;
            }
            set
            {
                if (value != visible)
                {
                    visible = value;

                    if (Overlay != null && Overlay.Control != null)
                    {
                        if (visible)
                        {
                            Overlay.Control.UpdatePolygonLocalPosition(this);
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverPolygon)
                            {
                                Overlay.Control.IsMouseOverPolygon = false;
#if !PocketPC
                                Overlay.Control.RestoreCursorOnLeave();
#endif
                            }
                        }

                        {
                            if (!Overlay.Control.HoldInvalidation)
                            {
                                Overlay.Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// can receive input
        /// </summary>
        public bool IsHitTestVisible = false;

        private bool isMouseOver = false;

        /// <summary>
        /// is mouse over
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return isMouseOver;
            }
            internal set
            {
                isMouseOver = value;
            }
        }

        GMapOverlay overlay;
        public GMapOverlay Overlay
        {
            get
            {
                return overlay;
            }
            internal set
            {
                overlay = value;
            }
        }

#if !PocketPC
        /// <summary>
        /// Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsInsideLocal(int x, int y)
        {
            if (graphicsPath != null)
            {
                return graphicsPath.IsVisible(x, y);
            }

            return false;
        }

        GraphicsPath graphicsPath;
        internal void UpdateGraphicsPath()
        {
            if (graphicsPath == null)
            {
                graphicsPath = new GraphicsPath();
            }
            else
            {
                graphicsPath.Reset();
            }

            {
                foreach (List<GPoint> localPolygon in LocalPolygons)
                {
                    Point[] pnts = new Point[localPolygon.Count];

                    for (int i = 0; i < localPolygon.Count; i++)
                    {
                        Point p2 = new Point((int)localPolygon[i].X, (int)localPolygon[i].Y);
                        pnts[pnts.Length - 1 - i] = p2;
                    }

                    if (pnts.Length > 2)
                    {
                        graphicsPath.AddPolygon(pnts);
                    }
                    else if (pnts.Length == 2)
                    {
                        graphicsPath.AddLines(pnts);
                    }
                }
            }
        }
#endif


        public virtual void OnRender(Graphics g)
        {
#if !PocketPC
            if (IsVisible)
            {
                if (IsVisible)
                {
                    if (graphicsPath != null)
                    {
                        g.FillPath(Fill, graphicsPath);
                        g.DrawPath(Stroke, graphicsPath);
                    }
                }
            }
#else
         {
            if(IsVisible)
            {
               Point[] pnts = new Point[LocalPoints.Count];
               for(int i = 0; i < LocalPoints.Count; i++)
               {
                  Point p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
                  pnts[pnts.Length - 1 - i] = p2;
               }

               if(pnts.Length > 1)
               {
                  g.FillPolygon(Fill, pnts);
                  g.DrawPolygon(Stroke, pnts);
               }
            }
         }
#endif
        }

        //public double Area
        //{
        //   get
        //   {
        //      return 0;
        //   }
        //}

#if !PocketPC
        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));
#else
      public static readonly Pen DefaultStroke = new Pen(Color.MidnightBlue);
#endif

        /// <summary>
        /// specifies how the outline is painted
        /// </summary>
        [NonSerialized]
        public Pen Stroke = DefaultStroke;

#if !PocketPC
        public static readonly Brush DefaultFill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));
#else
      public static readonly Brush DefaultFill = new System.Drawing.SolidBrush(Color.AliceBlue);
#endif

        /// <summary>
        /// background color
        /// </summary>
        [NonSerialized]
        public Brush Fill = DefaultFill;

        public List<GPoint> LocalPoints
        {
            get
            {
                return LocalPolygons[0];
            }
        }

        public readonly List<List<GPoint>> LocalPolygons = new List<List<GPoint>>();

        public List<List<PointLatLng>> Polygons = new List<List<PointLatLng>>();

        public List<PointLatLng> Points
        {
            get
            {
                return Polygons[0];
            }
        }

        static GMapPolygon()
        {
#if !PocketPC
            DefaultStroke.LineJoin = LineJoin.Round;
#endif
            DefaultStroke.Width = 5;
        }

        public GMapPolygon(List<PointLatLng> points, string name)
        {
            Name = name;

            Polygons.Add(new List<PointLatLng>());
            Polygons[0].AddRange(points);
            LocalPolygons.Add(new List<GPoint>());
            LocalPolygons[0].Capacity = points.Count;
        }

        public GMapPolygon(List<List<PointLatLng>> polygons, string name)
        {
            Name = name;

            Polygons.AddRange(polygons);
            for (int i = 0; i < polygons.Count; i++)
            {
                LocalPolygons.Add(new List<GPoint>());
                LocalPolygons[i].Capacity = polygons[i].Count;
            }
        }

        /// <summary>
        /// checks if point is inside the polygon,
        /// info.: http://greatmaps.codeplex.com/discussions/279437#post700449
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsInside(PointLatLng p)
        {
            List<PointLatLng> outerPolygon = Polygons[0];

            bool result = isInside(p, outerPolygon);

            for (int i = 1; i < Polygons.Count; i++)
            {
                result = result ^ isInside(p, Polygons[i]);
            }

            return result;
        }

        private bool isInside(PointLatLng p, List<PointLatLng> polygon)
        {
            int count = polygon.Count;

            if (count < 3)
            {
                return false;
            }

            bool result = false;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[j];

                if (p1.Lat < p.Lat && p2.Lat >= p.Lat || p2.Lat < p.Lat && p1.Lat >= p.Lat)
                {
                    if (p1.Lng + (p.Lat - p1.Lat) / (p2.Lat - p1.Lat) * (p2.Lng - p1.Lng) < p.Lng)
                    {
                        result = !result;
                    }
                }
                j = i;
            }

            return result;
        }

#if !PocketPC
        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Tag", Tag);
            info.AddValue("Polygons", Polygons.ToArray());

            info.AddValue("LocalPoints", this.LocalPoints.ToArray());
            info.AddValue("Visible", this.IsVisible);
        }

        // Temp store for de-serialization.
        private GPoint[] deserializedLocalPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapRoute"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapPolygon(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            Tag = Extensions.GetValue<object>(info, "Tag", null);

            this.deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
            this.IsVisible = Extensions.GetStruct<bool>(info, "Visible", true);
        }

        #endregion

        #region IDeserializationCallback Members

        /// <summary>
        /// Runs when the entire object graph has been de-serialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
        public void OnDeserialization(object sender)
        {

            // Accounts for the de-serialization being breadth first rather than depth first.
            // LocalPoints.AddRange(deserializedLocalPoints);
            // LocalPoints.Capacity = Points.Count;
        }

        #endregion
#endif

        #region IDisposable Members

        bool disposed = false;

        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                
                foreach (List<PointLatLng> polygon in Polygons)
                {
                    polygon.Clear();
                }

                Polygons.Clear();

                foreach (List<GPoint> localPolygon in LocalPolygons)
                {
                    localPolygon.Clear();
                }

                LocalPolygons.Clear();

#if !PocketPC
                if (graphicsPath != null)
                {
                    graphicsPath.Dispose();
                    graphicsPath = null;
                }
#endif
            }
        }

        #endregion
    }

    public delegate void PolygonClick(GMapPolygon item, MouseEventArgs e);
    public delegate void PolygonDoubleClick(GMapPolygon item, MouseEventArgs e);
    public delegate void PolygonEnter(GMapPolygon item);
    public delegate void PolygonLeave(GMapPolygon item);
}
