//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LibiadaWeb
{
    using System;
    using System.Collections.Generic;
    
    public partial class RemoteDb
    {
        public RemoteDb()
        {
            this.Sequence = new HashSet<CommonSequence>();
            this.DnaSequence = new HashSet<DnaSequence>();
            this.LiteratureSequence = new HashSet<LiteratureSequence>();
            this.MusicSequence = new HashSet<MusicSequence>();
            this.DataSequence = new HashSet<DataSequence>();
            this.Fmotiv = new HashSet<Fmotiv>();
            this.Measure = new HashSet<Measure>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int NatureId { get; set; }
    
        public virtual ICollection<CommonSequence> Sequence { get; set; }
        public virtual ICollection<DnaSequence> DnaSequence { get; set; }
        public virtual ICollection<LiteratureSequence> LiteratureSequence { get; set; }
        public virtual ICollection<MusicSequence> MusicSequence { get; set; }
        public virtual ICollection<DataSequence> DataSequence { get; set; }
        public virtual ICollection<Fmotiv> Fmotiv { get; set; }
        public virtual ICollection<Measure> Measure { get; set; }
        public virtual Nature Nature { get; set; }
    }
}
