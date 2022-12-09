using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using CommonHelpers;
using LinkCommands.Models;
using LinkCommands.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutocadCommands.Models
{
    public class Wire
    {
        private IEnumerable<ElectricalComponent> _components;

        /// <summary>
        /// If wire not devided by HalfWire, all parts of wire are here
        /// </summary>
        public IEnumerable<Curve> Curves = Enumerable.Empty<Curve>();

        public HalfWire Source { get; set; }
        public HalfWire Destination { get; set; }

        /// <summary>
        /// List of the connected terminals
        /// </summary>
        public List<ComponentTerminal> Terminals = new();

        public Wire(HalfWire halfWire1, HalfWire halfWire2)
        {
            if (halfWire1.IsSource)
            {
                Source = halfWire1;
                Destination = halfWire2;
                return;
            }
            Source = halfWire2;
            Destination = halfWire1;
        }

        public Wire(IEnumerable<Curve> curves)
        {
            Curves= curves;
        }

        public Wire(ObjectId sourceHalfWire, ObjectId destinationHalfWire, IEnumerable<ElectricalComponent> components)
        {
            _components = components;
            var sourceLineEntity = (Entity)sourceHalfWire.GetObject(OpenMode.ForRead, false);
            Source = new HalfWire(sourceLineEntity, components);

            var destinationLineEntity = (Entity)destinationHalfWire.GetObject(OpenMode.ForRead, false);
            Destination = new HalfWire(destinationLineEntity,components);
            
            SetWireAttributes();
        }

        private void SetWireAttributes()
        {
            var SigCode = Guid.NewGuid().ToString();

            Source.SigCode = SigCode;
            Destination.SigCode = SigCode;
            var electricalValidator = new ElectricalValidation(Source.Description, Destination.Description);
            if(!electricalValidator.IsValid)
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("\nWarning! " + electricalValidator.ErrorMessage);
                //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(electricalValidator.ErrorMessage);
            }

            Source.ShortDescription = electricalValidator.ShortName;
            Destination.ShortDescription = Source.ShortDescription;
        }

        public Wire(HalfWire source, HalfWire destination, IEnumerable<ElectricalComponent> components)
        {
            Source = source;
            Destination = destination;
            _components = components;
            SetWireAttributes();
        }

        public void Create()
        {
            Source.CreateSourceLink();
            Destination.CreateDestinationLink();
        }

        public bool IsPointOnEnd(Point3d point)
        {
            if (Curves.Any())
                return GeometryFunc.IsPointOnCurveEnd(point, Curves);
            if(Source != null && Destination != null)
                return Source.IsPointOnEnd(point) || 
                       Destination.IsPointOnEnd(point);
            throw new Exception("Null!");
        }
    }
}
