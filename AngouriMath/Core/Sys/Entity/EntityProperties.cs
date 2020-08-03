using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Core.Sys
{
    internal class EntityProperties
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
        private readonly AngouriMath.Entity parent;

        public EntityProperties(AngouriMath.Entity parent)
        {
            this.parent = parent;
        }

        public T GetProperty<T>(string name, Func<AngouriMath.Entity, T> definition) where T : class
        {
            if (!properties.ContainsKey(name))
                properties[name] = definition(parent);
            return (T)properties[name];
        }

        public T GetProperty<T>(string name, params Func<AngouriMath.Entity, T>[] definition) where T : struct
        {
            if (!properties.ContainsKey(name))
                properties[name] = definition[0](parent);
            return (T)properties[name];
        }

        public bool GetPropIsFinite()
        {
            var res = GetProperty("IsFinite",
                enf => enf.ChildrenReadonly.All(c => !(c is NumberEntity { Value: { IsFinite: false } })));
            return res;
        }

        public int GetPropComplexity()
        {
            var res = GetProperty("Complexity", 
                enf => enf.ChildrenReadonly.Select(c => c.Complexity()).Sum());
            return res;
        }

        public int GetPropHashCode()
        {
            var res = GetProperty("Complexity", 
                enf => enf.ChildrenReadonly.Select(c => c.GetHashCode()).Sum());
            return res;
        }

        public bool GetPropHasVar(string varName)
        {
            var res = GetProperty("HasVar_" + varName, enf =>
            {
                foreach (var el in enf.ChildrenReadonly)
                    if (el.HasVar(varName))
                        return true;
                return false;
            });
            return res;
        }
    }
}
