using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;

namespace CommonHelpers
{
    public abstract class CommandPrototype
    {
        protected readonly Document _doc;
        protected readonly Database _db;
        protected readonly Editor _ed;

        protected CommandPrototype(Document doc)
        {
            _doc = doc;
            _db = _doc.Database;
            _ed = _doc.Editor;
        }

        public abstract bool Init();

        public abstract void Run();
    }
}