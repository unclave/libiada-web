//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibiadaWeb
{
    using System;
    using System.Collections.Generic;
    
    public partial class product
    {
        public product()
        {
            this.dna_chain = new HashSet<dna_chain>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int piece_type_id { get; set; }
    
        public virtual ICollection<dna_chain> dna_chain { get; set; }
        public virtual piece_type piece_type { get; set; }
    }
}