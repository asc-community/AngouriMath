namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        internal struct ObjRef
        {
            private readonly ulong handle;
            public ObjRef(ulong handle)
                => this.handle = handle;
            public ObjRef Next()
                => new ObjRef(handle + 1);
            public Entity AsEntity => ObjStorage<Entity>.Get(this);
            public static implicit operator ObjRef(Entity entity)
                => ObjStorage<Entity>.Alloc(entity);
        }
    }
}
