namespace HttpStorehouse.Models
{
	public class ProductModel : IModel<int, string, string>
	{
		public int Key { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }

	}
}
