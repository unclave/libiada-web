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
    
    public partial class BinaryCharacteristic
    {
        public long Id { get; set; }
        public long SequenceId { get; set; }
        public int CharacteristicTypeLinkId { get; set; }
        public double Value { get; set; }
        public string ValueString { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public long FirstElementId { get; set; }
        public long SecondElementId { get; set; }
        public System.DateTimeOffset Modified { get; set; }
    
        public virtual Element FirstElement { get; set; }
        public virtual Element SecondElement { get; set; }
        public virtual CommonSequence Sequence { get; set; }
        public virtual CharacteristicTypeLink CharacteristicTypeLink { get; set; }
    }
}
