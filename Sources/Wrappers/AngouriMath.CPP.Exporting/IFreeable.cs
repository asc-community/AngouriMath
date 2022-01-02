//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
