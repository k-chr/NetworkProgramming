namespace HttpStorehouse.Models
{
	public interface IModel<TK, TV, TD>
	{
		TK Key { get; set; }
		TV Value { get; set; }
		TD Description { get; set; }
	}
}