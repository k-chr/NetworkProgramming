using System;

namespace ThreadingUtilities.KamillimakThreading.Helpers
{
   public class StrRef
   {
      public string Value { get; set; }
      public string Name { get; }
      public static implicit operator StrRef(Tuple<string, string> tuple)
      {
         var (name, other) = tuple;
         return new StrRef(name) { Value = other };
      }

      public StrRef(string name)
      {
         Name = name;
      }
      public override string ToString()
      {
         return Value;
      }

      public StrRef Replace(string old, string @new)
      {
         return (this.Name, this.Value.Replace(old, @new)).ToTuple();
      }

   }
}