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
    
    public partial class AccordanceCharacteristicValue
    {
        public long Id { get; set; }
        public long FirstSequenceId { get; set; }
        public long SecondSequenceId { get; set; }
        public short CharacteristicTypeLinkId { get; set; }
        public double Value { get; set; }
        public System.DateTimeOffset Created { get; set; }
        public long FirstElementId { get; set; }
        public long SecondElementId { get; set; }
        public System.DateTimeOffset Modified { get; set; }
    
        public virtual Element FirstElement { get; set; }
        public virtual Element SecondElement { get; set; }
        public virtual CommonSequence FirstSequence { get; set; }
        public virtual CommonSequence SecondSequence { get; set; }
        public virtual AccordanceCharacteristicLink AccordanceCharacteristicLink { get; set; }
    }
}
