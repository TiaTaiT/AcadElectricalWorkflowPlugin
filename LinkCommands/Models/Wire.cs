using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
        public HalfWire Source { get; set; }
        public HalfWire Destination { get; set; }

        public ObjectId SourceComponentId
        {
            get
            {
                return Source.ComponentId;
            }
        }

        public ObjectId DestinationComponentId
        {
            get
            {
                return Destination.ComponentId;
            }
        }

        
        public Wire(ObjectId sourceHalfWire, ObjectId destinationHalfWire)
        {
            var sourceLineEntity = (Entity)sourceHalfWire.GetObject(OpenMode.ForRead, false);
            Source = new HalfWire(sourceLineEntity);

            var destinationLineEntity = (Entity)destinationHalfWire.GetObject(OpenMode.ForRead, false);
            Destination = new HalfWire(destinationLineEntity);
            
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

            Source.ShortDescription = electricalValidator.ShortName;//NamesConverter.GetShortAlias(Source.Description, Destination.Description);
            Destination.ShortDescription = Source.ShortDescription;
            //ChooseDescription(Source.Description, Destination.Description);
        }

        public Wire(HalfWire source, HalfWire destination)
        {
            Source = source;
            Destination = destination;
            SetWireAttributes();
        }

        public void Create()
        {
            Source.CreateSourceLink();
            Destination.CreateDestinationLink();
        }
    }
}
