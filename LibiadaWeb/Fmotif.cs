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
    
    public partial class Fmotif
    {
        public Fmotif()
        {
            BinaryCharacteristicValue = new HashSet<BinaryCharacteristicValue>();
            CongenericCharacteristicValue = new HashSet<CongenericCharacteristicValue>();
            CharacteristicValue = new HashSet<CharacteristicValue>();
        }
    
        public long Id { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Notation Notation { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public System.DateTimeOffset Modified { get; set; }
        public LibiadaCore.Core.SimpleTypes.FmotifType FmotifType { get; set; }
    
        public virtual ICollection<BinaryCharacteristicValue> BinaryCharacteristicValue { get; set; }
        public virtual ICollection<CongenericCharacteristicValue> CongenericCharacteristicValue { get; set; }
        public virtual ICollection<CharacteristicValue> CharacteristicValue { get; set; }
    }
}
