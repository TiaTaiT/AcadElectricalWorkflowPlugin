global using Autodesk.AutoCAD.ApplicationServices;
global using Autodesk.AutoCAD.Runtime;
global using Autodesk.AutoCAD.DatabaseServices;
global using Autodesk.AutoCAD.EditorInput;
global using Autodesk.AutoCAD.Geometry;

using AutocadCommands.Services;
using LinkCommands.Services;

namespace LinkCommands
{
    public class Commands
    {
        // Link all wires
        [CommandMethod("LINKMULTIWIRES", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void LinkMultiWires()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var wiresLinker = new MultiWiresLinker(doc);
            if (wiresLinker.Init())
            {
                wiresLinker.Run();
            }
            wiresLinker.Commit();
            wiresLinker.Dispose();
        }

        // Link all wires
        [CommandMethod("LINKWIRES", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void LinkWires()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var wiresLinker = new WiresLinker(doc);
            if (wiresLinker.Init())
            {
                wiresLinker.Run();
            }
            wiresLinker.Commit();
            wiresLinker.Dispose();
        }

        // Link all wires
        [CommandMethod("LINKPAIRMULTIWIRES", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void LinkPairMultiWires()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var wiresLinker = new PairMultiWiresLinker(doc);
            if (wiresLinker.Init())
            {
                wiresLinker.Run();
            }
            wiresLinker.Commit();
            wiresLinker.Dispose();
        }

        // Remove all multiwires link simbols
        [CommandMethod("LINKPAIRREMOVE", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void LinkPairRemove()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var wiresLinker = new LinkPairsRemover(doc);
            if (wiresLinker.Init())
            {
                wiresLinker.Run();
            }
            wiresLinker.Commit();
            wiresLinker.Dispose();
        }

        // Remove all wire-links simbols
        [CommandMethod("LINKSREMOVE", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void LinksRemove()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;

            var wiresLinker = new WireLinksRemover(doc);
            if (wiresLinker.Init())
            {
                wiresLinker.Run();
            }
            wiresLinker.Commit();
            wiresLinker.Dispose();
        }
    }
}
