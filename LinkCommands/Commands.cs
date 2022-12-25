using AutocadCommands.Services;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
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
        }
    }
}
