using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Autodesk.AutoCAD.DatabaseServices.Ole2Frame;
using static CommonHelpers.Models.IAutocadDirectionEnum;

namespace CommonHelpers
{
    public static class GeometryFunc
    {
        /// <summary>
        /// Returns true if point argument lies on the segment of 
        /// the given curve between the start and end point
        /// arguments. All three points must lie on the curve.
        /// If the given point argument is coincident with either
        /// the start or end point arguments, the result is true.
        /// The start and end point arguments are interchangable.
        /// </summary>
        /// <param name="curve">The curve (line, arc, polyline, spline, etc.) to test</param>
        /// <param name="point">The candidate test point</param>
        /// <param name="start">The point at one end of the curve segment.</param>
        /// <param name="end">The point at the other end of the curve segment.</param>
        /// <returns>true if the point argument lies on the segment of
        /// the curve between the start and end point arguments</returns>
        private static bool IsOnSegment(Curve curve, Point3d point, Point3d start, Point3d end)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));
            var p = curve.GetParameterAtPoint(point);
            var p1 = curve.GetParameterAtPoint(start);
            var p2 = curve.GetParameterAtPoint(end);
            return p >= Math.Min(p1, p2) && p <= Math.Max(p1, p2);
        }

        private static bool IsOnLine(Line line, Point3d point)
        {
            var segment = new LineSegment3d(line.StartPoint, line.EndPoint);
            if(segment.IsOn(point)) 
                return true;
            return false;
        }

        private static bool IsOnPolyline(Polyline pl, Point3d pt)
        {
            bool isOn = false;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                Curve3d seg = null;

                SegmentType segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);

                if (seg != null)
                {
                    isOn = seg.IsOn(pt);
                    if (isOn)
                        break;
                }
            }
            return isOn;
        }

        public static bool IsPointOnLine<T>(T tLine, Point3d point) where T : Curve
        {
            if(tLine == null)
                return false;
            if(tLine is Line line)
                return IsOnLine(line, point);
            if(tLine is Polyline polyline)
                return IsOnPolyline(polyline, point);
            if(tLine.GetType() == typeof(Curve))
            {
                var curve = (Curve)tLine;
                return IsOnSegment(curve, point, curve.StartPoint, curve.EndPoint);
            }
            return false;
        }

        public static Direction GetDirection(Point3d zeroPoint, Point3d endPoint)
        {
            double angleRad = Math.Atan2(zeroPoint.X - endPoint.X, zeroPoint.Y - endPoint.Y);
            var pi = Math.PI;

            if ((angleRad >= 3 * pi / 4 && angleRad <= pi) || (angleRad >= -pi && angleRad < -3 * pi / 4))
            {
                return Direction.Above;
            }

            if (angleRad >= pi / 4 && angleRad < 3 * pi / 4)
            {
                return Direction.Left;
            }

            if (angleRad <= pi / 4 && angleRad > -pi / 4)
            {
                return Direction.Below;
            }

            if (angleRad <= -pi / 4 && angleRad >= -3 * pi / 4)
            {
                return Direction.Right;
            }

            return Direction.Above;
        }

        public static IEnumerable<Curve> MoveConjugatedCurves(Curve selectedLine, List<Curve> curves, IEnumerable<Point3d> terminators)
        {
            var connectedLines = new List<Curve>
            {
                selectedLine
            };
            
            curves.Remove(selectedLine);

            // For speed up return
            if(curves.Count == 0) 
                return connectedLines;

            Func<Curve, bool> selectedCondition = 
                item => connectedLines
                        .Where(x => (x.StartPoint.Equals(item.StartPoint) && !IsPointTerminated(x.StartPoint, terminators)) ||
                                    (x.EndPoint.Equals(item.StartPoint) && !IsPointTerminated(x.EndPoint, terminators)) ||
                                    (x.StartPoint.Equals(item.EndPoint) && !IsPointTerminated(x.StartPoint, terminators)) ||
                                    (x.EndPoint.Equals(item.EndPoint) && !IsPointTerminated(x.EndPoint, terminators)) ).Any();

            connectedLines.AddRange(curves.Where(selectedCondition));

            curves.RemoveAll(new Predicate<Curve>(selectedCondition));

            return connectedLines;
        }

        /// <summary>
        /// Get collection of all connected lines (autocad type LINE)
        /// </summary>
        /// <param name="db">Autocad draft database</param>
        /// <param name="selectedLineId">selected lines</param>
        /// <param name="layer">layer for search</param>
        /// <returns>All connected lines</returns>
        public static IEnumerable<Curve> GetAllConjugatedCurves(Database db, Curve selectedLine, string layer)
        {
            var AllLinesFromSheet = GetObjectsUtils.GetObjects<Line>(db, layer);

            return GetConjugatedCurves(selectedLine, AllLinesFromSheet);
        }

        public static IEnumerable<Curve> GetConjugatedCurves(Curve selectedLine, IEnumerable<Line> AllLinesFromSheet)
        {
            var connectedLines = new List<Curve>
            {
                selectedLine
            };
            FindAllConjugateCurves(AllLinesFromSheet, connectedLines);
            return connectedLines;
        }

        private static void FindAllConjugateCurves(IEnumerable<Curve> AllLinesFromSheet, List<Curve> connectedLines)
        {
            // This is instead of iterating
            for (var i = 0; i < connectedLines.Count(); i++)
            {
                var line = connectedLines[i];
                IEnumerable<Curve> conjugateLines = GetConjugateLines(line, AllLinesFromSheet);
                if (!conjugateLines.Any())
                    continue;
                foreach (var conjugateLine in conjugateLines)
                {
                    if (!connectedLines.Contains(conjugateLine))
                        connectedLines.Add(conjugateLine);
                }
            }
        }

        private static IEnumerable<Curve> GetConjugateLines(Curve currentMultiWireLine, IEnumerable<Curve> linesFromSheet)
        {
            foreach (var lineFromSheet in linesFromSheet)
            {
                var line = (Line)lineFromSheet;

                var IsStartPointOnAnotherLine = IsPointOnLine(line, ((Line)currentMultiWireLine).StartPoint);
                var IsEndPointOnAnotherLine = IsPointOnLine(line, ((Line)currentMultiWireLine).EndPoint);

                if (IsStartPointOnAnotherLine || IsEndPointOnAnotherLine)
                    yield return line;
            }

        }

        /*
        /// <summary>
        /// Function for searching for conjugated with selected curves
        /// </summary>
        /// <param name="selectedCurve">Selected curve/line</param>
        /// <param name="candidateCurves">List with candidates to conjugated with selected line</param>
        /// <param name="terminators">Stop points where search should be stop</param>
        /// <returns></returns>
        public static IEnumerable<Curve> GetConjugatedCurves(Curve selectedCurve,
                                                             IEnumerable<Curve> candidateCurves,
                                                             IEnumerable<Point3d> terminators)
        {
            var result = new List<Curve>() 
            { 
                selectedCurve 
            };

            var curves = candidateCurves.Where(c => c != selectedCurve);
            
            foreach (var curve in curves)
            {
                for(var i = 0; i < result.Count; i++)
                {
                    var candidate = result[i];
                    if(!IsPointTerminated(candidate.StartPoint, terminators))
                    {
                        if (IsCurveConjugated(curve.StartPoint, candidate))
                        {
                            if(!result.Contains(candidate))
                                result.Add(candidate);
                        }
                    }
                    if (!IsPointTerminated(candidate.EndPoint, terminators))
                    {
                        if (IsCurveConjugated(curve.EndPoint, candidate))
                        {
                            if (!result.Contains(candidate))
                                result.Add(candidate);
                        }
                    }
                }
            }
            return result;
        }

        private static bool IsCurveConjugated(Point3d point, Curve candidate)
        {
            return candidate.StartPoint.Equals(point) || candidate.EndPoint.Equals(point);
        }
*/
        private static bool IsPointTerminated(Point3d point, IEnumerable<Point3d> terminators)
        {
            foreach(var terminator in terminators)
            {
                if(point.Equals(terminator))
                    return true;
            }
            return false;
        }
        
    }
}
