using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    /// <summary>
    /// Responsible for native deallocation
    /// </summary>
    public interface IFreeable
    {
        public void Free();
    }
}
