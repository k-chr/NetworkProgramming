using System;
using Avalonia.Media;


namespace NetworkProgramming.Lab2.Models
{
   public class InternalMessageModel
   {
      public string ExceptionData { get; }
      public string Data { get; }
      public DateTime Date { get; }
      public InternalMessageType Type { get; }
      public IBrush ColorBrush { get; }

      internal InternalMessageModel(MessageBuilder builder)
      {
         Data = builder.MessageData;
         ExceptionData = builder.ExceptionData;
         Date = builder.Date;
         Type = builder.Type;

         ColorBrush = Type switch
         {
            InternalMessageType.Error => Brush.Parse("Red"),
            InternalMessageType.Client => Brush.Parse("Cyan"),
            InternalMessageType.Server => Brush.Parse("DarkMagenta"),
            InternalMessageType.Info => Brush.Parse("DarkCyan"),
            InternalMessageType.Success => Brush.Parse("Green"),
            _ => Brush.Parse("Black")
         };
      }

      public static MessageBuilder Builder()
      {
         return new MessageBuilder();
      }

      public override string ToString()
      {
         var dateMsg = Date == DateTime.MinValue ? "" : $"[{Date.ToShortDateString()} {Date.ToLongTimeString()}]";
         return $"[{Type}]{dateMsg} {Data}\n{ExceptionData}";
      }

      public class MessageBuilder
      {
         internal MessageBuilder() { }

         internal string MessageData;
         internal string ExceptionData;
         internal InternalMessageType Type = InternalMessageType.Info;
         internal DateTime Date;

         public InternalMessageModel BuildMessage()
         {
            return new InternalMessageModel(this);
         }

         public MessageBuilder WithType(InternalMessageType type)
         {
            Type = type;
            return this;
         }

         public MessageBuilder AttachTimeStamp(bool value)
         {
            if (value)
            {
               Date = DateTime.Now;
            }

            return this;
         }

         public MessageBuilder AttachExceptionData(Exception e)
         {
            var msg = $"{e.Message}\n{e.StackTrace}\n{e.Source}";
            ExceptionData = msg;
            return this;
         }

         public MessageBuilder AttachTextMessage(string data)
         {
            MessageData = data;
            return this;
         }
      }

   }

   public enum InternalMessageType
   {
      Info, Error, Success, Client, Server
   }
}
