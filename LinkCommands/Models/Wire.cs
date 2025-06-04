using CommonHelpers;
using LinkCommands.Models;
using LinkCommands.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutocadCommands.Models
{
    public class Wire
    {
        private IEnumerable<ElectricalComponent> _components;
        private readonly DesignationParser _designationParser;
        private readonly NamesConverter _namesConverter;
        private readonly Transaction _tr;

        /// <summary>
        /// If wire not devided by HalfWire, all parts of wire are here
        /// </summary>
        public IEnumerable<Curve> Curves = Enumerable.Empty<Curve>();
        public HalfWire Source { get; set; }
        public HalfWire Destination { get; set; }

        private string _descritption;
        public string Description
        {
            get
            {
                if (Source == null || Destination == null)
                    return _descritption;
                return Source.Description.Equals(Destination.Description) ? Source.Description : _descritption;
            }
            set => _descritption = value;
        }

        /// <summary>
        /// List of the connected terminals
        /// </summary>
        public List<ComponentTerminal> Terminals = new();

        public Wire(HalfWire halfWire1, HalfWire halfWire2)
        {
            _designationParser = new DesignationParser();
            _namesConverter = new NamesConverter();
            if (halfWire1.IsSource)
            {
                Source = halfWire1;
                Destination = halfWire2;
                return;
            }
            Source = halfWire2;
            Destination = halfWire1;
        }

        public Wire(Transaction tr, IEnumerable<Curve> curves)
        {
            _tr = tr;
            Curves = curves;
            _designationParser = new DesignationParser();
            _namesConverter = new NamesConverter();
        }

        public Wire(Transaction tr, ObjectId sourceHalfWire, ObjectId destinationHalfWire, IEnumerable<ElectricalComponent> components)
        {
            _tr = tr;
            _designationParser = new DesignationParser();
            _namesConverter = new NamesConverter();
            _components = components;
            var sourceLineEntity = (Entity)sourceHalfWire.GetObject(OpenMode.ForRead, false);
            Source = new HalfWire(_tr, sourceLineEntity, components);

            var destinationLineEntity = (Entity)destinationHalfWire.GetObject(OpenMode.ForRead, false);
            Destination = new HalfWire(_tr, destinationLineEntity, components);

            SetWireAttributes();
        }

        private void SetWireAttributes()
        {
            var SigCode = Guid.NewGuid().ToString();

            Source.SigCode = SigCode;
            Destination.SigCode = SigCode;
            if (string.IsNullOrEmpty(Source.ShortDescription) || string.IsNullOrEmpty(Destination.ShortDescription))
            {
                var electricalValidator = new ElectricalValidation(_designationParser, _namesConverter);
                var validationResult = electricalValidator.IsConnectionValid(Source.Description, Destination.Description);
                if (!validationResult)
                {
                    Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                    ed.WriteMessage("\nWarning! " + electricalValidator.ErrorMessage);
                    //Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(electricalValidator.ErrorMessage);
                }

                Source.ShortDescription = electricalValidator.ShortName;
            }

            Destination.ShortDescription = Source.ShortDescription;
        }

        public Wire(Transaction tr, HalfWire source, HalfWire destination, IEnumerable<ElectricalComponent> components)
        {
            _tr = tr;
            Source = source;
            Destination = destination;
            _components = components;
            _designationParser = new DesignationParser();
            _namesConverter = new NamesConverter();
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
            if (Source != null && Destination != null)
                return Source.IsPointOnEnd(point) ||
                       Destination.IsPointOnEnd(point);
            throw new System.Exception("Null!");
        }
    }
}
