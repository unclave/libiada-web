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
    
    public partial class Measure
    {
        public Measure()
        {
            BinaryCharacteristicValue = new HashSet<BinaryCharacteristicValue>();
            CongenericCharacteristicValue = new HashSet<CongenericCharacteristicValue>();
            CharacteristicValue = new HashSet<CharacteristicValue>();
        }
    
        public long Id { get; set; }
        public Notation Notation { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public long MatterId { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Beats { get; set; }
        public int Beatbase { get; set; }
        public Nullable<int> TicksPerBeat { get; set; }
        public int Fifths { get; set; }
        public Nullable<RemoteDb> RemoteDb { get; set; }
        public string RemoteId { get; set; }
        public System.DateTimeOffset Modified { get; set; }
        public bool major { get; set; }
    
        public virtual Matter Matter { get; set; }
        public virtual ICollection<BinaryCharacteristicValue> BinaryCharacteristicValue { get; set; }
        public virtual ICollection<CongenericCharacteristicValue> CongenericCharacteristicValue { get; set; }
        public virtual ICollection<CharacteristicValue> CharacteristicValue { get; set; }
    }
}
