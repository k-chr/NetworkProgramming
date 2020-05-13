using System;
using System.Collections.Generic;
using System.Text;

namespace HttpStorehouse.Models
{
	public class StoreHouseModel : IModel<int, string>
	{
		public int Key { get; set; }
		public string Value { get; set; }

		private List<ProductModel> Models { get; set; } 
	}
}
