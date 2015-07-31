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
    
    public partial class Feature
    {
        public Feature()
        {
            Sequence = new HashSet<CommonSequence>();
            DnaSequence = new HashSet<DnaSequence>();
            LiteratureSequence = new HashSet<LiteratureSequence>();
            Fmotiv = new HashSet<Fmotiv>();
            Measure = new HashSet<Measure>();
            MusicSequence = new HashSet<MusicSequence>();
            DataSequence = new HashSet<DataSequence>();
            Subsequence = new HashSet<Subsequence>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NatureId { get; set; }
        public string Type { get; set; }
        public bool Complete { get; set; }
    
        public virtual ICollection<CommonSequence> Sequence { get; set; }
        public virtual ICollection<DnaSequence> DnaSequence { get; set; }
        public virtual ICollection<LiteratureSequence> LiteratureSequence { get; set; }
        public virtual Nature Nature { get; set; }
        public virtual ICollection<Fmotiv> Fmotiv { get; set; }
        public virtual ICollection<Measure> Measure { get; set; }
        public virtual ICollection<MusicSequence> MusicSequence { get; set; }
        public virtual ICollection<DataSequence> DataSequence { get; set; }
        public virtual ICollection<Subsequence> Subsequence { get; set; }
    }
}
