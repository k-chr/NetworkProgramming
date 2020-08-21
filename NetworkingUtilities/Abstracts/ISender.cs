namespace NetworkingUtilities.Abstracts
{
   public interface ISender
   {
	   void Send(byte[] data, string to="");
   }
}
