using RouletteApi.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RouletteApi.Models
{
    [Serializable]
    public class Bet : IValidatableObject
    {
        public nint? Number { get; set; }
        public Color Color { get; set; } = Color.Black;
        public decimal BetValue { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Number != null && Number < 0)
                yield return new("The number for the bet must be between 0 and 36");
            if (0 > BetValue || BetValue >= 10000)
                yield return new("The max bet value is $10.000");
        }
    }
}