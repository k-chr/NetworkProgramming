using System;
using System.Collections.Generic;
using System.Text;

namespace HttpStorehouse.Models
{
	public interface IModel<K, V, D>
	{
		K Key { get;  set; }
		V Value { get; set; }
		D Description { get; set; }
	}
}
