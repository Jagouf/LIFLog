using LIFLog.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFLog.ViewModel
{
    public class Hit : IEquatable<Hit>
    {
        public int Ident { get; set; }

        public DateTime Date { get; set ; }

        public string Instance { get; set; }

        public string Opponent { get; set; }

        public string BodyPart { get; set; }

        public int ImpactSpeed { get; set; }

        public double HitPoint { get; set; }

        public double HitConscience { get; set; }

        public string HitPointWidth { get; }

        public string DamageType { get; set; }

        public DirectionEnum Direction { get; set; }

        public Hit(int ident, String instance, DateTime date, String opponant, DirectionEnum direction, string bodyPart, string damageType, int impactSpeed, double hitPoint, double hitConscience)
        {
            Ident = ident;
            Instance = instance;
            Opponent = opponant;
            Date = date;
            Direction = direction;
            ImpactSpeed = impactSpeed;
            DamageType = damageType;
            HitPoint = hitPoint;
            HitPointWidth = Convert.ToString(HitPoint)+"*";
            BodyPart = bodyPart;
            HitConscience = hitConscience;
        }


        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum DirectionEnum
        {
            [Description("Reçu")]
            incoming = 0,
            [Description("Donné")]
            outgoing = 1
        }

        bool IEquatable<Hit>.Equals(Hit other)
        {
            return
                this.BodyPart == other.BodyPart &&
                this.Instance == other.Instance &&
                this.DamageType == other.DamageType &&
                this.Date == other.Date &&
                this.Direction == other.Direction &&
                this.HitPoint == other.HitPoint &&
                this.HitConscience == other.HitConscience &&
                this.Ident == other.Ident &&
                this.ImpactSpeed == other.ImpactSpeed &&
                this.Opponent == other.Opponent;
        }
    }
}
