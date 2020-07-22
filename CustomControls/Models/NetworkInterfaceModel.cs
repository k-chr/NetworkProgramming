namespace CustomControls.Models
{
	public class NetworkInterfaceModel
	{
		public string Name { get; set; }
		public string Ip { get; set; }

		public override string ToString()
		{
			return $"{Name} | {Ip}";
		}
	}
}