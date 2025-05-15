using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Bricscad.EditorInput;
using System;

namespace CommonHelpers
{
    public abstract class CommandPrototype : IDisposable
    {
        protected readonly Document _doc;
        protected readonly Database _db;
        protected readonly Editor _ed;
        protected readonly Transaction _tr;

        protected CommandPrototype(Document doc)
        {
            _doc = doc;
            _db = _doc.Database;
            _ed = _doc.Editor;
            _tr = _db.TransactionManager.StartTransaction();
        }

        public abstract bool Init();

        public abstract void Run();

        public void Commit()
        {
            _tr.Commit();
        }

        public void Dispose()
        {
            _tr.Dispose();
        }
    }
}