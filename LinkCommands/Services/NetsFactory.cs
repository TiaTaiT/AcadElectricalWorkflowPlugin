using AutocadCommands.Models;
using AutocadCommands.Services;
using Autodesk.AutoCAD.DatabaseServices;
using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkCommands.Services
{
    public class NetsFactory
    {
        private readonly Database _db;

        public NetsFactory(Database db) 
        {
            _db = db;
            FindElectricalNets();
        }

        private void FindElectricalNets()
        {
            var curves = LinkerHelper.GetAllWiresFromDb(_db);
            CreateConjugatedCurves(curves);
        }

        private void CreateConjugatedCurves(IEnumerable<Curve> allCurves)
        {
            var curves = allCurves.ToList();
            var CurveGroups = new List<List<Curve>>();
            while (curves.Any())
            {
                var curve = curves.First();
                CurveGroups.Add(GeometryFunc.MoveConjugatedCurves(curve, curves).ToList());
                Debug.WriteLine("Remained in the collection: " + curves.Count);
            }
        }

        public IEnumerable<Wire> GetAllExistWires()
        { 
            throw new NotImplementedException(); 
        }
    }
}
