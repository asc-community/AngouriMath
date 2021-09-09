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