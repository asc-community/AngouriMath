//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Variable
        {
            internal override string ToSymPy() 
                => Name switch
                {
                    "e" => "sympy.E",
                    "pi" => "sympy.pi",
                    _ => Name
                };
        }
    }
}